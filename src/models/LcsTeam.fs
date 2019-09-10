namespace Models

open DotLiquid

type LcsTeam =
    | C9
    | CG
    | CLG
    | FOX
    | FLY
    | GGS
    | OPT
    | Thieves
    | TL
    | TSM
    | Unknown

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | C9 -> "C9" :> obj
            | CG -> "CG" :> obj
            | CLG -> "CLG" :> obj
            | FOX -> "FOX" :> obj
            | FLY -> "FLY" :> obj
            | GGS -> "GGS" :> obj
            | OPT -> "OPT" :> obj
            | Thieves -> "100" :> obj
            | TL -> "TL" :> obj
            | TSM -> "TSM" :> obj
            | Unknown -> "??" :> obj

[<RequireQualifiedAccess>]
module LcsTeam =

    let noneValue = LcsTeam.Unknown

    let lcsTeams = [C9; CG; CLG; FOX; FLY; GGS; OPT; Thieves; TL; TSM]

    let toString = function
        | C9 -> "Cloud9"
        | CG -> "Clutch Gaming"
        | CLG -> "Counter Logic Gaming"
        | FOX -> "Echo Fox"
        | FLY -> "FlyQuest"
        | GGS -> "Golden Guardians"
        | OPT -> "OpTic Gaming"
        | Thieves -> "100 Thieves"
        | TL -> "Team Liquid"
        | TSM -> "TSM"
        | Unknown -> "Unknown Team"

    let toCode = function
        | C9 -> "C9"
        | CG -> "CG"
        | CLG -> "CLG"
        | FOX -> "FOX"
        | FLY -> "FLY"
        | GGS -> "GGS"
        | OPT -> "OPT"
        | Thieves -> "100"
        | TL -> "TL"
        | TSM -> "TSM"
        | Unknown -> "??"

    let fromCode = function
        | "100" -> Thieves
        | "C9" -> C9
        | "CG" -> CG
        | "CLG" -> CLG
        | "FOX" -> FOX
        | "FLY" -> FLY
        | "GGS" -> GGS
        | "OPT" -> OPT
        | "TL" -> TL
        | "TSM" -> TSM
        | "Thieves" -> Thieves
        | _ -> LcsTeam.Unknown
