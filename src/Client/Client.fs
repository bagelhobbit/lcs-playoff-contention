module Client

open Elmish
open Elmish.React

open Fable.React
open Fable.React.Props

open Thoth.Json

open Shared
open Shared.TeamRecord


open Fulma
open Shared


type Model = { 
    TeamRecords: Option<TeamRecord list>
    PlayoffStatuses: Option<(string * PlayoffStatus) list>
}

type Msg =
| LcsRecordsLoaded of Result<TeamRecord list, exn>
| LcsPlayoffStatusesLoaded of Result<(string * PlayoffStatus) list, exn>

module Server =

    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : IPlayoffApi =
      Remoting.createApi()
      |> Remoting.withRouteBuilder Route.builder
      |> Remoting.buildProxy<IPlayoffApi>

let initialTeamRecord = Server.api.lcsTeamRecords
let playoffStatuses = Server.api.lcsPlayoffStatuses

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { TeamRecords = None; PlayoffStatuses = None }
    let loadCountCmd =
        Cmd.OfAsync.either
            initialTeamRecord
            ()
            (Ok >> LcsRecordsLoaded)
            (Error >> LcsRecordsLoaded)
    initialModel, loadCountCmd


let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | _, LcsRecordsLoaded (Ok initialTeamRecord) ->
        let nextModel = { currentModel with TeamRecords = Some initialTeamRecord }
        let nextCmd = 
            Cmd.OfAsync.either
                playoffStatuses
                initialTeamRecord
                (Ok >> LcsPlayoffStatusesLoaded)
                (Error >> LcsPlayoffStatusesLoaded)
        nextModel, nextCmd
    | { TeamRecords = Some _ }, LcsPlayoffStatusesLoaded (Ok initialStatuses) ->
        let nextModel = { currentModel with PlayoffStatuses = Some initialStatuses }
        nextModel, Cmd.none

    | _ -> currentModel, Cmd.none

let createTile playoffStatuses teamRecord =
    let getStatusModifier team =
        match playoffStatuses with
        | Some statuses ->
            let (_, status) = 
                statuses
                |> List.find (fun (t, _) -> t = team)
            match status with
            | Eliminated ->
                Tile.CustomClass ("eliminated team")
            | Unknown ->
                Tile.CustomClass ("team")
            | Clinched ->
                Tile.CustomClass ("clinched team")
            | Bye ->
                Tile.CustomClass ("bye team")
        | None ->
            Tile.CustomClass ("team")

    let createTiles result =
        let createOpponentTile =
            Heading.h6 [ ] [ str result.opponent ]

        let createWinLossTile =
            if result.won
            then Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsSuccess) ] ] [ str "Win" ]
            else Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsDanger) ] ] [ str "Loss" ]

        Tile.child [ ] [ createOpponentTile; createWinLossTile ]

    let tiles =
        teamRecord.results
        |> List.map createTiles

    let teamTile =
        Tile.child [ ] 
            [ Heading.h4 [ ] [ str teamRecord.team ]
              Heading.h6 [ ] [ str (sprintf "%d-%d" teamRecord.winLoss.wins teamRecord.winLoss.losses) ] ]

    Tile.parent [ (getStatusModifier teamRecord.team); Tile.Modifiers [ Modifier.BackgroundColor (Color.IsWhiteTer) ] ] (teamTile::tiles)

let showWinLoss records statuses =
    match records with
    | Some teamRecords ->
        teamRecords
        |> List.map (createTile statuses)
    | None _ ->
        []

let safeComponents =
    let components =
        span [ ]
           [
             a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io/elmish/" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://zaid-ajaj.github.io/Fable.Remoting/" ] [ str "Fable.Remoting" ]
           ]

    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ]
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "LCS Playoff Contention" ] ] ]

          Section.section [ ] [
            Container.container [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h2 [ ] [ str "LCS 2019 Spring Split Results"] ] ]

            Container.container [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h6 [ ] 
                        [ str ""
                          span [ ClassName "legend has-background-success"] [ ]
                          str "Clinch Bye" 
                          span [ ClassName "legend has-background-link" ] [ ]
                          str "Clinch Playoffs" 
                          span [ ClassName "legend has-background-danger" ] [ ]
                          str "Eliminated" ] ] ]

            Tile.ancestor [ Tile.IsVertical ] (showWinLoss model.TeamRecords model.PlayoffStatuses) ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
