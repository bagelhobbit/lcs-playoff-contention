namespace Shared

module TeamRecord = 

    open Shared

    type WinLoss = { Wins: int; Losses: int }
    type MatchResult = { Opponent: LcsTeam; Won: bool }
    type TeamRecord = 
        { Team: LcsTeam
          WinLoss: WinLoss
          Results: MatchResult list }

    let private isTeamInGame teamCode event =
        event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) 

    let private createMatchResult teamCode event =
        let opposingTeam =
            event.Match.Teams
            |> List.find (fun team -> team.Code <> teamCode)

        let outcome =
            match opposingTeam.Result.Outcome with
            | "win" -> false
            | "loss" -> true
            | _ -> true
        
        { Opponent = LcsTeam.create opposingTeam.Code; Won = outcome }

    let private generateWinLoss teamCode events =
        events
        |> Array.find (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode ) )
        |> fun event -> event.Match.Teams |> List.find (fun team -> team.Code = teamCode)
        |> fun team -> { Wins = team.Record.Wins ; Losses = team.Record.Losses }

    let private generateMatchResults teamCode events =
        events
        |> Array.filter (isTeamInGame teamCode)
        |> Array.map (createMatchResult teamCode)
        |> Array.toList

    let generateTeamRecord events team =
        let teamCode = LcsTeam.toCode team

        { Team = team
          WinLoss = generateWinLoss teamCode events
          Results = generateMatchResults teamCode events }