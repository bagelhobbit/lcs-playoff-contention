module Client.TeamHeadToHead

open Fable.React
open Fable.React.Props
open Fulma

open Shared

open Client.Styles


type Model = {
    Results: HeadToHead list option
    Team: LcsTeam
    HomeLink: unit -> unit
}

let private createTeamTile (result: HeadToHead) =
    let generateResultString result =
        match result with
        | Win ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsSuccess) ] ] [ str "Won" ]
        | Tie ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsInfo) ] ] [ str "Tied" ]
        | Loss ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsDanger) ] ] [ str "Lost" ]

    Tile.child [ ]
        [ Heading.h4 [ ] [ str (result.Team.Code) ]
          (generateResultString result.Result) ]

let private createTeamName team =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h2 [ ] [ str ( (LcsTeam.toString team) + " vs.") ] ] ]

let private createSectionTitle =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h2 [ ] [ str "LCS 2019 Spring Split Head to Head Matchups" ] ] ]

let private createBreadcrumbs team homeLink =
    Breadcrumb.breadcrumb [ Breadcrumb.Size IsMedium ]
        [ Breadcrumb.item [ ]
            [ buttonLink "" homeLink [ str "Home"] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
            [ a [ ] [ str "Head to Head" ] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
            [ a [ ] [ str (LcsTeam.toString team) ] ] ]

let view model =
    let results =
        match model.Results with
        | Some results ->
            results
            |> List.sortBy (fun headToHead -> headToHead.Result, headToHead.Team.Code)
            |> List.map createTeamTile

        | None ->
            [ ]

    div [ ]
        [ createBreadcrumbs model.Team model.HomeLink
          createSectionTitle
          createTeamName model.Team
          Tile.parent [ Tile.Modifiers [ Modifier.BackgroundColor (Color.IsWhiteTer) ] ] results ]
