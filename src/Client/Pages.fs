module Client.Pages

open Elmish.UrlParser

[<RequireQualifiedAccess>]
type Page =
    | Home
    | HeadToHead of string

let toPath =
    function
    | Page.Home -> "/"
    | Page.HeadToHead team -> "/headtohead/" + team

let pageParser : Parser<Page -> Page, _> =
    oneOf
        [ map Page.Home (s "")
          map Page.HeadToHead (s "headtohead" </> str) ]

let urlParser location = parsePath pageParser location