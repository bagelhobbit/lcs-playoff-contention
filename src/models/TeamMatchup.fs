namespace Models

type TeamMatchup =
    { Team : string
      TeamCode : string
      Matchups : Matchup list }


module TeamMatchups =

    let create matchups team =
        {
            Team = team.Name
            TeamCode = team.Code
            Matchups = matchups
        }

    let toJson matchups =
        let contents =
            let fold state str =
                if state = "" 
                then str 
                else state + ", " + str

            let toTeamMatchupJson matchup =
                let matchups =
                    matchup.Matchups
                    |> List.sortBy ( fun m -> m.Team.Name )
                    |> List.map Matchups.toJson
                    |> List.fold fold ""

                sprintf "{ \"team\" : \"%s\", \"teamCode\" : \"%s\", \"matchups\" : [%s] }" matchup.Team matchup.TeamCode matchups

            matchups
            |> Array.sortBy ( fun m -> m.Team )
            |> Array.map toTeamMatchupJson
            |> Array.fold fold ""

        "[" + contents + "]"