namespace Models

type TeamMatchup =
    { League: League
      Team : string
      TeamCode : string
      Matchups : Matchup list }


module TeamMatchups =

    let create league matchups team =
        {
            League = league
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

                sprintf "{ \"league\" : \"%A\", \"team\" : \"%s\", \"teamCode\" : \"%s\", \"matchups\" : [%s] }" matchup.League matchup.Team matchup.TeamCode matchups

            matchups
            |> Array.sortBy ( fun m -> m.Team )
            |> Array.map toTeamMatchupJson
            |> Array.fold fold ""

        "[" + contents + "]"