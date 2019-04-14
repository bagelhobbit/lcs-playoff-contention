module HeadToHead

open Shared.Schedule

type HeadToHeadResult = Loss | Tie | Win
type HeadToHead = { team: string; result: HeadToHeadResult }

let generateHeadToHeads team pastResults =
    let createHeadToHeadResult result =
        if(result.winner = team)
        then {team=result.loser; result=Win}
        else {team=result.winner; result=Loss}

    let combine (results: HeadToHead list) (headToHead:HeadToHead) =
        match results with
        | [] -> [headToHead]
        | x::xs when x.team = headToHead.team ->
            match (x.result, headToHead.result) with
            | (Win, Loss) ->  {team=x.team; result=Tie}::xs
            | (Win, _) ->  {team=x.team; result=Win}::xs
            | (Loss, Win) ->  {team=x.team; result=Tie}::xs
            | (Loss, _) ->  {team=x.team; result=Loss}::xs
            | (Tie, Win) ->  {team=x.team; result=Win}::xs
            | (Tie, Loss) ->  {team=x.team; result=Loss}::xs
            | (Tie, Tie) ->  {team=x.team; result=Tie}::xs
        | xs -> headToHead::xs

    pastResults
    |> List.filter (fun result -> result.winner = team || result.loser = team)
    |> List.map createHeadToHeadResult
    |> List.sortBy (fun result -> result.team)
    |> List.fold combine []

let generateHeadToHeadResult team1 team2 pastResults =
    let filterTeams result =
        (result.winner = team1 && result.loser = team2)
        || (result.winner = team2 && result.loser = team1)
    
    let headToHeadWins =
        pastResults
        |> Seq.filter filterTeams
        |> Seq.filter (fun result -> result.winner = team1)
        |> Seq.length
       
    match headToHeadWins with
    | 2 -> Win
    | 1 -> Tie
    | 0 -> Loss
    | _ -> Win