namespace LeagueScheduleJson

open FSharp.Data

open Shared


type LeagueSchedule = JsonProvider<"C:\Users\Evan\Documents\code\F#\PlayoffContentionWeb\src\server\eventBasic.json">


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