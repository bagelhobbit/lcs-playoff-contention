namespace Shared

module HeadToHead =

    open Team
    open Schedule

    type HeadToHeadResult = Win | Tie | Loss
    type HeadToHead = { Team: Team; Result: HeadToHeadResult }

    let generateHeadToHeads team pastEvents =
        let teamCode = toCode team

        let createHeadToHead event =
            let teamWon (team: Schedule.Team) =
                match team.Result.Outcome with
                | "win" -> true
                | "loss" -> false
                | _ -> false

            let currentTeam, opposingTeam =
                let teams = event.Match.Teams
                teams |> List.partition (fun team -> team.Code = teamCode)

            if (teamWon currentTeam.Head)
            then { Team = opposingTeam.Head; Result = Win }
            else { Team = currentTeam.Head; Result = Loss }


        let combine results headToHead =
            match results with
            | [] -> [headToHead]
            | x::xs when x.Team = headToHead.Team ->
                match (x.Result, headToHead.Result) with
                | (Win, Loss) ->  { Team=x.Team; Result=Tie }::xs
                | (Win, _) ->  { Team=x.Team; Result=Win }::xs
                | (Loss, Win) ->  { Team=x.Team; Result=Tie }::xs
                | (Loss, _) ->  { Team=x.Team; Result=Loss }::xs
                | (Tie, Win) ->  { Team=x.Team; Result=Win }::xs
                | (Tie, Loss) ->  { Team=x.Team; Result=Loss }::xs
                | (Tie, Tie) ->  { Team=x.Team; Result=Tie }::xs
            | xs -> headToHead::xs

        pastEvents
        |> Seq.filter (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) )
        |> Seq.map createHeadToHead
        |> Seq.sortBy (fun result -> result.Team)
        |> Seq.fold combine []

    let generateHeadToHeadResult team1 team2 pastEvents =
        let teamCode1 = toCode team1
        let teamCode2 = toCode team2

        let teamWon (team: Schedule.Team) =
            match team.Result.Outcome with
            | "win" -> true
            | "loss" -> false
            | _ -> false

        let filterHeadToHeads event =
            event.Match.Teams 
            |> List.forall (fun team -> team.Code = teamCode1 || team.Code = teamCode2) 

        let filterWinningTeams event =
            event.Match.Teams 
            |> List.exists (fun team -> team.Code = teamCode1 && (teamWon team)) 
        
        let headToHeadWins =
            pastEvents
            |> Seq.filter filterHeadToHeads
            |> Seq.filter filterWinningTeams
            |> Seq.length
           
        match headToHeadWins with
        | 2 -> Win
        | 1 -> Tie
        | 0 -> Loss
        | _ -> Win