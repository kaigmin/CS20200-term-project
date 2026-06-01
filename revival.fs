module TermProject.Revival

open System
open TermProject.Types
open TermProject.player

let askRevival (player: PlayerInfo) : PlayerInfo * GameState =
  let cost = 20 * (pown 2 player.RevivalCount)
  
  let mutable keepPrompting = true
  let mutable selectedOption = 0 // 0: Yes, 1: No
  let mutable nextState = GameState.Lobby
  let mutable finalPlayer = player
  
  for y in 2 .. player.TerminalSize.Height - 1 do
    Console.SetCursorPosition(0, y)
    Console.Write("\x1b[2K")
    
  let promptY = player.TerminalSize.Height / 2 - 2
  
  let drawUI () =
    if player.Balance < cost then
      let msg = "Not enough coins!"
      Console.SetCursorPosition(player.TerminalSize.Width / 2 - msg.Length / 2, promptY)
      Console.Write(msg)
      
      let costMsg = sprintf "(🪙 %d)" cost
      Console.SetCursorPosition(player.TerminalSize.Width / 2 - (costMsg.Length - 1) / 2, promptY + 2)
      Console.Write(costMsg)
    else
      let msg = "Game Over!"
      let msg2 = "Do you want to revive?"
      Console.SetCursorPosition(player.TerminalSize.Width / 2 - msg.Length / 2, promptY - 2)
      Console.Write(msg)
      Console.SetCursorPosition(player.TerminalSize.Width / 2 - msg2.Length / 2, promptY)
      Console.Write(msg2)
      
      let yesBtn = "[ Yes ]"
      let noBtn = "[ No ]"
      
      let totalWidth = yesBtn.Length + noBtn.Length + 4
      let startX = player.TerminalSize.Width / 2 - totalWidth / 2
      
      Console.SetCursorPosition(startX, promptY + 2)
      if selectedOption = 0 then
        Console.Write("\x1b[33m" + yesBtn + "\x1b[0m")
      else
        Console.Write(yesBtn)
        
      Console.SetCursorPosition(startX + yesBtn.Length + 4, promptY + 2)
      if selectedOption = 1 then
        Console.Write("\x1b[31m" + noBtn + "\x1b[0m")
      else
        Console.Write(noBtn)
        
      let costMsg = sprintf "(🪙 %d)" cost
      let costX = startX + (yesBtn.Length / 2) - ((costMsg.Length - 1) / 2)
      Console.SetCursorPosition(max 0 costX, promptY + 4)
      Console.Write(costMsg)
      
  drawUI()
  
  while keepPrompting do
    if Console.KeyAvailable then
      let key = Console.ReadKey(true).Key
      if player.Balance < cost then
        if key = ConsoleKey.Enter || key = ConsoleKey.Spacebar || key = ConsoleKey.Escape then
          keepPrompting <- false
      else
        match key with
        | ConsoleKey.LeftArrow -> 
          selectedOption <- 0
          drawUI()
        | ConsoleKey.RightArrow -> 
          selectedOption <- 1
          drawUI()
        | ConsoleKey.Enter | ConsoleKey.Spacebar ->
          if selectedOption = 0 then
            finalPlayer <- 
              { player with
                  Balance = player.Balance - cost
                  RevivalCount = player.RevivalCount + 1
                  IsRevived = true }
            nextState <- GameState.InGame
          else
            nextState <- GameState.Lobby
          keepPrompting <- false
        | _ -> ()
        
    System.Threading.Thread.Sleep(50)
    
  (finalPlayer, nextState)
