namespace Models


type WinLoss = { Wins: int; Losses: int }
type LcsResult = { Opponent: LcsTeam; Won: bool }
type TeamRecord = 
    { LcsTeam: LcsTeam
      WinLoss: WinLoss
      Results: LcsResult list }

[<RequireQualifiedAccess>]
module TeamRecord = 

    let private isTeamInGame teamCode event =
        event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) 

    let private createLcsResult teamCode event =
        let opposingTeam =
            event.Match.Teams
            |> List.find (fun team -> team.Code <> teamCode)

        let outcome =
            match opposingTeam.Result.Outcome with
            | "win" -> false
            | "loss" -> true
            | _ -> true
        
        { Opponent = LcsTeam.fromCode opposingTeam.Code; Won = outcome }

    let private createWinLoss teamCode events =
        match events with
        | [| |] -> 
            { Wins = 0; Losses = 0 }
        | _ -> 
            events
            |> Array.find (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode ) )
            |> fun event -> event.Match.Teams |> List.find (fun team -> team.Code = teamCode)
            |> fun team -> { Wins = team.Record.Wins ; Losses = team.Record.Losses }

    let private createLcsResults teamCode events =
        events
        |> Array.filter (isTeamInGame teamCode)
        |> Array.map (createLcsResult teamCode)
        |> Array.toList

    let create events team =
        let teamCode = LcsTeam.toCode team

        { LcsTeam = team
          WinLoss = createWinLoss teamCode events
          Results = createLcsResults teamCode events }