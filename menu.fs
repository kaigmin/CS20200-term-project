module TermProject.Menu

open System
open Spectre.Console
open Spectre.Console.Rendering
open TermProject.Types
open TermProject.Characters
open TermProject.player

type PromptMessage =
  | StartGame
  | StartWithBoost
  | Store
  | Exit

  override this.ToString() =
    match this with
    | StartGame -> "Start Game"
    | StartWithBoost -> "Start with Boost! (🪙 50)"
    | Store -> "Character Store"
    | Exit -> "Exit"

let showLobby (player: PlayerInfo) : PlayerInfo * GameState =
  AnsiConsole.Clear()

  // Title
  let title = new FigletText("Infinite Pixels")
  title.Color <- Color.Aqua
  AnsiConsole.Write(title :> IRenderable)
  AnsiConsole.WriteLine()

  // Stats Table
  let statsTable = new Table()

  // assigned each column since `new` used
  let col1 = new TableColumn("[yellow]Balance[/]")
  let col2 = new TableColumn("[green]High Score[/]")
  let col3 = new TableColumn("[cyan]Last Score[/]")
  let col4 = new TableColumn("[blue]Character[/]")

  statsTable.AddColumn col1 |> ignore
  statsTable.AddColumn col2 |> ignore
  statsTable.AddColumn col3 |> ignore
  statsTable.AddColumn col4 |> ignore

  let lastScoreStr = 
    match player.PreviousScore with
    | Some s -> string s
    | None -> "-"

  statsTable.AddRow(
    [| $"🪙 {player.Balance}"
       $"{player.HighScore}"
       lastScoreStr
       player.CurrentCharacter.Marker.DefaultRight |> Option.defaultValue "🏃" |]
  )
  |> ignore

  AnsiConsole.Write(statsTable :> IRenderable)
  AnsiConsole.WriteLine()

  // Menu Prompt
  let prompt = new SelectionPrompt<PromptMessage>()
  prompt.Title <- "[white]Select an option:[/]"
  prompt.PageSize <- 10

  let choices =
    if player.HighScore >= 100 then
      [| StartGame; StartWithBoost; Store; Exit |]
    else
      [| StartGame; Store; Exit |]

  prompt.AddChoices(choices) |> ignore

  let choice = AnsiConsole.Prompt(prompt)

  match choice with
  | StartGame -> ({ player with CurrentScore = 0 }, GameState.InGame)
  | StartWithBoost ->
    if player.Balance >= 50 then
      let boostScore = (player.HighScore / 50) * 20
      ({ player with
          Balance = player.Balance - 50
          CurrentScore = boostScore }, GameState.InGame)
    else
      AnsiConsole.MarkupLine("[red]Not enough coins![/]")
      Threading.Thread.Sleep(1000)
      (player, GameState.Lobby)
  | Store -> (player, GameState.Store)
  | Exit -> (player, GameState.Quit)

let showStore (player: PlayerInfo) : PlayerInfo * GameState =
  AnsiConsole.Clear()
  let rule = new Rule("[yellow]Character Store[/]")
  rule.Style <- Style.Parse("yellow")
  AnsiConsole.Write(rule :> IRenderable)
  AnsiConsole.WriteLine()
  AnsiConsole.MarkupLine(sprintf "[yellow]Your Currency: %d 🪙[/]" player.Balance)
  AnsiConsole.WriteLine()

  let promptChoices =
    player.AvailableCharacters
    |> List.mapi (fun i c ->
      let emoji = c.Marker.DefaultRight |> Option.defaultValue "🏃"

      let status =
        if
          c.Marker = player.CurrentCharacter.Marker
          && c.Price = player.CurrentCharacter.Price
        then
          "[green](Equipped)[/]"
        elif c.Unlocked then
          "[blue](Unlocked)[/]"
        else
          sprintf "[yellow](%d 🪙)[/]" c.Price

      sprintf "%d. %s - %s" (i + 1) emoji status)

  let choices = promptChoices @ [ "Back to Lobby" ]
  let prompt = new SelectionPrompt<string>()
  prompt.Title <- "Select a character to buy/equip:"
  prompt.PageSize <- 15
  prompt.AddChoices(choices |> List.toArray) |> ignore

  let choice = AnsiConsole.Prompt(prompt)

  if choice = "Back to Lobby" then
    (player, GameState.Lobby)
  else
    let idxStr = choice.Substring(0, choice.IndexOf('.'))
    let idx = int idxStr - 1
    let selectedChar = player.AvailableCharacters.[idx]

    let player' =
      if
        selectedChar.Marker = player.CurrentCharacter.Marker
        && selectedChar.Price = player.CurrentCharacter.Price
      then
        AnsiConsole.MarkupLine("[green]Already equipped![/]")
        System.Threading.Thread.Sleep(1000)
        player
      else if selectedChar.Unlocked then
        AnsiConsole.MarkupLine("[green]Equipped character![/]")
        System.Threading.Thread.Sleep(1000)

        { player with
            CurrentCharacter = selectedChar }
      else if player.Balance >= selectedChar.Price then
        AnsiConsole.MarkupLine("[green]Character bought and equipped![/]")
        System.Threading.Thread.Sleep(1000)
        let updatedChar = { selectedChar with Unlocked = true }

        let updatedAvailable =
          player.AvailableCharacters
          |> List.map (fun c ->
            if c.Price = selectedChar.Price && c.Marker = selectedChar.Marker
              then updatedChar else c)

        { player with
            Balance = player.Balance - selectedChar.Price
            AvailableCharacters = updatedAvailable
            CurrentCharacter = updatedChar }
      else
        AnsiConsole.MarkupLine("[red]Not enough currency![/]")
        System.Threading.Thread.Sleep(1000)
        player

    (player', GameState.Store)
