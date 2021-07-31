module Server

open Microsoft.FSharp.Reflection

open Models

open LeagueTournamentJson
open LeagueScheduleJson
open Teams
open EliminatedTeams
open PlayoffTeams


type RecordSort = Cumulative | Split 

let getCurrentRecords sortType : TeamRecord list =

    let lcsResults = 
        LeagueSchedule.getSchedule()
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
        |> Array.map LeagueSchedule.create

    let descendingComparer team1 team2 =
        // 1 - x > y; 0 - x = y; -1 - x < y
        // Reverse the comparer so teams with a better head to head record are at the top
        if team1.WinLoss = team2.WinLoss
        then 
            let headToHead = MatchupResult.create team1.LcsTeam team2.LcsTeam lcsResults

            match headToHead with
            | Win -> -1
            | Tie -> 0
            | Loss -> 1
            | NA -> 1
        else 0

    let getSortableWins record =
        match sortType with
        | Cumulative -> record.WinLoss.Wins
        | Split -> record.SplitWinLoss.Wins

    let currentRecords =
        LolTeam.LcsTeams
        |> Array.toList
        |> List.map (TeamRecord.create lcsResults)
        |> List.sortByDescending getSortableWins
        |> List.sortWith descendingComparer

    currentRecords 

let getPlayoffStatuses sort : PlayoffStatus list =

    let sortType =
        match sort with
        | "split" -> Split
        | "cumulative" -> Cumulative
        | "all" -> Cumulative 
        | _ -> Cumulative

    let teamRecords = getCurrentRecords sortType

    let remainingSchedule =
        LeagueSchedule.getSchedule()
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateUnstarted)
        |> Array.map LeagueSchedule.create

    let eliminatedTeams = 
        findEliminatedTeams teamRecords remainingSchedule
        |> List.map (fun team -> team.LcsTeam)

    let playoffTeams =
        findPlayoffTeams teamRecords
        |> List.map (fun team -> team.LcsTeam)

    let assignPlayoffStatus team =
        let containsTeam =
            [ eliminatedTeams; playoffTeams]
            |> List.map (List.contains team.LcsTeam)

        match containsTeam with
        | _::[x] when x -> { Status = Clinched; Team = team }
        | x::_ when x -> { Status = Eliminated; Team = team }
        | _ -> { Status = Unknown; Team = team }

    teamRecords
    |> List.map assignPlayoffStatus


let getMatchups team : Matchup list =
    let lcsResults = 
        LeagueSchedule.getSchedule()
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
        |> Array.map LeagueSchedule.create

    Matchups.create team lcsResults
    |> List.sortBy (fun matchup -> matchup.Team.Code)
    |> List.sortBy (fun matchup -> matchup.Result)

let getSplitTitle (_:string) : string =
    LeagueTournament.currentSplitSeason

let createTeamMatchup team =
    let matchups = getMatchups team
    TeamMatchups.create matchups team

let createAllMatchups (_:string) =
    let teams = LolTeam.LcsTeams
    
    let sortedTeams =
        teams
        |> Array.sortBy (fun (t) -> t.Name)

    let emptyMatchups =
        sortedTeams
        |> Array.map ( fun team -> { 
            Team = { Code = team.Code; Name = team.Name; 
                     Result = { Outcome = ""; GameWins = 0 }; Record = { Wins = 0; Losses = 0 } }
            Result = NA } )

    let matchups =
        sortedTeams
        |> Array.map createTeamMatchup

    let filledMatchups =
        matchups
        |> Array.map ( fun matchup ->
            let updated = 
                Array.fold ( fun (acc : Matchup list) (elem : Matchup) ->
                    let alreadyContains = acc |> List.exists (fun item -> item.Team.Code = elem.Team.Code)
                    if alreadyContains then acc
                    else elem::acc
                    ) matchup.Matchups emptyMatchups 

            { matchup with Matchups = updated } )

    filledMatchups