namespace Shared

module Schedule =

    open System
    open FSharp.Data

    type League = 
        { Name: string
          Slug: string }

    type MatchResult =
        { Outcome: string
          GameWins: int }

    type Record =
        { Wins: int
          Losses: int }

    type Team =
        { Name: string
          Code: string
          Result: MatchResult
          Record: Record }

    type Strategy =
        { Type: string
          Count: int }

    type Match =
        { Id: int64 
          Teams: Team list
          Strategy: Strategy }

    type EventState = Completed | Unstarted

    type LeagueEvent =
        { StartTime: DateTimeOffset
          State: string //EventState
          Type: string
          BlockName: string
          League: League
          Match: Match }

    type LeagueEventsJson = JsonProvider<"C:\Users\Evan\Documents\code\F#\PlayoffContentionWeb\src\server\eventBasic.json">

    let private naLeagueId = "98767991299243165"
    let private apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

    let apiSite = 
        Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getSchedule", httpMethod = "GET",
            query = [ "hl", "en-US"; "leagueId", naLeagueId],
            headers = [ "x-api-key", apiKey] )