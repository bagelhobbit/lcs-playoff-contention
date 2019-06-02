namespace LeagueScheduleJson

open FSharp.Data

open Shared
open LeagueTournamentJson


type LeagueSchedule = JsonProvider<"scheduleBasic.json">


[<RequireQualifiedAccess>]
module LeagueSchedule =


    [<Literal>]
    let StateCompleted = "completed"

    [<Literal>]
    let StateUnstarted = "unstarted"

    // Fable can't use the type provider,
    // so we need to use actual types
    let create (event: LeagueSchedule.Event) =

        let createEventState = function
            | "completed" -> EventState.Completed
            | _ -> EventState.Unstarted

        { StartTime = event.StartTime
          State = createEventState event.State
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
                  Count = event.Match.Strategy.Count } } }

    let private getScheduleJson pageToken =
        let naLeagueId = "98767991299243165"
        let apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

        let query = 
            let baseQuery = ["hl", "en-US"; "leagueId", naLeagueId]

            match pageToken with
            | Some token -> baseQuery @ ["pageToken", token]
            | None -> baseQuery

        Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getSchedule", httpMethod = "GET",
            query = query,
            headers = [ "x-api-key", apiKey] )

    let getSchedule =
        let schedule =
            let scheduleJson = getScheduleJson None
            LeagueSchedule.Parse(scheduleJson).Data.Schedule

        let regularSeasonFilter (event: LeagueSchedule.Event) =
            event.StartTime.Date >= LeagueTournament.mostRecentTournament.StartDate.Date &&
            event.BlockName.Contains "Week"

        let regularSeasonEvents =
            schedule.Events
            |> Array.filter regularSeasonFilter

        // Total matches: 9 weeks * 10 games/week = 90 games
        if(regularSeasonEvents |> Array.length <> 90)
        then
            let oldEventsJson = getScheduleJson <| Some schedule.Pages.Older

            let oldEvents =
                LeagueSchedule.Parse(oldEventsJson).Data.Schedule.Events
                |> Array.filter regularSeasonFilter

            Array.append regularSeasonEvents oldEvents
            |> Array.sortBy (fun event -> event.StartTime)
        else
            regularSeasonEvents
            |> Array.sortBy (fun event -> event.StartTime)