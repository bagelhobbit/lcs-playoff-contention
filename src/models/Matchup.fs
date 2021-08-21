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

type Matchup = { Team: Team; Result: MatchupResult; MatchResults: string }


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

            let gameResults =
                if (opposingTeam.Result.GameWins + currentTeam.Result.GameWins >= 2)
                then sprintf "(%d-%d)" currentTeam.Result.GameWins opposingTeam.Result.GameWins
                else ""

            if (teamWon currentTeam)
            then ({ Team = opposingTeam; Result = Win; MatchResults = sprintf "%A" Win }, gameResults)
            else ({ Team = opposingTeam; Result = Loss; MatchResults =  sprintf "%A" Loss }, gameResults)


        let combine (_, results) =
            let stringifyResults matchupResults =
                let joinResults acc (matchup, result) =
                    match acc with
                    | "" -> sprintf "%A%s" matchup.Result result
                    | _ -> sprintf "%s - %A%s" acc matchup.Result result

                matchupResults |> List.fold joinResults "" 

            let matchups = results |> List.map (fun (m,_) -> m )

            let totalGames = matchups |> List.length
            let wins = matchups |> List.filter ( fun y -> y.Result = Win) |> List.length
            match (totalGames - wins) with
            | 0 -> { Team = matchups.[0].Team; Result = Win; MatchResults = stringifyResults results }
            | x when (single)x < ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Win; MatchResults = stringifyResults results }
            | x when (single)x = ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Tie; MatchResults = stringifyResults results }
            | x when (single)x > ((single)totalGames / 2.0f) -> { Team = matchups.[0].Team; Result = Loss; MatchResults = stringifyResults results }
            | _ -> { Team = matchups.[0].Team; Result = NA; MatchResults = stringifyResults results }

        pastEvents
        |> Array.toList
        |> List.filter ( fun event -> event.Match.Teams |> List.exists (fun team -> team.Code = teamCode) )
        |> List.map createMatchup
        |> List.groupBy (fun (x, _) -> x.Team.Code)
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
    // doesn't have accurate results except at/near end of season
    // doesn't handle difference in # games played in spring vs summer
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