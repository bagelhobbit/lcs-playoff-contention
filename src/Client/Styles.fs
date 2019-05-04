module Client.Styles

open Elmish.Navigation

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props

open Client.Pages

let goToUrl (e: Browser.Types.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore

let viewLink page description =
    a [ Href (Pages.toPath page)
        OnClick goToUrl ]
      [ str description]

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