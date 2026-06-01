module TermProject.Types

type GameState =
  | Lobby
  | Store
  | InGame
  | GameOver // prompting chance to revive
  | Quit
(*
  Lobby → Store, InGame
  Store → Lobby
  InGame → GameOver
  GameOver → InGame (when revived), Lobby
*)

type Position = int * int // position for playing character & stairs

type Direction =
  | Left
  | Right

module Direction =
  let delta =
    function
    | Left -> -1
    | Right -> 1

  let opposite =
    function
    | Left -> Right
    | Right -> Left

type CoinType =
  | CoinNone
  | Coin1
  | Coin5
  override this.ToString() =
    match this with
    | CoinNone -> ""
    | Coin1 -> "🪙"
    | Coin5 -> "🌕"
