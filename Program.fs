module TermProject.Program

open System
open TermProject.Types
open TermProject.Characters
open TermProject.player
open TermProject.Menu
open TermProject.Game
open TermProject.Revival
open Spectre.Console

[<EntryPoint>]
let main argv =
  Console.OutputEncoding <- Text.Encoding.UTF8

  let mutable player =
    { CurrentCharacter = List.head characterList
      AvailableCharacters = characterList
      HighScore = 0
      PreviousScore = None
      Balance = 150

      Direction = Direction.Left
      Position = (0, 0)
      CurrentScore = 0
      Stamina = 100.0
      MaxStamina = 100.0
      RevivalCount = 0

      IsRevived = false
      BoardStairs = []
      StairGenState = (Direction.Left, 0)

      TerminalSize = {| Width = 80; Height = 25 |} }

  let mutable currentState = GameState.Lobby
  let mutable keepRunning = true

  while keepRunning do
    match currentState with
    | GameState.Lobby ->
        let p, nextState = showLobby player
        player <- p
        currentState <- nextState
    | GameState.Store ->
        let p, nextState = showStore player
        player <- p
        currentState <- nextState
    | GameState.InGame ->
        let p, nextState = runInGame player
        player <- p
        currentState <- nextState
    | GameState.GameOver ->
        let p, nextState = askRevival player
        player <- p
        currentState <- nextState
    | GameState.Quit -> keepRunning <- false

  AnsiConsole.MarkupLine("[bold red]Exiting Game. Goodbye![/]")
  0
