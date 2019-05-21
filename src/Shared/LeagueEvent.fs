namespace Shared

open System
open FSharp.Data


type EventState = Completed | Unstarted

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

type LeagueEvent =
    { StartTime: DateTimeOffset
      State: EventState
      Type: string
      BlockName: string
      League: League
      Match: Match }

type LeagueEventsJson = JsonProvider<"C:\Users\Evan\Documents\code\F#\PlayoffContentionWeb\src\server\eventBasic.json">


[<RequireQualifiedAccess>]
module LeagueEvent =

    [<Literal>]
    let StateCompleted = "completed"

    [<Literal>]
    let StateUnstarted = "unstarted"

    // Fable can't use the type provider,
    // so we need to use actual types
    let create (event: LeagueEventsJson.Event) =

        let createEventState = function
            | "completed" -> Completed
            | _ -> Unstarted

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