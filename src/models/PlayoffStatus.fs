namespace Models

open DotLiquid

type PlayoffContentionStatus = 
    Bye | Clinched | Unknown | Eliminated

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | Bye -> "Bye" :> obj
            | Clinched -> "Clinched" :> obj
            | Unknown -> "Unknown" :> obj
            | Eliminated -> "Eliminated" :> obj

type PlayoffStatus =
    {
        Status : PlayoffContentionStatus
        Team : TeamRecord
    }