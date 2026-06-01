# Infinite Pixels: "Infinite Stairs" cloned in CLI

## Overview

_Infinite Pixels_ is a console-based arcade game inspired by [Infinite Stairs](https://play.google.com/store/apps/details?id=com.nflystudio.InfiniteStaircase), a casual mobile game that was widely popular in Korea.

The player climbs an endlessly generated zigzag staircase as high as possible. At each step, the player must quickly decide whether to change the character's facing direction or keep it, then press the corresponding arrow key before a stamina gauge runs out. Along the way, coins appear on certain stairs and can be collected to unlock new character skins or to pay for revival after a failed attempt.

---

## Setup & Execution

### Prerequisites

The project was developed with .NET 10 and F#. Also, it involves **Spectre.Console** library for high-abstracted and elegant terminal rendering and handling user inputs.

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Verify the installation, type `dotnet --version` to check whether the correct version of .NET is installed. 
The output should display a version starting with `10`.

If so, type `dotnet restore` to install the required packages.

### Running the Game

After obtaining source folder thru `git clone ...` or manual download, you can run the game simply by 
opening a terminal and typing the commands below at the root of project directory:

```bash
dotnet build
dotnet run
```

On the first run, the NuGet package (Spectre.Console) might be restored automatically. This may take a few seconds.

> [!WARNING]
> For the best visual experience, use a terminal that supports **Unicode emoji rendering and ANSI escape codes**
  (e.g., iTerm2 on macOS, Windows Terminal or Powershell on Windows). The default terminal size assumed by the game is **at least 80×24**; resizing may affect rendering.

---

## How to Play

### 1. Lobby

When the game starts, you are greeted with the **lobby screen**. It displays:

| Info         | Description                                       |
| ------------ | ------------------------------------------------- |
| **Currency** | Your total coins (🪙). Earned by collecting coins during gameplay. |
| **High Score** | The highest score you have achieved across all runs. |
| **Last Score** | The score from your most recent run (`-` if none). |
| **Character** | Your currently equipped character emoji.           |

From the lobby, select one of the following options using the arrow keys and Enter:

- **Start Game**: Begin a new run.
- **Character Store**: Browse, unlock, and equip characters.
- **Exit**: Quit the program.

### 2. Character Store

In the store, you can spend your coins to **unlock** new character emojis and **equip** any unlocked character. Characters vary in price and appearance:

| Character | Price | Notes |
| --------- | ----- | ----- |
| 🚶 / 🏃 | Free  | Man emoji (default) |
| 🚶‍♂️ / 🏃‍♂️ | 50 🪙 | Alternate man emoji |
| 🤜 / 🤛 | 50 🪙 | Fist emojis with direction |
| 🤷‍♀️ | 67 🪙 | sIx SeVeN |
| 👾 | 100 🪙 | An alien |
| 🌛 / 🌜 | 100 🪙 | Moon faces with direction |
| 🦈 | 100 🪙 | Just an ordinary shark |

Some characters do not change their facing direction regardless of movement, due to the limitations of the emoji system. Once a character is unlocked, it stays unlocked permanently as long as the game process keeps alive.

### 3. In-Game

#### Controls

| Key | Action |
| --- | ------ |
| ← (Left Arrow) | Change facing direction and climb one stair |
| → (Right Arrow) | Keep current facing direction and climb one stair |

#### HUD (Top of Screen)

- **First row** indicates Current score (center), coin count (right).
- **Second row** indicates Stamina gauge — a red bar that drains over time. Its length represents `Stamina / MaxStamina`.

#### Gameplay Mechanics

- **Stairs** are rendered as zigzag pattern of `▀` (upper half block) characters, with the character emoji at a fixed screen position.
- **Coins** (🪙 = 1 coin, 🌕 = 5 coins) appear onwards random stairs with a probability of ~10% and ~2%, respectively.
- **Stamina** begins draining once you take your first keystroke. Each successive step recovers a portion of stamina (starting at 0.5s, decreasing gradually as you climb higher). If stamina reaches zero, the game ends.
- **MaxStamina** decreases slowly after score 20, creating increasing pressure as the run progresses.
- **Wrong direction:** If you step in the wrong direction (i.e., no stair exists at the expected position), the game ends immediately with a ❗ indicator.

### 4. Revival

When the game ends (either by wrong step or running out of stamina), you are presented with a **revival prompt** instead of immediately returning to the lobby:

- If you have enough coins, you can choose **Yes** or **No** using the left/right arrow keys.
  - The revival cost is displayed below: `(🪙 20)` for the first revival, doubling each time. (`20 → 40 → 80 → ...`).
- If you don't have enough coins, a **"Not enough coins!"** message is shown, and pressing any key returns you to the lobby.

Upon revival:
- Stamina is fully restored to its current maximum.
- The staircase state is preserved — you continue from exactly where you left off.
- Stamina does not drain until you take your next keystroke, just like at the start of a run.
- Score and MaxStamina remain unchanged from the moment of failure.

---

## Requirement Changes from Proposal

### Removed Requirement 5: Custom Key Bindings

This requirement was marked as "Unsure to be implemented in final version" in the proposal itself. It was decided not to be ultimately implemented.

### Changed: Requirement 1 — Lobby Start Control

In the current implementation, the lobby uses Spectre.Console's `SelectionPrompt`, where the player navigates with arrow keys and confirms with Enter, rather than a single space key press. This change was conducted to make full use of and respect Spectre library's features and API, rather than implementing a custom lobby system from scratch.
Besides, I thought this decision does not critically ruin user experience because the cursor is hovered at "Start Game" button as default, and it takes only a single press of space or enter key to start.

### Changed: Logics about vertical coordinate of Player's Character

Requirement 2 of the proposal described that the character's on-screen Y position should begin near the bottom of the terminal and gradually rise to the center over the first several steps, after which it would remain fixed. In the final implementation, however, the character's screen position is **fully fixed** at approximately 85% of the terminal height from the very beginning of each run — there is no initial upward animation.

This change was made to better match the actual user experience of the original *Infinite Stairs* mobile game. In practice, the gradually rising camera adds visual noise during the first few inputs and makes it harder for the player to immediately get a feel for the stair layout. Keeping the camera anchor stationary from the start results in a cleaner, more responsive feel that is closer to what players expect from the reference game. (TODO)

### Added: Last Score Display in Lobby

The lobby now shows the score from the player's most recent run. This was not in the original proposal but was added to improve the user experience by giving immediate feedback after each attempt.

### Modified & Added: Revival "Game Over" Title

The revival screen now displays a "Game Over!" message above the revival prompt, providing clearer visual feedback about the game state.

---

## About LLM Attributions

### Boundary of LLM usage in this project

Basically, I designed every fundamental structures of the game: signatures of data types for characters, player environment, stair generating algorithm, rendering strategy and most of game logics, utilizing AI agent system partially for studying ideas or more efficient implementation. Detailed usage examples:

- I first implemented core logics for every-next stair generation and movement physics. Then, I asked to Gemini Code Assistant to port the logic in the form of member, and to add some details & modifications e.g. using `Random()` function for random behaviors like changing direction of staircase and spawning coin, instead of `Seq.random~~` methods as my rough sketch.

- I got suggested by Claude model for overall early UI structure, lobby page in particular, since I was not familiar with Spectre library so far yet. I adopted suggestions and retouched the contents on my own.

- I asked some usual game loop patterns for such TUI platformer games like this project, and I got recommended one which was similar pattern matching strategy likewise Homework #6. This idea helped to prioritize insights from code skeleton in previous lecture notes and homeworks.


### Hardships and Limitations of LLM usage

- The LLM initially generated code with 4-space indentation, which conflicted with the project's 2-space standard in my flavor. Multiple rounds of correction were needed, and at one point I tried an automated formatting tool (`fantomas`) by its suggestion, but it rather caused file corruption, deleting some files of code like `types.fs` and `board.fs` completely. I managed to restore them by the context restore feature in the Antigravity IDE.

- The precise refinement of placement of lobby screen on the terminal screen required multiple iterations of reprompting with far detailed specifications.

- Autocomplete feature in my IDE sometimes suggested annoying snnippets not making sense to me, besides it also tried to change the hard-coded value like stamina draining rate whenever I edited any part of code nearby. After then, I turned off the autocomplete feature.

---

## Project Structure

```
./
├── types.fs
├── characters.fs
├── player.fs
├── board.fs
├── menu.fs
├── game.fs
├── revival.fs
├── Program.fs
├── term-project.fsproj
└── README.md
```
