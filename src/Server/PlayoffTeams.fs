module PlayoffTeams

open Shared

let findPlayoffTeams teamRecords =
    let remainingGames =
        let currentGames = List.head teamRecords |> (fun x -> x.WinLoss.Wins + x.WinLoss.Losses)
        Constants.totalLcsGames - currentGames

    let minimunRequiredWins =
        let minWins =
            teamRecords
            |> List.sortByDescending (fun team -> team.WinLoss.Wins)
            |> List.item 5
            |> (fun team -> team.WinLoss.Wins)

        let tiedPotentialContenders =
            teamRecords
            |> List.filter (fun team -> team.WinLoss.Wins = minWins)

        match tiedPotentialContenders with
        | [] -> minWins
        //| [_] -> minWins - 1
        | _ -> 18

    teamRecords
    |> List.filter (fun team -> team.WinLoss.Wins + remainingGames > minimunRequiredWins)

let findPlayoffByes teamRecords =
    let remainingGames =
        let currentGames = List.head teamRecords |> (fun x -> x.WinLoss.Wins + x.WinLoss.Losses)
        Constants.totalLcsGames - currentGames

    if remainingGames = 0
    then
        teamRecords
        |> List.sortByDescending (fun team -> team.WinLoss.Wins)
        |> List.take 2
    else
        let minimunRequiredWins =
            let minWins =
                teamRecords
                |> List.maxBy (fun team -> team.WinLoss.Wins)
                |> (fun team -> team.WinLoss.Wins)

            let tiedPotentialContenders =
                teamRecords
                |> List.filter (fun team -> team.WinLoss.Wins = minWins)

            match tiedPotentialContenders with
            | [] -> minWins
            | [_] -> minWins
            | _ -> 18

        teamRecords
        |> List.filter (fun team -> team.WinLoss.Wins + remainingGames > minimunRequiredWins)