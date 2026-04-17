# Maze Runner

A first-person 3D horror maze game built in Unity 2022.3 LTS for a Game Jam event.

**Theme:** Pathfinding

You're trapped in a procedurally generated maze. Your flashlight is dying. Find the exit — before the dark finds you.

## Gameplay

You wake up in a pitch-black maze with nothing but a flashlight. The maze is different every time. Walls surround you. Something is breathing in the dark. Your only goal: find the glowing exit before your flashlight dies — or before whatever's hunting you gets close enough.

## Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look around |
| Left Shift | Sprint (drains battery faster) |
| F | Toggle flashlight on/off |
| Tab | View clues (if implemented) |
| R | Restart (after game over) |
| Esc | Pause |

## Mechanics

* **Dying Flashlight** — Your flashlight has a 4-minute battery. As it drains, the beam narrows, dims, and eventually starts flickering. At 5% it strobes. At 0% you have 5 seconds of grace in total darkness before death.
* **Battery Conservation** — Toggle the flashlight off with F to slow drain to 10%. Sprint doubles drain rate. Managing your battery is the core survival decision.
* **Procedural Maze** — Generated using recursive backtracker algorithm. Every run is a different layout. Maze size is 15×15 cells, each cell 4×4 units with 3-unit walls.
* **The Creature (Optional)** — A shadowy figure with glowing red eyes spawns 60 seconds into the run. It wanders toward you. If it gets line of sight, it charges. You cannot fight it. You can only run.
* **Head Bob** — Camera bobs subtly while walking, faster while sprinting. Adds embodied presence to movement.
* **Random Scare Sounds** — Distant door slams, metal scrapes, and whispers play from random directions every 25–60 seconds. They mean nothing mechanically. They mean everything psychologically.

## Scoring

* Escape time is tracked and displayed on the win screen
* Faster escapes = better runs
* Each playthrough generates a unique maze seed

## Screenshots

The game features:

* Procedurally generated 3D maze with dark concrete/stone walls
* First-person flashlight as the only light source
* Volumetric fog that swallows the beam at distance
* Creature with glowing red eyes lurking in corridors
* Post-processing: heavy vignette, film grain, chromatic aberration near creature
* Flashlight degradation: dimming → narrowing → flickering → strobing → death
* Glowing blue-white exit beacon visible only when close
* Camera shake on death
* Minimalist start screen and game over screen
* No HUD — immersion over information

## Tech Stack

* **Engine:** Unity 2022.3 LTS (3D URP)
* **Language:** C#
* **Art:** All geometry built from Unity primitives (cubes, planes, capsules). No external 3D models.
* **Lighting:** Single spotlight (flashlight) + point lights (exit glow, creature eyes)
* **Audio:** Spatial 3D audio for creature, 2D for ambient and footsteps
* **Platform:** Windows / macOS / WebGL

## Project Structure

```
MazeRunner/Assets/Scripts/
├── MazeGenerator.cs        — Recursive backtracker maze generation + 3D wall building
├── PlayerController.cs     — First-person movement, mouse look, sprint, head bob
├── Flashlight.cs           — Battery drain, light degradation, flicker, toggle
├── GameManager.cs          — Game states, win/lose conditions, UI management
├── ExitTrigger.cs          — Detects player reaching the exit
├── CreatureAI.cs           — Enemy wandering, chasing, line-of-sight detection
├── FootstepAudio.cs        — Biome-aware footstep sounds with pitch variation
├── RandomScares.cs         — Random directional horror sound cues
├── CreatureDistortion.cs   — Post-processing distortion when creature is near
├── StartMenu.cs            — Start screen, loads game on Enter/Space
```

## How to Run

1. Open the project in Unity 2022.3 LTS (URP)
2. Open `Assets/Scenes/StartScene.unity`
3. Hit Play
4. Press ENTER or SPACE to start
5. Survive.

## How to Build

1. File → Build Settings
2. Add scenes: `StartScene` (index 0), `GameScene` (index 1)
3. Select platform (PC/Mac/WebGL)
4. Click Build

For WebGL: Player Settings → Publishing Settings → Compression: Gzip

## Credits

Built for Game Jam 2026

