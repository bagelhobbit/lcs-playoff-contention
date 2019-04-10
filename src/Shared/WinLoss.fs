namespace Shared

module WinLoss = 

    open Schedule

    type WinLoss = { wins: int; losses: int }
    type MatchResult = { opponent: string; won: bool } 
    type TeamRecord = { team: string; winLoss: WinLoss; results: MatchResult list }

    let private isTeamInGame team result =
        result.winner = team || result.loser = team

    let private createMatchResult team result =
        if result.winner = team 
        then { opponent=result.loser; won=true }
        else { opponent=result.winner; won=false }

    let generateWinLoss team pastResults =
        let (wins, losses) =
            pastResults
            |> Seq.filter (isTeamInGame team)
            |> Seq.toList
            |> List.partition (fun result -> result.winner = team)

        { wins = wins.Length; losses = losses.Length }

    let generateMatchResults team pastResults =
        pastResults
        |> Seq.filter (isTeamInGame team)
        |> Seq.map (createMatchResult team)
        |> Seq.toList

    
    let generateTeamRecord results team =
        { team = team
          winLoss = generateWinLoss team results
          results = generateMatchResults team results
        }