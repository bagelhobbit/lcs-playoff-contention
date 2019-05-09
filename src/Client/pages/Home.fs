module Client.Home

open Shared
open Shared.TeamRecord

open Client.Styles

open Fable.React
open Fable.React.Props
open Fulma


type Model = {
    Records: TeamRecord list option
    PlayoffStatuses: (string * PlayoffStatus) list option
    HeadToHeadLink: string -> unit
}


let private createTile playoffStatuses headToHeadLink teamRecord =
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
            [ buttonLink "" (fun _ -> headToHeadLink teamRecord.team)
                [ Heading.h4 [ ] [ str teamRecord.team ]
                  Heading.h6 [ ] [ str (sprintf "%d-%d" teamRecord.winLoss.wins teamRecord.winLoss.losses) ] ] ]

    Tile.parent [ (getStatusModifier teamRecord.team); Tile.Modifiers [ Modifier.BackgroundColor (Color.IsWhiteTer) ] ] (teamTile::tiles)

let private playoffLegend =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h6 [ ] 
                [ div [ ClassName "legend" ]
                    [ span [ ClassName "has-background-success"] [ ]
                      str "Clinch Bye" ]
                  div [ ClassName "legend" ]
                    [ span [ ClassName "has-background-link" ] [ ]
                      str "Clinch Playoffs" ]
                  div [ ClassName "legend" ] 
                    [ span [ ClassName "has-background-danger" ] [ ]
                      str "Eliminated" ] ] ] ]

let private sectionTitle =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h2 [ ] [ str "LCS 2019 Spring Split Results"] ] ]

let view model =
    let tiles = 
        match model.Records with
        | Some teamRecords ->
                teamRecords
                |> List.map (createTile model.PlayoffStatuses model.HeadToHeadLink)
        | None _ ->
            []
    div [ ]
        [ sectionTitle
          playoffLegend
          Tile.ancestor [ Tile.IsVertical ] tiles ]