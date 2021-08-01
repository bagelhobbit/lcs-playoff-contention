namespace Teams

open Models
open LeagueScheduleJson

module LolTeam =

    let LolTeams league =
        LeagueSchedule.getSchedule league
        |> Array.map (fun event -> [| event.Match.Teams.[0]; event.Match.Teams.[1] |] )
        |> Array.collect id
        |> Array.map (fun team -> { Name = team.Name; Code = team.Code; Image = team.Image })
        |> Array.distinct