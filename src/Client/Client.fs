module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Thoth.Json

open Shared
open Shared.WinLoss


open Fulma


// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { 
    TeamRecords: Option<TeamRecord list>
}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| LcsRecordsLoaded of Result<TeamRecord list, exn>

module Server =

    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : IPlayoffApi =
      Remoting.createApi()
      |> Remoting.withRouteBuilder Route.builder
      |> Remoting.buildProxy<IPlayoffApi>

let initialTeamRecord = Server.api.lcsTeamRecords

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { TeamRecords = None }
    let loadCountCmd =
        Cmd.ofAsync
            initialTeamRecord
            ()
            (Ok >> LcsRecordsLoaded)
            (Error >> LcsRecordsLoaded)
    initialModel, loadCountCmd



// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | _, LcsRecordsLoaded (Ok initialTeamRecord) ->
        let nextModel = {currentModel with TeamRecords = Some initialTeamRecord}
        nextModel, Cmd.none

    | _ -> currentModel, Cmd.none


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

let showWinLoss record =
    match record with
    | Some teamRecord ->
        let teamColumns, teamResultColumns = 
            teamRecord 
            |> List.fold (fun (teams, records) current -> 
                    let teamColumn = Column.column [] [ Heading.h4 [] [ str current.team ] ]
                    let recordColumn = Column.column [] [str (sprintf "%d - %d" current.winLoss.wins current.winLoss.losses )]
                    (teams @ [teamColumn], records @ [recordColumn])
                ) ([], [])
        [ Columns.columns [] teamColumns
          Columns.columns [] teamResultColumns ]
    | None _ ->
        []

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Section.section [] [
            Container.container []
                  [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered ) ] ]
                      [ Heading.h3 [] [ str "LCS Spring 2019 Split Results"] ] ]

            Container.container [] (showWinLoss model.TeamRecords) ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
