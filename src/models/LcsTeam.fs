namespace Models

open DotLiquid

type LcsTeam =
    | C9
    | CLG
    | DIG
    | FOX
    | FLY
    | GG
    | IMT
    | Thieves
    | TL
    | TSM
    | Unknown

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | C9 -> "C9" :> obj
            | CLG -> "CLG" :> obj
            | DIG -> "DIG" :> obj
            | FOX -> "FOX" :> obj
            | FLY -> "FLY" :> obj
            | GG -> "GG" :> obj
            | IMT -> "IMT" :> obj
            | Thieves -> "100" :> obj
            | TL -> "TL" :> obj
            | TSM -> "TSM" :> obj
            | Unknown -> "??" :> obj

[<RequireQualifiedAccess>]
module LcsTeam =

    let noneValue = LcsTeam.Unknown

    let lcsTeams = [C9; CLG; DIG; FOX; FLY; GG; IMT; Thieves; TL; TSM]

    let toString = function
        | C9 -> "Cloud9"
        | CLG -> "Counter Logic Gaming"
        | DIG -> "Dignitas"
        | FOX -> "Echo Fox"
        | FLY -> "FlyQuest"
        | GG -> "Golden Guardians"
        | IMT -> "Immortals"
        | Thieves -> "100 Thieves"
        | TL -> "Team Liquid"
        | TSM -> "TSM"
        | Unknown -> "Unknown Team"

    let toCode = function
        | C9 -> "C9"
        | CLG -> "CLG"
        | DIG -> "DIG"
        | FOX -> "FOX"
        | FLY -> "FLY"
        | GG -> "GG"
        | IMT -> "IMT"
        | Thieves -> "100"
        | TL -> "TL"
        | TSM -> "TSM"
        | Unknown -> "??"

    let fromCode = function
        | "100" -> Thieves
        | "C9" -> C9
        | "CLG" -> CLG
        | "DIG" -> DIG
        | "FOX" -> FOX
        | "FLY" -> FLY
        | "GG" -> GG
        | "IMT" -> IMT
        | "TL" -> TL
        | "TSM" -> TSM
        | _ -> LcsTeam.Unknown
