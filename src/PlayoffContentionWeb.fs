module PlayoffContentionWeb

open System.IO
open Suave

open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open DotLiquid

open Models
open Server


let app =
    choose 
      [
          GET >=> path "/" >=> Files.sendFile "./public/html/index.html" true
          GET >=> pathScan "/api/getSplitHeader/%s" (getSplitTitle >> page "splitHeader.liquid")
          GET >=> pathScan "/api/getPlayoffStatuses/%s/%s" (getPlayoffStatuses >> page "teamRecords.liquid")
          GET >=> pathScan "/api/matchups/%s" (createAllMatchups >> TeamMatchups.toJson >> OK)
          GET >=> pathScan "/matchups/%s/%s" (createTeamMatchupByCode >> page "teamMatchup.liquid")
          GET >=> pathScan "/matchups/%s" (( fun s -> s.ToUpper() ) >> page "allMatchups.liquid")
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
