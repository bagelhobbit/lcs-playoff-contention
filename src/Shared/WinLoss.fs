namespace Shared

module WinLoss = 

    open Schedule

    type WinLoss = {wins: int; losses: int}
    type TeamRecord = {team: string; winLoss: WinLoss}

    let generateWinLoss team pastResults=
        let (wins, losses) =
            pastResults
            |> Seq.filter (fun result -> result.winner = team || result.loser = team)
            |> Seq.toList
            |> List.partition (fun result -> result.winner = team)

        {wins = wins.Length; losses = losses.Length}