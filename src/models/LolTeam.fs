namespace Models

open DotLiquid

type League = 
    LCS | LEC | LPL | LCK
    
    interface ILiquidizable with
        member this.ToLiquid() =
            match this with
            | LCS -> "LCS" :> obj
            | LEC -> "LEC" :> obj
            | LPL -> "LPL" :> obj
            | LCK -> "LCK" :> obj

type LolTeam = 
    { Name: string
      Code: string
      Image: string }
