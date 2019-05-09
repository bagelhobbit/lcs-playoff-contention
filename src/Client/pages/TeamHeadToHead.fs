module Client.TeamHeadToHead

open Shared.HeadToHead

open Fable.React
open Fable.React.Props
open Fulma

open Client.Styles


type Model = {
    Results: HeadToHead list option
    Team: string
    HomeLink: unit -> unit
}


let private createTeamTile result =
    let generateResultString result =
        match result with
        | Win ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsSuccess) ] ] [ str "Won" ]
        | Tie ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsInfo) ] ] [ str "Tied" ]
        | Loss ->
            Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsDanger) ] ] [ str "Lost" ]

    Tile.child [ ]
        [ Heading.h4 [ ] [ str result.team ]
          (generateResultString result.result) ]

let private teamName name =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h2 [ ] [ str (name + " vs.") ] ] ]

let private sectionTitle =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h2 [ ] [ str "LCS 2019 Spring Split Head to Head Matchups" ] ] ]

let private breadcrumbs homeLink =
    Breadcrumb.breadcrumb [ Breadcrumb.Size IsMedium ]
        [ Breadcrumb.item [ ]
            [ buttonLink "" homeLink [ str "Home"] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
            [ a [ ] [ str "Head to Head" ] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
            [ a [ ] [ str "Team Liquid" ] ] ]

let view model =
    let results =
        match model.Results with
        | Some results ->
            results
            |> List.sortBy (fun r -> r.result)
            |> List.map createTeamTile

        | None ->
            [ ]
    div [ ]
        [ breadcrumbs model.HomeLink
          sectionTitle
          (teamName model.Team)
          Tile.parent [ Tile.Modifiers [ Modifier.BackgroundColor (Color.IsWhiteTer) ] ] results ]
