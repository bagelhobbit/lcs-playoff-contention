module PlayoffContentionWeb

open System.IO
open Suave

open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open DotLiquid
open Suave.DotLiquid

open Server

let greetings q =
    defaultArg (Option.ofChoice (q ^^ "name")) "World" |> sprintf "Hello %s"

let sample : WebPart =
    path "/hello" >=> choose 
      [
          GET >=> request (fun r -> OK (greetings r.query))
          POST >=> request (fun r -> OK (greetings r.form))
          NOT_FOUND "Found no handlers"
      ]

let requiresAuthentication _ =
    choose
      [
          GET >=> path "/public" >=> OK "Default GET"
          // Access to handler after this one will require authentication
          Authentication.authenticateBasic
            (fun (user, pwd) -> user = "foo" && pwd = "bar")
            (GET >=> path "/whereami" >=> OK (sprintf "Hello authenticated person "))
      ]

type MatchupModel =
    { Team : string
      Matchups : Models.Matchup list }

let createMatchupModel team =
    let team = Models.LcsTeam.fromCode team
    let matchups = getMatchups team
    {
        Team = Models.LcsTeam.toString team
        Matchups = matchups
    }

let app =
    choose 
      [
          GET >=> path "/" >=> Files.sendFile "./public/html/index.html" true
          GET >=> path "/api/getSplitHeader" >=> page "splitHeader.liquid" ((getSplitTitle()))
          GET >=> path "/api/getPlayoffStatuses" >=> page "teamRecords.liquid" (getPlayoffStatuses())
          GET >=> pathScan "/matchups/%s" (createMatchupModel >> page "teamMatchup.liquid")
          GET >=> Files.browseHome
          sample
          requiresAuthentication ()
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
