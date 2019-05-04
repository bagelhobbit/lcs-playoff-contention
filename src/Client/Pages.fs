module Client.Pages

open Elmish.UrlParser

[<RequireQualifiedAccess>]
type Page =
    | Home
    | HeadToHead

let toPath =
    function
    | Page.Home -> "/"
    | Page.HeadToHead -> "/headtohead"

let pageParser : Parser<Page -> Page, _> =
    oneOf
        [ map Page.Home (s "")
          map Page.HeadToHead (s "headtohead") ]

let urlParser location = parsePath pageParser location