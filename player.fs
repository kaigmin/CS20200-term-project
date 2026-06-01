module TermProject.player

open Types
open Characters

type PlayerInfo =
  {
    // general data
    CurrentCharacter: Character
    AvailableCharacters: Character list
    HighScore: int
    PreviousScore: int option
    Balance: int

    // in-game data
    Direction: Direction
    Position: Position
    CurrentScore: int
    Stamina: float // fixed definition as remaining real-time to survive
    MaxStamina: float
    RevivalCount: int // revival boost used in a single run

    // persistence data for revival
    IsRevived: bool
    BoardStairs: (Position * CoinType) list
    StairGenState: Direction * int
    
    // terminal settings; is it okay to be here?
    TerminalSize: {| Width: int; Height: int |} }
