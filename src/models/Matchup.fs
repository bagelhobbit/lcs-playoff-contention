namespace Models

open DotLiquid

type MatchupResult = 
    Win | Tie | Loss

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | Win -> "Win" :> obj
            | Tie -> "Tie" :> obj
            | Loss -> "Loss" :> obj

type Matchup = { Team: Team; Result: MatchupResult }


[<RequireQualifiedAccess>]
module Matchups =

    let create team pastEvents =
        let teamCode = LcsTeam.toCode team

        let createMatchup event =
            let teamWon (team: Team) =
                match team.Result.Outcome with
                | "win" -> true
                | "loss" -> false
                | _ -> false

            let currentTeam, opposingTeam =
                let teams = event.Match.Teams
                teams 
                |> List.partition (fun team -> team.Code = teamCode)
                |> fun (current, opposing) -> (current |> List.exactlyOne, opposing |> List.exactlyOne)

            if (teamWon currentTeam)
            then { Team = opposingTeam; Result = Win }
            else { Team = opposingTeam; Result = Loss }


        let combine results matchup =
            match results with
            | [] -> [matchup]
            | x::xs when x.Team.Code = matchup.Team.Code ->
                match (x.Result, matchup.Result) with
                | (Win, Loss) ->  { Team=x.Team; Result=Tie }::xs
                | (Win, _) ->  { Team=x.Team; Result=Win }::xs
                | (Loss, Win) ->  { Team=x.Team; Result=Tie }::xs
                | (Loss, _) ->  { Team=x.Team; Result=Loss }::xs
                | (Tie, Win) ->  { Team=x.Team; Result=Win }::xs
                | (Tie, Loss) ->  { Team=x.Team; Result=Loss }::xs
                | (Tie, Tie) ->  { Team=x.Team; Result=Tie }::xs
            | xs -> matchup::xs

        pastEvents
        |> Array.filter (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) )
        |> Array.map createMatchup
        |> Array.sortBy (fun result -> result.Team.Code)
        |> Array.fold combine []

[<RequireQualifiedAccess>]
module MatchupResult =

    let create team1 team2 pastEvents =
        let teamCode1 = LcsTeam.toCode team1
        let teamCode2 = LcsTeam.toCode team2

        let teamWon (team: Team) =
            match team.Result.Outcome with
            | "win" -> true
            | "loss" -> false
            | _ -> false

        let filterMatchups event =
            event.Match.Teams 
            |> List.forall (fun team -> team.Code = teamCode1 || team.Code = teamCode2) 

        let filterWinningTeams event =
            event.Match.Teams 
            |> List.exists (fun team -> team.Code = teamCode1 && (teamWon team)) 
        
        let matchupWins =
            pastEvents
            |> Array.filter filterMatchups
            |> Array.filter filterWinningTeams
            |> Array.length
           
        match matchupWins with
        | 2 -> Win
        | 1 -> Tie
        | 0 -> Loss
        | _ -> Win