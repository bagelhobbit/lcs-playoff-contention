module PlayoffContentionWeb

open System.IO
open Suave

open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open DotLiquid

open Server


let app =
    choose 
      [
          GET >=> path "/" >=> Files.sendFile "./public/html/index.html" true
          GET >=> path "/api/getSplitHeader" >=> page "splitHeader.liquid" ((getSplitTitle()))
          GET >=> path "/api/getPlayoffStatuses" >=> page "teamRecords.liquid" (getPlayoffStatuses())
          GET >=> path "/api/matchups" >=> OK(Models.TeamMatchups.toJson <| createAllMatchups)
          GET >=> path "/matchups" >=> page "allMatchups.liquid" createAllMatchups
          GET >=> pathScan "/matchups/%s" (createTeamMatchup >> page "teamMatchup.liquid")
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
