module TermProject.Game

open System
open TermProject.Types
open TermProject.Characters
open TermProject.player
open TermProject.Board

let runInGame (initialPlayer: PlayerInfo) : PlayerInfo * GameState =
  Console.Clear()
  Console.CursorVisible <- false

  let board = Stair()
  let mutable stairState = (Direction.Left, 0)
  let mutable player = initialPlayer

  let fps = 60
  let frameDuration = TimeSpan.FromMilliseconds(1000.0 / float fps)
  let mutable lastFrameTime = DateTime.Now
  let mutable gameRunning = true
  let mutable hasStarted = false

  if initialPlayer.IsRevived then
    board.aliveStairs <- initialPlayer.BoardStairs
    stairState <- initialPlayer.StairGenState

    player <-
      { initialPlayer with
          IsRevived = false
          Stamina = initialPlayer.MaxStamina }
  else
    let boostScore = initialPlayer.CurrentScore
    stairState <- board.initStairs (boostScore + initialPlayer.TerminalSize.Height)
    let startPos =
      if boostScore > 0 then
        board.aliveStairs
        |> List.tryFind (fun ((_, y), _) -> y = boostScore)
        |> Option.map fst
        |> Option.defaultValue (0, 0)
      else (0, 0)

    let boostedMaxStamina =
      if boostScore > 0 then
        max 2.0 (10.0 - float (boostScore - 20) * 0.005)
      else 10.0

    let startDir = // ensuring right keystroke as correct first step after a boost
      if boostScore > 0 then
        let sx = fst startPos
        board.aliveStairs
        |> List.tryFind (fun ((_, y), _) -> y = boostScore + 1)
        |> Option.map (fun ((nx, _), _) ->
          if nx > sx then Direction.Right else Direction.Left)
        |> Option.defaultValue Direction.Left
      else Direction.Left

    player <-
      { initialPlayer with
          Direction = startDir
          Position = startPos
          CurrentScore = boostScore
          MaxStamina = boostedMaxStamina
          Stamina = boostedMaxStamina
          RevivalCount = 0 }

  let mutable lastActionTime = DateTime.Now.AddSeconds(-1.0)
  let actionDuration = TimeSpan.FromSeconds(0.2)

  let render () =
    Console.SetCursorPosition(0, 0)
    Console.Write("\x1b[2K")
    let scoreText = sprintf "Score: %d" player.CurrentScore
    let currencyText = sprintf "🪙 %d" player.Balance

    Console.SetCursorPosition(player.TerminalSize.Width / 2 - scoreText.Length / 2, 0)
    Console.Write(scoreText)
    Console.SetCursorPosition(max 0 (player.TerminalSize.Width - currencyText.Length - 3), 0)
    Console.Write(currencyText)

    Console.SetCursorPosition(0, 1)
    Console.Write("\x1b[2K")
    let barWidth = player.TerminalSize.Width - 2
    let fillRatio = max 0.0 (min 1.0 (player.Stamina / player.MaxStamina))
    let fillLength = int (Math.Round(float barWidth * fillRatio))
    Console.SetCursorPosition(1, 1)

    Console.Write(
      "\x1b[31m"
      + String('█', fillLength)
      + String('░', barWidth - fillLength)
      + "\x1b[0m"
    )

    for y in 2 .. player.TerminalSize.Height - 1 do
      Console.SetCursorPosition(0, y)
      Console.Write("\x1b[2K")

    let charRenderY = int (float player.TerminalSize.Height * 0.85)
    let charRenderX = player.TerminalSize.Width / 2
    let (pX, pY) = player.Position

    for ((sx, sy), coin) in board.aliveStairs do
      let diffY = sy - pY
      let diffX = sx - pX
      let stairRenderY = charRenderY - diffY + 1
      let coinRenderY = stairRenderY - 1
      let renderX = charRenderX + (diffX * 4)

      if
        stairRenderY >= 2
        && stairRenderY < player.TerminalSize.Height
        && renderX >= 0
        && renderX < player.TerminalSize.Width - 1
      then
        Console.SetCursorPosition(renderX, stairRenderY)
        Console.Write("▀")

      if
        coinRenderY >= 2
        && coinRenderY < player.TerminalSize.Height
        && renderX >= 0
        && renderX < player.TerminalSize.Width - 1
      then
        match coin with
        | CoinType.Coin1 ->
            Console.SetCursorPosition(renderX, coinRenderY)
            Console.Write("🪙")
        | CoinType.Coin5 ->
            Console.SetCursorPosition(renderX, coinRenderY)
            Console.Write("🌕")
        | CoinType.CoinNone -> ()

    Console.SetCursorPosition(charRenderX, charRenderY)
    let isAction = (DateTime.Now - lastActionTime) < actionDuration
    let marker = player.CurrentCharacter.Marker

    let charStr =
      match player.Direction with
      | Direction.Left ->
          if isAction then
            (marker.ActionLeft |> Option.defaultValue "🏃")
          else
            (marker.DefaultLeft |> Option.defaultValue "🚶")
      | Direction.Right ->
          if isAction then
            (marker.ActionRight |> Option.defaultValue "🏃‍➡️")
          else
            (marker.DefaultRight |> Option.defaultValue "🚶‍➡️")

    Console.Write(charStr)

  render ()

  while gameRunning do
    let now = DateTime.Now
    let delta = now - lastFrameTime

    if delta >= frameDuration then
      lastFrameTime <- now

      if hasStarted then
        player <-
          { player with Stamina = player.Stamina - delta.TotalSeconds }

        if player.Stamina <= 0.0 then
          gameRunning <- false

      if Console.KeyAvailable then
        hasStarted <- true
        let key = Console.ReadKey(true).Key

        let mutable moveProcessed = false
        let mutable nextExpectedDir = player.Direction

        match key with
        | ConsoleKey.LeftArrow ->
            nextExpectedDir <- Direction.opposite player.Direction
            moveProcessed <- true
        | ConsoleKey.RightArrow ->
            nextExpectedDir <- player.Direction
            moveProcessed <- true
        | _ -> ()

        if moveProcessed then
          let (pX, pY) = player.Position
          let expectedNextPos = (pX + Direction.delta nextExpectedDir, pY + 1)

          let matchedStair =
            board.aliveStairs |> List.tryFind (fun (pos, _) -> pos = expectedNextPos)

          match matchedStair with
          | Some(_, coin) ->
              player <-
                { player with
                    Position = expectedNextPos
                    Direction = nextExpectedDir
                    CurrentScore = player.CurrentScore + 1 }

              match coin with
              | CoinType.CoinNone -> ()
              | CoinType.Coin1 ->
                player <-
                  { player with
                      Balance = player.Balance + 1 }
              | CoinType.Coin5 ->
                player <-
                  { player with
                      Balance = player.Balance + 5 }

              board.aliveStairs <-
                board.aliveStairs
                |> List.map (fun (pos, c) ->
                  if pos = expectedNextPos then
                    (pos, CoinType.CoinNone)
                  else
                    (pos, c))

              let gainStamina = max 0.05 (0.5 - float player.CurrentScore * 0.0005)
  
              player <-
                { player with
                    Stamina = min player.MaxStamina (player.Stamina + gainStamina) }

              if player.CurrentScore > 20 then
                player <-
                  { player with
                      MaxStamina = max 2.0 (player.MaxStamina - 0.005) }

              stairState <- board.nextStair (stairState)
              lastActionTime <- DateTime.Now

          | Option.None ->
              let charRenderY = int (float player.TerminalSize.Height * 0.85)
              let charRenderX = player.TerminalSize.Width / 2
              Console.SetCursorPosition(charRenderX, charRenderY - 1)
              Console.Write("❗")
              Threading.Thread.Sleep(500)
              gameRunning <- false

      render ()

  if player.Stamina <= 0.0 then
    let charRenderY = int (float player.TerminalSize.Height * 0.85)
    let charRenderX = player.TerminalSize.Width / 2
    Console.SetCursorPosition(charRenderX, charRenderY - 1)
    Console.Write("❗")
    Threading.Thread.Sleep(500)

  Console.CursorVisible <- true

  if player.CurrentScore > player.HighScore then
    player <-
      { player with
          HighScore = player.CurrentScore }

  player <-
    { player with
        PreviousScore = Some player.CurrentScore
        BoardStairs = board.aliveStairs
        StairGenState = stairState }

  (player, GameState.GameOver)
