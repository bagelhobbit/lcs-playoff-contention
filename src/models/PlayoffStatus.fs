namespace Models

open DotLiquid

type PlayoffContentionStatus = 
    Clinched | Unknown | Eliminated

    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | Clinched -> "Clinched" :> obj
            | Unknown -> "Unknown" :> obj
            | Eliminated -> "Eliminated" :> obj

type PlayoffStatus =
    {
        Status : PlayoffContentionStatus
        League : League
        Team : TeamRecord
    }