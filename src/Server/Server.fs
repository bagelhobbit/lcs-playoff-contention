open System.IO
open System.Threading.Tasks

open FSharp.Data

open FSharp.Control.Tasks.V2
open Saturn

open Shared

open LeagueScheduleJson
open EliminatedTeams
open PlayoffTeams

open Fable.Remoting.Server
open Fable.Remoting.Giraffe


let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let getCurrentRecords() : Task<TeamRecord list> =
    task {
        let lcsTeams = LcsTeam.lcsTeams

        let lcsResults = 
            LeagueSchedule.getSchedule
            |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
            |> Array.map LeagueSchedule.create

        let descendingComparer team1 team2 =
            // 1 - x > y; 0 - x = y; -1 - x < y
            // Reverse the comparer so teams with a better head to head record are at the top
            if team1.WinLoss = team2.WinLoss
            then 
                let headToHead = HeadToHeadResult.create team1.LcsTeam team2.LcsTeam lcsResults

                match headToHead with
                | Win -> -1
                | Tie -> 0
                | Loss -> 1
            else 0

        let currentRecords =
            lcsTeams
            |> List.map (TeamRecord.create lcsResults)
            |> List.sortByDescending (fun record -> record.WinLoss.Wins)
            |> List.sortWith descendingComparer

        return currentRecords 
    }

let getLcsPlayoffStatuses teamRecords : Task<(LcsTeam * PlayoffStatus) list> =
    task {
        let remainingSchedule =
            LeagueSchedule.getSchedule
            |> Array.filter (fun event -> event.State = LeagueSchedule.StateUnstarted)
            |> Array.map LeagueSchedule.create

        let eliminatedTeams = 
            findEliminatedTeams teamRecords remainingSchedule
            |> List.map (fun team -> team.LcsTeam)

        let playoffTeams =
            findPlayoffTeams teamRecords
            |> List.map (fun team -> team.LcsTeam)

        let playoffByes =
            findPlayoffByes teamRecords
            |> List.map (fun team -> team.LcsTeam)

        let assignPlayoffStatus team =
            let containsTeam =
                [ eliminatedTeams; playoffTeams; playoffByes ]
                |> List.map (List.contains team.LcsTeam)

            match containsTeam with
            | _::_::[x] when x -> (team.LcsTeam, Bye)
            | _::x::_ when x -> (team.LcsTeam, Clinched)
            | x::_ when x -> (team.LcsTeam, Eliminated)
            | _ -> (team.LcsTeam, Unknown)

        return teamRecords
        |> List.map assignPlayoffStatus
    }

let getHeadToHeads team : Task<HeadToHead list> =
    task {
        let lcsResults = 
            LeagueSchedule.getSchedule
            |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
            |> Array.map LeagueSchedule.create

        return HeadToHeads.create team lcsResults
    }

let playoffApi = {
    lcsTeamRecords = getCurrentRecords >> Async.AwaitTask
    lcsPlayoffStatuses = getLcsPlayoffStatuses >> Async.AwaitTask
    teamHeadToHeadRecords = getHeadToHeads >> Async.AwaitTask
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue playoffApi
    |> Remoting.buildHttpHandler

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
}

run app
