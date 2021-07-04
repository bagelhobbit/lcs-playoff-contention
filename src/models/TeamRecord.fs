namespace Models


type WinLoss = { Wins: int; Losses: int }
type LcsResult = { Opponent: LcsTeam; Won: bool }
type TeamRecord = 
    { LcsTeam: LcsTeam
      WinLoss: WinLoss
      SplitWinLoss: WinLoss
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
        let findTeamInEvent = function
            | Some e -> e.Match.Teams |> List.tryFind (fun team -> team.Code = teamCode)
            | None -> None

        let createWinLossFromOption = function
            | Some t -> { Wins = t.Record.Wins ; Losses = t.Record.Losses }
            | None -> { Wins = 0; Losses = 0 }    

        match events with
        | [| |] -> 
            { Wins = 0; Losses = 0 }
        | _ ->
            events
            |> Array.tryFind ( fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode ) )
            |> findTeamInEvent
            |> createWinLossFromOption

    let private createLcsResults teamCode events =
        events
        |> Array.filter (isTeamInGame teamCode)
        |> Array.map (createLcsResult teamCode)
        |> Array.toList

    let private createSplitWinLoss teamCode events =
        events
        |> Array.filter (isTeamInGame teamCode)
        |> Array.map (createLcsResult teamCode)
        |> Array.partition (fun (result) -> result.Won)
        |> fun (wins, lossess) -> { Wins = wins |> Array.length; Losses = lossess |> Array.length }

    let create events team =
        let teamCode = LcsTeam.toCode team

        { LcsTeam = team
          WinLoss = createWinLoss teamCode events
          SplitWinLoss = createSplitWinLoss teamCode events
          Results = createLcsResults teamCode events }