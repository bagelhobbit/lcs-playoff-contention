module Server

open Models

open LeagueTournamentJson
open LeagueScheduleJson
open Teams
open EliminatedTeams
open PlayoffTeams


type RecordSort = Cumulative | Split 

let getLeagueType (league: string) =
    match league.ToLower() with
    | "lcs" -> LCS
    | "lec" -> LEC
    | _ -> LCS

let getCurrentRecords sortType league: TeamRecord list =
    let lcsResults = 
        LeagueSchedule.getSchedule league
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
        |> Array.map LeagueSchedule.create

    let descendingComparer team1 team2 =
        // 1 - x > y; 0 - x = y; -1 - x < y
        // Reverse the comparer so teams with a better head to head record are at the top
        if team1.WinLoss = team2.WinLoss
        then 
            let headToHead = MatchupResult.create team1.LolTeam team2.LolTeam lcsResults

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
        LolTeam.LolTeams league
        |> Array.toList
        |> List.map (TeamRecord.create lcsResults)
        |> List.sortByDescending getSortableWins
        |> List.sortWith descendingComparer

    currentRecords 

let getPlayoffStatuses (league, sort) : PlayoffStatus list = 
    let sortType =
        match sort with
        | "split" -> Split
        | "cumulative" -> Cumulative
        | "all" -> Cumulative 
        | _ -> Cumulative

    let leagueType = getLeagueType league

    let teamRecords = getCurrentRecords sortType leagueType

    let remainingSchedule =
        LeagueSchedule.getSchedule leagueType
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateUnstarted)
        |> Array.map LeagueSchedule.create

    let eliminatedTeams = 
        findEliminatedTeams teamRecords remainingSchedule
        |> List.map (fun team -> team.LolTeam)

    let playoffTeams =
        findPlayoffTeams teamRecords
        |> List.map (fun team -> team.LolTeam)

    let assignPlayoffStatus team =
        let containsTeam =
            [ eliminatedTeams; playoffTeams]
            |> List.map (List.contains team.LolTeam)

        match containsTeam with
        | _::[x] when x -> { Status = Clinched; Team = team }
        | x::_ when x -> { Status = Eliminated; Team = team }
        | _ -> { Status = Unknown; Team = team }

    teamRecords
    |> List.map assignPlayoffStatus

let getMatchups league team : Matchup list =
    let lolResults = 
        LeagueSchedule.getSchedule league
        |> Array.filter (fun event -> event.State = LeagueSchedule.StateCompleted)
        |> Array.map LeagueSchedule.create

    Matchups.create team lolResults
    |> List.sortBy (fun matchup -> matchup.Team.Code)
    |> List.sortBy (fun matchup -> matchup.Result)

let getSplitTitle league : string =
    let leagueType = getLeagueType league
    LeagueTournament.currentSplitSeason leagueType

let createTeamMatchup league team =
    let matchups = getMatchups league team
    TeamMatchups.create league matchups team

let createTeamMatchupByCode (league, teamCode: string) =
    let team = { Name = ""; Code = teamCode.ToUpper(); Image = "" }
    let leagueType = getLeagueType league
    createTeamMatchup leagueType team

let createAllMatchups league =
    let leagueType = getLeagueType league

    let teams = LolTeam.LolTeams leagueType
    
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
        |> Array.map (createTeamMatchup leagueType)

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