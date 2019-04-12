namespace Shared

module Schedule =
    open FSharp.Data

    type ScheduleResult = {winner: string; loser: string}
    type Schedule = {team1: string; team2: string}
    type ScheduleResultJson = JsonProvider<""" [ {"winner":"TL", "loser":"C9" } ] """>
    type ScheduleJson = JsonProvider<""" [ {"team1":"TL", "team2":"C9" } ] """>