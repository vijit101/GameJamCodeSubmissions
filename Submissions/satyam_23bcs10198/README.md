# Ariadne

A 3D first-person labyrinth about Theseus, the Minotaur, and the glowing thread. Built for the **BACKTRACKING** theme.

## Theme interpretation
- **BACKTRACKING** made literal. Each step forward pushes a point onto a visible DFS stack — a glowing amber thread unspooling behind the player in world space. Each retrace pops the stack: segments retract, the player slides back through the labyrinth.
- **Two-phase myth-loop**: **SEARCH** (walk in, find the amber prize at the heart) → **ESCAPE** (pull yourself home along the thread before the Minotaur catches you). Reaching the prize is the midpoint, not the end — you win by making it back to the entrance.
- **Gems as scatter currency**: collect cyan gems from dead-end detours; spend one to scatter the Minotaur when he draws near. The DFS-stack-as-resource turns exploration into risk/reward management.

## Tech stack
- **Unity 6000.4.2f1**, URP (3D)
- 100% C#, **~3500 LOC across 18 scripts** (1600 LOC of 3D-specific runtime + 900 LOC of headless editor tooling)
- WebGL build, uncompressed for static itch.io hosting (**19 MB zipped / 98 MB on disk**)
- **No imported 3D assets** — the entire 25×24 labyrinth is built at runtime from Unity primitives (cubes, cylinders, spheres) with per-instance material tinting for emissive tiles, wall variation, gem glow, etc.
- Lore narration, HUD, pause menu, and minimap all rendered via code-driven Unity UI (Canvas + Text + RawImage + dynamic Texture2D)
- Headless scene generation + WebGL build via `-executeMethod` so everything is reproducible from a single command

## Assets & credits
- All 3D geometry in this build is Unity primitives — no imports
- Intro narration and five stone-tablet whispers written for the project
- Kenney Tiny Town 2D sprites (CC0) used in sibling branches (2D jungle, rhythm runner) — not shipped in this build

## Gameplay
### Two-phase labyrinth
- **SEARCH**: enter a 25×24 branching labyrinth lit only by your flashlight; find the amber prize pedestal at the heart
- **ESCAPE**: once claimed, the pedestal dims, the Minotaur speeds up, and you must retrace back to the starting cell to win

### Core mechanics
- **Ariadne's thread**: auto-unspools behind you (LineRenderer sampled every 0.12s or 0.5m traveled). Pulses amber at 2.4 Hz, lives in world space.
- **The Minotaur**: shadow pursuer with a glowing red eye-light. Spawns 8s in, walks forward along your recorded thread points at 3.0 u/s (vs your 4.5). Catches you = game over.
- **Flashlight flicker**: your spotlight dims and stutters as he closes — starts at 10m, peaks at 0m.
- **Tap R — scatter the Minotaur**: costs 1 gem, only works if he's within 10m. Teleports him to the labyrinth entrance and freezes him for the duration.
- **Hold R — teleport-retrace to last fork**: jumps you to the most recent branch cell and truncates the thread. No gem cost, but limited by how many branches you've passed.
- **Shift — sprint**: 1.7× speed, drains stamina (34/sec), regens at 42/sec, locks out if fully depleted until 25% recovered. Always-available last-resort escape.
- **Space** — jump (cosmetic).
- **Esc** — pause menu with full controls + reminders.

### Hazards and rewards
- **3 red trap plates** — stepping on one auto-rewinds 5 thread segments (an involuntary partial retrace)
- **6 cyan gems** — floating, spinning, glowing spheres in dead-end corridors; each collected adds to your scatter reserve
- **5 blue stone tablets** — inscribed with Ariadne's voice. One-line whispers fade into the status bar on proximity (`"Every gem is a fallen tribute. Spend them well."`)
- **1 amber prize pedestal** — the heart of the labyrinth; touching it flips the game into Escape phase

### HUD
- **Top-left**: phase tag (SEARCH / ESCAPE), thread length, gem count, TAP R scatter-ready hint, stamina bar
- **Top-right**: 320×320 real-time minimap (walls hidden until visited, amber for visited floor, cyan dot for player, red dot for Minotaur — retrace visibly pops both dots in real time)
- **Bottom-center**: status / whisper / tutorial text
- **Pause**: two-column overlay — CONTROLS list + REMINDERS (phase tags, tile legend, Minotaur tip), Resume + Restart buttons

## Controls
| Input | Action |
|---|---|
| WASD / Arrow keys | Walk |
| Mouse | Look around |
| Shift | Sprint (limited by stamina) |
| Space | Jump |
| **Tap R** | Scatter the Minotaur (−1 gem, within 10m) |
| **Hold R** | Teleport-retrace to last fork |
| Esc | Pause / Resume |

## How to build
### Scenes
```
Unity -batchmode -quit -projectPath <path> \
      -executeMethod Backtrack.EditorTools.SceneBuilder.BuildAll
```
Builds all six scenes (MainMenu, Level1–3, WinScreen, ThreeD) with every reference wired, and registers them in Build Settings.

### WebGL
```
Unity -batchmode -quit -projectPath <path> \
      -executeMethod Backtrack.EditorTools.WebGLBuilder.BuildWebGL
```
Output lands in `Build/WebGL/`. The included `WebGLBuilder` sets: no compression, 512 MB heap, full exception stacktraces, minimal IL2CPP stripping, and forces `Universal Render Pipeline/Lit`, `Simple Lit`, and `Unlit` into **Always Included Shaders** so URP survives the build.

## Architecture
```
Assets/Scripts/ThreeD/
  ThreeDPlayer.cs       CharacterController FPS — WASD + mouse-look, sprint + stamina
                        drain/regen + lockout, jump, head-bob (sine Y jiggle),
                        FOV kick on sprint.
  ThreeDDungeon.cs      Runtime labyrinth from ASCII layout — walls, floors, start/
                        goal, gem + trap + tablet spawners, per-tile material
                        instances for fog decay, IsReady guard wrapping a try/catch
                        around shader-find fallback chain.
  ThreeDManager.cs      Phase state machine (Search → Escape), HUD updates, R-key
                        tap/hold state machine, pause menu + Time.timeScale=0,
                        goal/start proximity win checks, trap handler, tablet
                        whispers.
  AriadneThread.cs      LineRenderer backed by a sampled position history;
                        color pulse every frame, RewindCoroutine (hold),
                        TrapRewindCoroutine (forced), TruncateTo (checkpoint retrace).
  Minotaur.cs           Walks forward through recorded thread points; eye-light
                        intensity lerps with distance to player; Scatter() resets
                        his index to 0 and teleports him to the entrance.
  MapHUD.cs             Real-time Texture2D minimap rendered from dungeon state —
                        fog of war, colored tile markers, live player + Minotaur dots.
  IntroNarration.cs     Fade-in narration overlay, skippable by any key, unlocks
                        player input when done.
  BobRotate.cs          Bob + spin animation for gems and the goal pedestal.
  EmissivePulse.cs      Sine-modulated emission color for glowing tiles.
  FlashlightFlicker.cs  Spotlight dims + flickers based on distance to Minotaur.

Assets/Editor/
  SceneBuilder.cs     Headless generator for every scene (1 main menu, 3 2D levels,
                      1 win screen, 1 3D labyrinth) — fully wires references +
                      Canvas UI + win/lose/pause panels.
  WebGLBuilder.cs     Headless WebGL build for itch.io with URP-shader inclusion
                      mutation on GraphicsSettings.
```

## Notable polish
- **Floor decay**: every visited floor tile glows bright then fades to 15% over 18 seconds. The labyrinth visibly forgets you as you move — only the thread remembers.
- **Flashlight flicker**: the spotlight wavers increasingly as the Minotaur closes, like a dying bulb at 3m. Horror telegraph without audio.
- **Per-wall color jitter** via MaterialPropertyBlock — no two walls are identical, natural stone variance without material duplication.
- **Emissive pulse** on gems and the goal pedestal with randomized phase so objects feel alive and out-of-sync.
- **Thread color pulse** at 2.4 Hz — even standing still, the silk glows brighter/dimmer. Reinforces the magical-object framing.
- **Head-bob**: sinusoidal Y camera jiggle, 8 Hz walking / 14 Hz sprinting, settles when you stop.
- **Sprint FOV punch** 75° → 88°, lerped for comfort.
- **Intro narration**: 9-second fade-in atmospheric text that doubles as tutorial, skippable with any key.
- **Five stone tablets** scattered through the labyrinth with Ariadne's voice — one-line whispers delivered contextually (`"Follow me home, Theseus. Pull the thread."`).
- **Two-phase win condition** completes the Ariadne myth mechanically: enter, claim, escape. You don't just reach the goal — you thread your way back out.
- **Defensive runtime init**: try/catch wraps dungeon build, shader-find falls through a chain of five candidates, WebGL build forces URP shaders into Always Included so the labyrinth renders even if URP shaders are shipped via atypical paths.

`github repo link` : https://github.com/satyamyadav979/ariadne