open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn

open Shared
open Shared.Schedule
open HeadToHead
open Shared.TeamRecord
open EliminatedTeams
open PlayoffTeams

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let totalLcsGames = 18

let getLcsResults = 
    let resultsFile = File.ReadAllText @"lcs_results.json"
    ScheduleResultJson.Parse(resultsFile) |> Seq.map (fun game -> {winner=game.Winner; loser=game.Loser})

let getRemainingLcsSchedule = 
    let scheduleFile = File.ReadAllText @"lcs_remaining_schedule.json"
    ScheduleJson.Parse(scheduleFile) |> Seq.map (fun game -> {team1=game.Team1; team2=game.Team2})

let getCurrentRecords() : Task<TeamRecord list> =
    task {
        let lcsTeams = ["100"; "C9"; "CG"; "CLG"; "FOX"; "FQ"; "GGS"; "OPT"; "TL"; "TSM"]

        let lcsResults = getLcsResults

        let descendingComparer team1 team2 =
            // 1 - x > y; 0 - x = y; -1 - x < y
            // Reverse the comparer so teams with a better head to head record are at the top
            if team1.winLoss = team2.winLoss
            then 
                let headToHead = generateHeadToHeadResult team1.team team2.team lcsResults

                match headToHead with
                | Win -> -1
                | Tie -> 0
                | Loss -> 1
            else 0

        let currentRecords = 
            lcsTeams
            |> List.map (generateTeamRecord lcsResults)
            |> List.sortByDescending (fun record -> record.winLoss.wins)
            |> List.sortWith descendingComparer

        return currentRecords 
    }

let getLcsPlayoffStatuses teamRecords : Task<(string * PlayoffStatus) list> =
    task {
        let eliminatedTeams = 
            findEliminatedTeams teamRecords getRemainingLcsSchedule totalLcsGames 
            |> List.map (fun team -> team.team)

        let playoffTeams =
            findPlayoffTeams teamRecords totalLcsGames
            |> List.map (fun team -> team.team)

        let playoffByes =
            findPlayoffByes teamRecords totalLcsGames
            |> List.map (fun team -> team.team)

        let assignPlayoffStatus team =
            let containsTeam =
                [ eliminatedTeams; playoffTeams; playoffByes ]
                |> List.map (List.contains team.team)

            match containsTeam with
            | _::_::[x] when x -> (team.team, Bye)
            | _::x::_ when x -> (team.team, Clinched)
            | x::_ when x -> (team.team, Eliminated)
            | _ -> (team.team, Unknown)

        return teamRecords
        |> List.map assignPlayoffStatus
    }


let playoffApi = {
    lcsTeamRecords = getCurrentRecords >> Async.AwaitTask
    lcsPlayoffStatuses = getLcsPlayoffStatuses >> Async.AwaitTask
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
