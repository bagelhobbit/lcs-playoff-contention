module Server

open Microsoft.FSharp.Reflection

open Models

open LeagueTournamentJson
open LeagueScheduleJson
open EliminatedTeams
open PlayoffTeams


let getCurrentRecords() : TeamRecord list =
    let lcsTeams = LcsTeam.lcsTeams

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
        else 0

    let currentRecords =
        lcsTeams
        |> List.map (TeamRecord.create lcsResults)
        |> List.sortByDescending (fun record -> record.WinLoss.Wins)
        |> List.sortWith descendingComparer

    currentRecords 

let getPlayoffStatuses (_:int) : PlayoffStatus list =
    let teamRecords = getCurrentRecords()

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

    let playoffByes =
        findPlayoffByes teamRecords
        |> List.map (fun team -> team.LcsTeam)

    let assignPlayoffStatus team =
        let containsTeam =
            [ eliminatedTeams; playoffTeams; playoffByes ]
            |> List.map (List.contains team.LcsTeam)

        match containsTeam with
        | _::_::[x] when x -> { Status = Bye; Team = team }
        | _::x::_ when x -> { Status = Clinched; Team = team }
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
    |> List.sortBy (fun matchup -> matchup.Team)
    |> List.sortBy (fun matchup -> matchup.Result)

let getSplitTitle (_:int) : string =
    LeagueTournament.currentSplitSeason


let createTeamMatchup code =
    let team = LcsTeam.fromCode code
    let matchups = getMatchups team
    TeamMatchups.create matchups team

let createAllMatchups =
    let teams = FSharpType.GetUnionCases typeof<LcsTeam>
    teams
    |> Array.map ( fun case -> case.Name )
    |> Array.sort
    |> Array.filter ( fun name -> name <> "Unknown") //ignore unknown team
    |> Array.map createTeamMatchup