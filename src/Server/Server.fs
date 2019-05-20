open System.IO
open System.Threading.Tasks

open FSharp.Data

open FSharp.Control.Tasks.V2
open Saturn

open Shared.HeadToHead
open Shared.TeamRecord
open Shared

open EliminatedTeams
open PlayoffTeams

open Fable.Remoting.Server
open Fable.Remoting.Giraffe


let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let getApiSchedule =
    let naLeagueId = "98767991299243165"
    let apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

    let apiSite =
        // This should probably be rate limited somewhat based on last updated time
        Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getSchedule", httpMethod = "GET",
            query = [ "hl", "en-US"; "leagueId", naLeagueId],
            headers = [ "x-api-key", apiKey] )

    let schedule = LeagueEventsJson.Parse(apiSite).Data.Schedule

    let regularSeasonEvents =
        schedule.Events
        |> Array.filter (fun event -> event.BlockName.Contains "Week")

    // Total matches: 9 weeks * 10 games/week = 90 games
    if(regularSeasonEvents |> Array.length <> 90)
    then
        let oldEventsJson =
            Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getSchedule", httpMethod = "GET",
                query = [ "hl", "en-US"; "leagueId", naLeagueId; "pageToken", schedule.Pages.Older],
                headers = [ "x-api-key", apiKey] )

        // We should never need more than weeks 1 & 2
        // since that's the most that is missing after the LCS finals
        let oldEvents =
            LeagueEventsJson.Parse(oldEventsJson).Data.Schedule.Events
            |> Array.filter (fun event -> event.BlockName = "Week 1" || event.BlockName = "Week 2")

        Array.append regularSeasonEvents oldEvents
        |> Array.sortBy (fun event -> event.StartTime)
    else
        regularSeasonEvents
        |> Array.sortBy (fun event -> event.StartTime)

let getCurrentRecords() : Task<TeamRecord list> =
    task {
        let lcsTeams = [C9; CG; CLG; FOX; FLY; GGS; OPT; Thieves; TL; TSM]

        // This should probably not be converted
        // Just let type provider provide types
        // But I wanted to delcare types for debugging/conversion

        // Conversion should move to schedule file, which should be refactored as well
        let lcsResults = 
            getApiSchedule
            |> Array.filter (fun event -> event.State = Schedule.StateCompleted)
            |> Array.map (fun event -> 
                { StartTime = event.StartTime
                  State = event.State
                  Type = event.Type
                  BlockName = event.BlockName
                  League =
                    { Name = event.League.Name
                      Slug = event.League.Slug  }
                  Match =
                    { Id = event.Match.Id
                      Teams = 
                        event.Match.Teams
                        |> Array.map (fun team ->
                            { Name = team.Name
                              Code = team.Code
                              Result = 
                                { Outcome = team.Result.Outcome
                                  GameWins = team.Result.GameWins }
                              Record = 
                                { Wins = team.Record.Wins
                                  Losses = team.Record.Losses } } )
                        |> List.ofArray
                      Strategy =
                        { Type = event.Match.Strategy.Type
                          Count = event.Match.Strategy.Count } } } )

        let descendingComparer team1 team2 =
            // 1 - x > y; 0 - x = y; -1 - x < y
            // Reverse the comparer so teams with a better head to head record are at the top
            if team1.WinLoss = team2.WinLoss
            then 
                let headToHead = generateHeadToHeadResult team1.Team team2.Team lcsResults

                match headToHead with
                | Win -> -1
                | Tie -> 0
                | Loss -> 1
            else 0

        let currentRecords =
            lcsTeams
            |> List.map (generateTeamRecord lcsResults)
            |> List.sortByDescending (fun record -> record.WinLoss.Wins)
            |> List.sortWith descendingComparer

        return currentRecords 
    }

let getLcsPlayoffStatuses teamRecords : Task<(LcsTeam * PlayoffStatus) list> =
    task {
        let remainingSchedule =
            getApiSchedule
            |> Array.filter (fun event -> event.State = Schedule.StateUnstarted)
            |> Array.map (fun event -> 
                { StartTime = event.StartTime
                  State = event.State
                  Type = event.Type
                  BlockName = event.BlockName
                  League =
                    { Name = event.League.Name
                      Slug = event.League.Slug  }
                  Match =
                    { Id = event.Match.Id
                      Teams = 
                        event.Match.Teams
                        |> Array.map (fun team ->
                            { Name = team.Name
                              Code = team.Code
                              Result = 
                                { Outcome = team.Result.Outcome
                                  GameWins = team.Result.GameWins }
                              Record = 
                                { Wins = team.Record.Wins
                                  Losses = team.Record.Losses } } )
                        |> List.ofArray
                      Strategy =
                        { Type = event.Match.Strategy.Type
                          Count = event.Match.Strategy.Count } } } )

        let eliminatedTeams = 
            findEliminatedTeams teamRecords remainingSchedule
            |> List.map (fun team -> team.Team)

        let playoffTeams =
            findPlayoffTeams teamRecords
            |> List.map (fun team -> team.Team)

        let playoffByes =
            findPlayoffByes teamRecords
            |> List.map (fun team -> team.Team)

        let assignPlayoffStatus team =
            let containsTeam =
                [ eliminatedTeams; playoffTeams; playoffByes ]
                |> List.map (List.contains team.Team)

            match containsTeam with
            | _::_::[x] when x -> (team.Team, Bye)
            | _::x::_ when x -> (team.Team, Clinched)
            | x::_ when x -> (team.Team, Eliminated)
            | _ -> (team.Team, Unknown)

        return teamRecords
        |> List.map assignPlayoffStatus
    }

let getHeadToHeads team : Task<HeadToHead list> =
    task {
        let lcsResults = 
            getApiSchedule
            |> Array.filter (fun event -> event.State = Schedule.StateCompleted)
            |> Array.map (fun event -> 
                { StartTime = event.StartTime
                  State = event.State
                  Type = event.Type
                  BlockName = event.BlockName
                  League =
                    { Name = event.League.Name
                      Slug = event.League.Slug  }
                  Match =
                    { Id = event.Match.Id
                      Teams = 
                        event.Match.Teams
                        |> Array.map (fun team ->
                            { Name = team.Name
                              Code = team.Code
                              Result = 
                                { Outcome = team.Result.Outcome
                                  GameWins = team.Result.GameWins }
                              Record = 
                                { Wins = team.Record.Wins
                                  Losses = team.Record.Losses } } )
                        |> List.ofArray
                      Strategy =
                        { Type = event.Match.Strategy.Type
                          Count = event.Match.Strategy.Count } } } )

        return generateHeadToHeads team lcsResults
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
