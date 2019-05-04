module Client.HeadToHead

open Shared.HeadToHead

open Fable.React
open Fable.React.Props
open Fulma


type Model = {
    Results: HeadToHead list option
}

let view model =
    let results =
        match model.Results with
        | Some results ->
            str (results |> List.head |> (fun r -> sprintf "%s - %A" r.team r.result)) 
        | None ->
            str ""
    div [ ] 
        [ str "TEST"
          br [ ]
          results ]
