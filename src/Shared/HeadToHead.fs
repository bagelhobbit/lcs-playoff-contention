namespace Shared


type HeadToHeadResult = Win | Tie | Loss
type HeadToHead = { Team: Team; Result: HeadToHeadResult }

[<RequireQualifiedAccess>]
module HeadToHeads =

    let create team pastEvents =
        let teamCode = LcsTeam.toCode team

        let createHeadToHead event =
            let teamWon (team: Team) =
                match team.Result.Outcome with
                | "win" -> true
                | "loss" -> false
                | _ -> false

            let currentTeam, opposingTeam =
                let teams = event.Match.Teams
                teams |> List.partition (fun team -> team.Code = teamCode)

            if (teamWon currentTeam.Head)
            then { Team = opposingTeam.Head; Result = Win }
            else { Team = opposingTeam.Head; Result = Loss }


        let combine results headToHead =
            match results with
            | [] -> [headToHead]
            | x::xs when x.Team.Code = headToHead.Team.Code ->
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
        |> Array.filter (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) )
        |> Array.map createHeadToHead
        |> Array.sortBy (fun result -> result.Team.Code)
        |> Array.fold combine []

[<RequireQualifiedAccess>]
module HeadToHeadResult =

    let create team1 team2 pastEvents =
        let teamCode1 = LcsTeam.toCode team1
        let teamCode2 = LcsTeam.toCode team2

        let teamWon (team: Team) =
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
            |> Array.filter filterHeadToHeads
            |> Array.filter filterWinningTeams
            |> Array.length
           
        match headToHeadWins with
        | 2 -> Win
        | 1 -> Tie
        | 0 -> Loss
        | _ -> Win