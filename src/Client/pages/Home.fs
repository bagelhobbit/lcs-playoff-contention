module Client.Home

open Fable.React
open Fable.React.Props
open Fulma

open Shared.TeamRecord
open Shared

open Client.Styles


type Model = {
    Records: TeamRecord list option
    PlayoffStatuses: (LcsTeam * PlayoffStatus) list option
    HeadToHeadLink: LcsTeam -> unit
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
                Tile.CustomClass ("team-eliminated")
            | Unknown ->
                Tile.CustomClass ("team")
            | Clinched ->
                Tile.CustomClass ("team-clinched")
            | Bye ->
                Tile.CustomClass ("team-bye")
        | None ->
            Tile.CustomClass ("team")

    let createTiles result =
        let createOpponentTile =
            Heading.h6 [ ] [ str (LcsTeam.toCode result.Opponent) ]

        let createWinLossTile =
            if result.Won
            then Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsSuccess) ] ] [ str "Win" ]
            else Heading.h6 [ Heading.Modifiers [ Modifier.TextColor (Color.IsDanger) ] ] [ str "Loss" ]

        Tile.child [ ] [ createOpponentTile; createWinLossTile ]

    let tiles =
        teamRecord.Results
        |> List.map createTiles

    let teamTile =
        Tile.child [ ] 
            [ buttonLink "" (fun _ -> headToHeadLink teamRecord.Team)
                [ Heading.h4 [ ] [ str (LcsTeam.toCode teamRecord.Team) ]
                  Heading.h6 [ ] [ str (sprintf "%d-%d" teamRecord.WinLoss.Wins teamRecord.WinLoss.Losses) ] ] ]

    Tile.parent [ (getStatusModifier teamRecord.Team); Tile.Modifiers [ Modifier.BackgroundColor (Color.IsWhiteTer) ] ] (teamTile::tiles)

let private playoffLegend =
    Container.container [ ]
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h6 [ ] 
                [ span [ ClassName "legend-bye"] [ str "Clinch Bye" ]
                  span [ ClassName "legend-clinched" ] [ str "Clinch Playoffs" ]
                  span [ ClassName "legend-eliminated" ] [ str "Eliminated" ] ] ] ]

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