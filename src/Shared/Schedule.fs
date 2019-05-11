namespace Shared

module Schedule =

    open FSharp.Data

    open Team

    type ScheduleResult = { winner: Team; loser: Team }
    type Schedule = { team1: Team; team2: Team }
    type ScheduleResultJson = JsonProvider<""" [ {"winner":"TL", "loser":"C9" } ] """>
    type ScheduleJson = JsonProvider<""" [ {"team1":"TL", "team2":"C9" } ] """>