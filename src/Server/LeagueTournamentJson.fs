namespace LeagueTournamentJson

open FSharp.Data
open System

type LeagueTournaments = JsonProvider<"tournamentBasic.json">

[<RequireQualifiedAccess>]
module LeagueTournament =  

    let mostRecentTournament =
        let tournamentApi =
            let naLeagueId = "98767991299243165"
            let apiKey = "0TvQnueqKa5mxJntVWt0w4LpLfEkrV1Ta8rQBb9Z"

            Http.RequestString( "https://esports-api.lolesports.com/persisted/gw/getTournamentsForLeague", httpMethod = "GET",
                query = [ "hl", "en-US"; "leagueId", naLeagueId],
                headers = [ "x-api-key", apiKey] )

        let tournaments = LeagueTournaments.Parse(tournamentApi).Data.Leagues.[0].Tournaments

        tournaments
        |> Array.maxBy (fun t -> t.StartDate)

    let currentSplitSeason =
        let currentSeason =
            if mostRecentTournament.Slug.EndsWith("spring")
            then "Spring"
            else "Summer"
        
        sprintf "LCS %d %s"  DateTime.Now.Year currentSeason