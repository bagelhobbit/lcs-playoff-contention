namespace Shared

module TeamRecord = 

    open Schedule
    open Team

    type WinLoss = { Wins: int; Losses: int }
    type MatchResult = { Opponent: Team; Won: bool }
    type TeamRecord = 
        { team: Team
          winLoss: WinLoss
          results: MatchResult list
        }

    let private createTeam = function
        | "100" -> Thieves
        | "C9" -> C9
        | "CG" -> CG
        | "CLG" -> CLG
        | "FOX" -> FOX
        | "FQ" -> FQ
        | "GGS" -> GGS
        | "OPT" -> OPT
        | "TL" -> TL
        | "TSM" -> TSM
        | _ -> Team.Unknown

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
        
        { Opponent = createTeam opposingTeam.Code; Won = outcome }

    let private generateWinLoss teamCode events =
        events
        |> Seq.find (fun event -> event.Match.Teams |> Seq.exists (fun team -> team.Code = teamCode ) )
        |> fun event -> event.Match.Teams |> List.find (fun team -> team.Code = teamCode)
        |> fun team -> { Wins = team.Record.Wins ; Losses = team.Record.Losses }

    let private generateMatchResults teamCode events =
        events
        |> Seq.filter (isTeamInGame teamCode)
        |> Seq.map (createMatchResult teamCode)
        |> Seq.toList

    let generateTeamRecord events team =
        let teamCode = toCode team

        { team = team
          winLoss = generateWinLoss teamCode events
          results = generateMatchResults teamCode events
        }