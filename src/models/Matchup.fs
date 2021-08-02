namespace Models

open DotLiquid

type MatchupResult = 
    Win | Tie | Loss | NA

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | Win -> "Win" :> obj
            | Tie -> "Tie" :> obj
            | Loss -> "Loss" :> obj
            | NA -> "-" :> obj

type Matchup = { Team: Team; Result: MatchupResult }


[<RequireQualifiedAccess>]
module Matchups =

    let create team pastEvents =
        let teamCode = team.Code

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


        let combine (_, matchups) =
            let totalGames = matchups |> List.length
            let wins = matchups |> List.filter ( fun y -> y.Result = Win) |> List.length
            match (totalGames - wins) with
            | 0 -> { Team = matchups.[0].Team; Result = Win }
            | x when (single)x < ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Win }
            | x when (single)x = ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Tie }
            | x when (single)x > ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Loss }
            | _ -> { Team = matchups.[0].Team; Result = NA }

        pastEvents
        |> Array.toList
        |> List.filter (fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) )
        |> List.map createMatchup
        |> List.sortBy (fun result -> result.Team.Code)
        |> List.groupBy (fun x -> x.Team.Code)
        |> List.map combine


    let toJson matchup =
        let matchupResult =
            match matchup.Result with
            | Win -> "Won"
            | Tie -> "Tied"
            | Loss -> "Lost"
            | NA -> "-"
        
        sprintf "{ \"team\" : \"%s\", \"result\" : \"%s\" }" matchup.Team.Name matchupResult




[<RequireQualifiedAccess>]
module MatchupResult =

    // TODO: Better method for determining head-to-head matchup
    // does have accurate results except at/near end of season
    // doesn't handle differnce in # games played in spring vs summer
    let create team1 team2 pastEvents =
        let teamCode1 = team1.Code
        let teamCode2 = team2.Code

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