module TermProject.Board

open Types
open System

type StairGenerationState = Direction * int 
// current direction, left steps to determine whether to reverse dir.

type Stair() =
  let rng = Random()

  member val aliveStairs: (Position * CoinType) list = [] with get, set

  member this.nextStair(state: StairGenerationState) : StairGenerationState =
    let currentDir, leftSteps = state

    let nextDir, nextLeftSteps =
      if leftSteps > 0 then (currentDir, leftSteps - 1) else
        let newDir =
          if rng.Next 2 = 0 then
            Direction.opposite currentDir
          else
            currentDir
        (newDir, rng.Next (0, 4))
        
    let nextPos =
      match this.aliveStairs with
      | [] -> (0, 0)
      | ((lastX, lastY), _) :: _ -> (lastX + Direction.delta nextDir, lastY + 1)

    let coinRoll = rng.NextDouble()

    let coinType =
      if coinRoll < 0.02 then Coin5
      elif coinRoll < 0.12 then Coin1
      else CoinNone

    let updatedStairs = (nextPos, coinType) :: this.aliveStairs

    this.aliveStairs <-
      if updatedStairs.Length > 100 then
        List.take 100 updatedStairs
      else
        updatedStairs

    (nextDir, nextLeftSteps)

  member this.initStairs(height: int) =
    let mutable state = (Direction.Left, rng.Next(1, 5))
    this.aliveStairs <- [ ((0, 0), CoinNone) ]

    for _ in 1..height do
      state <- this.nextStair (state)

    state
