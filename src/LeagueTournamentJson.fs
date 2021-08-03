namespace LeagueTournamentJson

open FSharp.Data
open System
open Models

type LeagueTournaments = JsonProvider<"src/json/tournament.json">


[<RequireQualifiedAccess>]
module LeagueTournament =  


    let mutable private _lastLeague = LCS

    let mutable private _lastUpdated = DateTime.MinValue

    let mutable private _tournament = None

    let private getMostRecentTournament league =
        let tournamentApi =
            let lcsLeagueId = "98767991299243165"
            let lecLeagueId = "98767991302996019"

            let leagueId = 
                match league with
                | LEC -> lecLeagueId
                | _ -> lcsLeagueId
                
            let apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

            Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getTournamentsForLeague", httpMethod = "GET",
                query = [ "hl", "en-US"; "leagueId", leagueId],
                headers = [ "x-api-key", apiKey] )

        let tournaments = LeagueTournaments.Parse(tournamentApi).Data.Leagues.[0].Tournaments

        tournaments
        |> Array.maxBy (fun t -> t.StartDate)

    let mostRecentTournament league = 
        match _tournament with
        | None ->
            let recentTournament = getMostRecentTournament league
            _tournament <- Some recentTournament
            _lastLeague <- league
            _lastUpdated <- DateTime.Now
            recentTournament
        | Some tournament ->
            if (DateTime.Now - _lastUpdated).Days > 1 || _lastLeague <> league 
            then
                let recentTournament = getMostRecentTournament league
                _tournament <- Some recentTournament
                _lastLeague <- league
                _lastUpdated <- DateTime.Now
                recentTournament
            else 
                tournament

    let currentSplitSeason league =
        let currentSeason =
            if (mostRecentTournament league).Slug.EndsWith("spring") || (mostRecentTournament league).Slug.EndsWith("split1")
            then "Spring"
            else "Summer"

        sprintf "%A %d %s Split Results" league DateTime.Now.Year currentSeason