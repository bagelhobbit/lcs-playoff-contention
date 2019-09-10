module PlayoffContentionWeb

open System.IO
open Microsoft.FSharp.Reflection
open Suave

open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open DotLiquid

open Server

type MatchupModel =
    { Team : string
      Matchups : Models.Matchup list }


let createMatchupModel code =
    let team = Models.LcsTeam.fromCode code
    let matchups = getMatchups team
    {
        Team = Models.LcsTeam.toString team
        Matchups = matchups
    }

let createAllMatchups =
    let teams = FSharpType.GetUnionCases typeof<Models.LcsTeam>
    teams
    |> Array.map ( fun case -> case.Name )
    |> Array.sort
    |> Array.filter ( fun name -> name <> "Unknown") //ignore unknown team
    |> Array.map createMatchupModel

let private createMatchupJson matchups =
    let contents =
        let fold state str =
            if state = "" 
            then str 
            else state + ", " + str

        let toJson matchup =
            let matchups =
                matchup.Matchups
                |> List.sortBy ( fun m -> m.Team.Name )
                |> List.map Models.Matchups.toJson
                |> List.fold fold ""

            sprintf "{ \"team\" : \"%s\", \"matchups\" : [%s] }" matchup.Team matchups

        matchups
        |> Array.sortBy ( fun m -> m.Team )
        |> Array.map toJson
        |> Array.fold fold ""

    "[" + contents + "]"

let app =
    choose 
      [
          GET >=> path "/" >=> Files.sendFile "./public/html/index.html" true
          GET >=> path "/api/getSplitHeader" >=> page "splitHeader.liquid" ((getSplitTitle()))
          GET >=> path "/api/getPlayoffStatuses" >=> page "teamRecords.liquid" (getPlayoffStatuses())
          GET >=> path "/api/matchups" >=> OK(createMatchupJson <| createAllMatchups)
          GET >=> path "/matchups" >=> page "allMatchups.liquid" createAllMatchups
          GET >=> pathScan "/matchups/%s" (createMatchupModel >> page "teamMatchup.liquid")
          GET >=> Files.browseHome
          NOT_FOUND "Found no handlers."
      ]


[<EntryPoint>]
let main argv =
    setTemplatesDir "./templates"
    setCSharpNamingConvention()

    let config =
        { defaultConfig with
            homeFolder = Some (Path.GetFullPath "./public")
            // This allows us to acces the server while it's running in docker
            bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" 8080 ] 
        }

    startWebServer config app
    0 // return an integer exit code
