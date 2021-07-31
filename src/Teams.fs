namespace Teams

open System
open Models
open LeagueScheduleJson

module LolTeam =

    let mutable private _lastUpdated = DateTime.MinValue

    let mutable private _teams = None

    let private updateTeams =
        _lastUpdated <- DateTime.Now

        let updatedTeams =
            LeagueSchedule.getSchedule()
            |> Array.map (fun event -> [| event.Match.Teams.[0]; event.Match.Teams.[1] |] )
            |> Array.collect id
            |> Array.map (fun team -> { Name = team.Name; Code = team.Code; Image = team.Image })
            |> Array.distinct
        
        _teams <- Some updatedTeams
        updatedTeams

    let LcsTeams =
        match _teams with
        | None -> updateTeams
        | Some teams -> 
            // Teams don't change very often, so cache for roughly 3 months
            if (DateTime.Now - _lastUpdated).Days >= 90 then
                updateTeams
            else
                teams
