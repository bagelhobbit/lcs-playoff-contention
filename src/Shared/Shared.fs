namespace Shared

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

open TeamRecord

type PlayoffStatus = Bye | Clinched | Unknown | Eliminated

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IPlayoffApi = { 
    lcsTeamRecords : unit -> Async<TeamRecord list>
    lcsPlayoffStatuses : TeamRecord list -> Async<(string * PlayoffStatus) list>
}
