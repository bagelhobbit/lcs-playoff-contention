module EliminatedTeams

open Models

let findEliminatedTeams teamRecords futureSchedule =
    let remainingGames =
        let currentGames = List.head teamRecords |> (fun x -> x.WinLoss.Wins + x.WinLoss.Losses)
        Constants.totalLcsGames - currentGames

    let minimunRequiredWins =
        let minWins =
            teamRecords
            |> List.sortByDescending (fun team -> team.WinLoss.Wins)
            |> List.item 5 // 6th place
            |> (fun team -> team.WinLoss.Wins)

        let filterFutureSchedule team1 team2 event =
            let teamCode1 = team1.Code
            let teamCode2 = team2.Code

            event.Match.Teams
            |> List.map (fun team -> team.Code)
            |> fun teams -> teams = [ teamCode1; teamCode2 ] || teams = [ teamCode2; teamCode1 ]

        let unpairwise x =
            match x with
            | (a,b)::t -> [a;b] @ [ for (_,b) in t -> b]
            | _ -> []

        let tiedPotentialContenders =
            teamRecords
            |> List.filter (fun team -> team.WinLoss.Wins = minWins)
            |> List.pairwise
            |> List.filter (fun (team1, team2) -> Seq.exists (filterFutureSchedule team1.LolTeam team2.LolTeam) futureSchedule)
            |> unpairwise
            |> List.map (fun team -> team.LolTeam)

        match tiedPotentialContenders with
        | [] -> minWins
        | [_] -> minWins + 1
        | _ -> minWins

    // To be eliminated from playoffs a team needs to not be able to win enough games to enter the top 6 
    // assuming they win every game, and any challenger loses all their remaining games
    // teamWins + remainingGames < 6thWins
    teamRecords
    |> List.filter (fun team -> team.WinLoss.Wins + remainingGames < minimunRequiredWins)