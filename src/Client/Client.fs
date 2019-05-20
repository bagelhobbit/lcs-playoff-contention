module Client.App

open Elmish
open Elmish.React
open Elmish.Navigation

open Fable.React

open Shared
open Shared.TeamRecord
open Shared.HeadToHead

open Client.Styles

open Fulma

type PageModel =
    | HomePageModel
    | HeadToHeadPageModel

type Model = { 
    TeamRecords: Option<TeamRecord list>
    PlayoffStatuses: Option<(LcsTeam * PlayoffStatus) list>
    HeadToHeadResults: Option<HeadToHead list>
    HeadToHeadTeam: LcsTeam option
    PageModel: PageModel
}

type Msg =
    | LoadTeamRecords
    | LoadTeamHeadToHead of LcsTeam
    | TeamRecordsLoaded of Result<TeamRecord list, exn>
    | PlayoffStatusesLoaded of Result<(LcsTeam * PlayoffStatus) list, exn>
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

let init () : Model * Cmd<Msg> =
    let initialModel = 
        { TeamRecords = None
          PlayoffStatuses = None
          HeadToHeadResults = None
          HeadToHeadTeam = None
          PageModel = HomePageModel }
    let loadTeamRecordsCmd =
        Cmd.OfAsync.perform
            initialTeamRecord
            ()
            (Ok >> TeamRecordsLoaded)
    initialModel, loadTeamRecordsCmd

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | _, LoadTeamRecords ->
        let nextModel = { currentModel with PageModel = HomePageModel }
        let nextCmd =
            Cmd.OfAsync.perform
                initialTeamRecord
                ()
                (Ok >> TeamRecordsLoaded)
        nextModel, nextCmd

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
        
    | _, LoadTeamHeadToHead team ->
        let nextModel = { currentModel with PageModel = HeadToHeadPageModel; HeadToHeadTeam = Some team }
        let nextCmd =
            Cmd.OfAsync.perform
                headToHeadResults
                team
                (Ok >> HeadToHeadResult)
        nextModel, nextCmd

    | { PageModel = HeadToHeadPageModel }, HeadToHeadResult (Ok results) ->
        let nextModel = { currentModel with HeadToHeadResults = Some results}
        nextModel, Cmd.none

    | _ ->
        currentModel, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ]
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ Heading.Modifiers [ Modifier.TextColor Color.IsWhite ] ]
                    [ str "LCS Playoff Contention" ] ] ]

          Section.section [ ] [
            match model.PageModel with
            | HomePageModel ->
                yield Home.view { Records = model.TeamRecords
                                  PlayoffStatuses = model.PlayoffStatuses
                                  HeadToHeadLink = (fun s -> dispatch (LoadTeamHeadToHead s) ) }
            | HeadToHeadPageModel ->
                let teamName =
                    match model.HeadToHeadTeam with
                    | Some s -> s
                    | None -> LcsTeam.Unknown
                yield TeamHeadToHead.view { Results = model.HeadToHeadResults
                                            Team = teamName
                                            HomeLink = (fun _ -> dispatch LoadTeamRecords) }
          ]

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
