module EliminatedTeams

open Shared
open Shared.Schedule
open Shared.TeamRecord

let findEliminatedTeams teamRecords futureSchedule =
    let remainingGames =
        let currentGames = List.head teamRecords |> (fun x -> x.winLoss.wins + x.winLoss.losses)
        Constants.totalLcsGames - currentGames

    let minimunRequiredWins =
        let minWins =
            teamRecords
            |> List.sortByDescending (fun team -> team.winLoss.wins)
            |> List.item 5
            |> (fun team -> team.winLoss.wins)

        let unpairwise x =
            match x with
            | (a,b)::t -> [a;b] @ [ for (_,b) in t -> b]
            | _ -> []

        let tiedPotentialContenders =
            teamRecords
            |> List.filter (fun team -> team.winLoss.wins = minWins)
            |> List.pairwise
            |> List.filter (fun (team1, team2) -> Seq.contains {team1=team1.team; team2=team2.team} futureSchedule)
            |> unpairwise
            |> List.map (fun team -> team.team)

        match tiedPotentialContenders with
        | [] -> minWins
        | [_] -> minWins
        | _ -> minWins + 1

    teamRecords
    |> List.filter (fun team -> team.winLoss.wins + remainingGames < minimunRequiredWins)