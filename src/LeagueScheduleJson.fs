namespace LeagueScheduleJson

open FSharp.Data

open Models
open LeagueTournamentJson
open System


type LeagueSchedule = JsonProvider<"src/json/schedule.json", SampleIsList=true>

type League = LCS | LEC | LPL | LCK


[<RequireQualifiedAccess>]
module LeagueSchedule =


    [<Literal>]
    let StateCompleted = "completed"

    [<Literal>]
    let StateUnstarted = "unstarted"

    let mutable private _lastLeague = LCS

    let mutable private _lastUpdated = DateTime.MinValue

    let mutable private _schedule = None

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

    let private getScheduleJson league pageToken =
        let lcsLeagueId = "98767991299243165"
        let lecLeagueId = "98767991302996019"

        let leagueId = 
            match league with
            | LEC -> lecLeagueId
            | _ -> lcsLeagueId

        let apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

        let query = 
            let baseQuery = ["hl", "en-US"; "leagueId", leagueId]

            match pageToken with
            | Some token -> baseQuery @ ["pageToken", token]
            | None -> baseQuery

        Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getSchedule", httpMethod = "GET",
            query = query,
            headers = [ "x-api-key", apiKey] )

    let getSchedule league =
        let schedule = 
            match _schedule with
            | None ->
                let schedule = 
                    let scheduleJson = getScheduleJson league None
                    LeagueSchedule.Parse(scheduleJson).Data.Schedule 
                _schedule <- Some schedule
                _lastLeague <- league
                _lastUpdated <- DateTime.Now
                schedule
            | Some schedule ->
                if (DateTime.Now - _lastUpdated).Minutes > 5 || _lastLeague <> league 
                then
                    let schedule = 
                        let scheduleJson = getScheduleJson league None
                        LeagueSchedule.Parse(scheduleJson).Data.Schedule 
                    _schedule <- Some schedule
                    _lastLeague <- league
                    _lastUpdated <- DateTime.Now
                    schedule
                else 
                    schedule

        let regularSeasonFilter (event: LeagueSchedule.Event) =
            event.StartTime.Date >= LeagueTournament.mostRecentTournament.StartDate.Date &&
            event.BlockName.Contains "Week"

        let regularSeasonEvents =
            schedule.Events
            |> Array.filter regularSeasonFilter

        // Total matches: 9 weeks * 10 games/week = 90 games
        if(regularSeasonEvents |> Array.length <> 90)
        then
            let containsWeek1 = 
                let numWeek1Games =
                    regularSeasonEvents 
                    |> Array.filter (fun x -> x.BlockName = "Week 1") 
                    |> Array.length

                numWeek1Games = 10

            let page =
                match schedule.Pages.Newer, containsWeek1 with
                | Some s, true -> s
                | Some _, false -> schedule.Pages.Older
                | None, _ -> schedule.Pages.Older

            let extraEventsJson = getScheduleJson league <| Some page

            let extraEvents =
                LeagueSchedule.Parse(extraEventsJson).Data.Schedule.Events
                |> Array.filter regularSeasonFilter

            Array.append regularSeasonEvents extraEvents
            |> Array.sortBy (fun event -> event.StartTime)
        else
            regularSeasonEvents
            |> Array.sortBy (fun event -> event.StartTime)