namespace Models

open System


type EventState = Completed | Unstarted

type LeagueSeason = 
    { Name: string
      Slug: string }

type MatchResult =
    { Outcome: string
      GameWins: int }

type Record =
    { Wins: int
      Losses: int }

type Team =
    { Name: string
      Code: string
      Result: MatchResult
      Record: Record }

type Strategy =
    { Type: string
      Count: int }

type Match =
    { Id: int64 
      Teams: Team list
      Strategy: Strategy }

type LeagueEvent =
    { StartTime: DateTimeOffset
      State: EventState
      Type: string
      BlockName: string
      League: LeagueSeason
      Match: Match }