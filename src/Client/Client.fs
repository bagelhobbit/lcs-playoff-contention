module Client.App

open Elmish
open Elmish.React
open Elmish.Navigation

open Fable.React

open Shared
open Shared.TeamRecord
open Shared.HeadToHead
open Client.Pages
open Client.Styles

open Fulma

type PageModel =
    | HomePageModel
    | HeadToHeadPageModel

type Model = { 
    TeamRecords: Option<TeamRecord list>
    PlayoffStatuses: Option<(string * PlayoffStatus) list>
    HeadToHeadResults: Option<HeadToHead list>
    PageModel: PageModel
}

type Msg =
    | TeamRecordsLoaded of Result<TeamRecord list, exn>
    | PlayoffStatusesLoaded of Result<(string * PlayoffStatus) list, exn>
    | HeadToHeadResult of Result<HeadToHead list, exn>

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
let headToHeadResults = Server.api.teamHeadToHeadRecords

let urlUpdate (result:Page option) (model:Model) =
    match result with
    | None ->
        model, Navigation.modifyUrl (toPath Page.Home)
    | Some Page.Home ->
        { model with PageModel = HomePageModel }, Cmd.none
    | Some (Page.HeadToHead team) ->
        let nextModel = { model with PageModel = HeadToHeadPageModel }
        let nextCmd =
            Cmd.OfAsync.perform
                headToHeadResults
                team
                (Ok >> HeadToHeadResult)
        nextModel, nextCmd

// Use the page parameter to make `toNavigatable` happys
let init page : Model * Cmd<Msg> =
    let initialModel = 
        { TeamRecords = None
          PlayoffStatuses = None
          HeadToHeadResults = None
          PageModel = HomePageModel }
    let loadCountCmd =
        Cmd.OfAsync.perform
            initialTeamRecord
            ()
            (Ok >> TeamRecordsLoaded)
    initialModel, loadCountCmd


let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | _, TeamRecordsLoaded (Ok initialTeamRecord) ->
        let nextModel = { currentModel with TeamRecords = Some initialTeamRecord }
        let nextCmd = 
            Cmd.OfAsync.perform
                playoffStatuses
                initialTeamRecord
                (Ok >> PlayoffStatusesLoaded)
        nextModel, nextCmd

    | { TeamRecords = Some _ }, PlayoffStatusesLoaded (Ok initialStatuses) ->
        let nextModel = { currentModel with PlayoffStatuses = Some initialStatuses }
        nextModel, Cmd.none

    | { PageModel = HeadToHeadPageModel }, HeadToHeadResult (Ok results) ->
        let nextModel = { currentModel with HeadToHeadResults = Some results}
        nextModel, Cmd.none

    | _ ->
        currentModel, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ]
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "LCS Playoff Contention" ] ] ]

          viewLink (Page.HeadToHead "TL") "TL Head to Heads"

          Section.section [ ] [
            match model.PageModel with
            | HomePageModel ->
                yield Home.view { Records = model.TeamRecords; PlayoffStatuses = model.PlayoffStatuses }
            | HeadToHeadPageModel ->
                yield HeadToHead.view { Results = model.HeadToHeadResults }
          ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
#endif

Program.mkProgram init update view
|> Program.toNavigable Pages.urlParser urlUpdate
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
