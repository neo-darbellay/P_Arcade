# P_Arcade

## Overview

P_Arcade is a C# console application that bundles multiple games into a single terminal interface.
It includes an interactive selection menu, per-game option screens, an about section for each game, and a high score system for supported titles.

---

## Games

| Game           | About section | High Scores |
| -------------- | ------------- | ----------- |
| 2048           | Yes           | Yes         |
| Connect 4      | Yes           | No          |
| Copy A Drawing | Yes           | No          |
| MineSweeper    | Yes           | No          |
| Simon          | Yes           | Yes         |
| Sliding Puzzle | Yes           | No          |
| Snake          | Yes           | Yes         |
| Tic Tac Toe    | Yes           | No          |
| Yahtzee        | Yes           | No          |

---

## Getting Started

### Requirements

- **Visual Studio** with the **.NET Desktop Development** workload installed
- A computer running **Windows 11** (support for Windows 10 is not guaranteed)

### Setup

1. Open the `P_Arcade` folder
2. Open the solution file: `P_Arcade.sln`
3. Press **Ctrl + F5** to build and launch the application

The compiled executable can be found at `P_Arcade/bin/Debug` after building.

---

## Navigation

### Main Menu

- **Up / Down Arrow Keys** to move between options
- **Number Keys** to jump directly to a numbered option
- **Enter** or **Spacebar** to confirm a selection
- **Escape** to go back or exit

### In-Game

- Most games use **Arrow Keys** or **WASD** for movement
- **Escape** exits back to the menu from any game or setup screen
- Each game's About section lists its specific controls and other information about them

---

## High Scores

Games that support high scores save them locally as XML files in the same folder as the executable, using the naming format `GAMENAME_highscores.xml`.

Scores are saved automatically after each run and persist between sessions.

---

## Notes

- P_Arcade runs entirely in the Windows console with no external dependencies
- Some games automatically adjust the terminal zoom level if the board is too large to fit on screen
- Yahtzee supports 2 to 4 players on the same machine
- Connect 4 includes a bot opponent with 10 difficulty levels
