open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared
open Shared.Schedule
open Shared.TeamRecord

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open EliminatedTeams
open PlayoffTeams

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

        let currentRecords = 
            lcsTeams
            |> List.map (generateTeamRecord getLcsResults)
            |> List.sortByDescending (fun record -> record.winLoss.wins)

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
            if eliminatedTeams |> List.contains team.team
            then (team.team, PlayoffStatus.Eliminated)
            else 
                if playoffByes |> List.contains team.team
                then (team.team, PlayoffStatus.Bye)
                else
                    if playoffTeams |> List.contains team.team
                    then (team.team, PlayoffStatus.Clinched)
                    else (team.team, PlayoffStatus.Unknown)

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
