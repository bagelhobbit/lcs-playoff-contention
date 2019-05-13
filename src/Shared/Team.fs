namespace Shared

module Team =
    type Team =
        | C9
        | CG
        | CLG
        | FOX
        | FQ
        | GGS
        | OPT
        | Thieves
        | TL
        | TSM
        | Unknown

    let toString = function
        | C9 -> "Cloud9"
        | CG -> "Clutch Gaming"
        | CLG -> "Counter Logic Gaming"
        | FOX -> "Echo Fox"
        | FQ -> "FlyQuest"
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
        | FQ -> "FQ"
        | GGS -> "GGS"
        | OPT -> "OPT"
        | Thieves -> "100"
        | TL -> "TL"
        | TSM -> "TSM"
        | Unknown -> "??"
