# Master AI Prompt Log — Crystal Break

> ⚠ Replace every `[FILL IN]` with what actually happened. Logs must be truthful. Entries marked ✅ are pre-written because the described step was done with Claude; edit freely.

## 0. Setup
- Engine: Unity [version] · Coding agent(s): Claude [+ Codex/Cursor] · Tripo route(s): [MCP / tripo3d.ai / generate-and-import]
- Tripo credits: before **[FILL IN]** → after **[FILL IN]**

## 1. Chronological Code Prompts
| # | Date/time | Agent | Prompt (verbatim or summarized) | Result |
|---|---|---|---|---|
| 1 ✅ | Oct 28 | Claude | "Build a complete Unity game: Reward–Damage–End triad, Tripo assets, audio, restartable, playable by a stranger. Full rubric pasted." | Scaffold: `GameBootstrap` (runtime level builder), `GameManager` (state/HUD/end screens), `PlayerController` (move + dash i-frames), `EnemyChaser`, `Hazard`, `Collectible`, `Portal`, `CameraFollow`, `AudioManager` (with synth fallbacks). |
| 2 | [time] | [agent] | [e.g. "Tune enemy speed; make lava slightly larger"] | [FILL IN] |
| 3 | | | | |

## 2. Error Recovery Log (what broke → re-prompt → fix)
| # | Symptom | Cause | Re-prompt / action | Resolution |
|---|---|---|---|---|
| 1 | Each Tripo `.glb` was ~60 MB (4 × ≈250 MB) | Models exported at ultra quality: **~1.9 M triangles** + three 4096² textures each. The arena spawns ~22 model instances (≈40 M tris) → unplayable, and too heavy for GitHub. | Uploaded the four GLBs to Claude: "make these game-ready". | Wrote `Tools/optimize_tripo_glb.py` to decimate (retopology step) → 0.4–1 MB each. |
| 2 | First decimation attempt (texture-aware quadric in MeshLab) left **cracks** along UV seams; mesh split into hundreds of loose pieces; dark lines in preview | Seam vertices were decimated independently, tearing the mesh | Re-prompted Claude with the preview showing cracks; inspected connected components (hundreds instead of 1) | v2 pipeline: weld seam vertices → bake texture to vertex colors → decimate → re-transfer colors. All 4 models now 1 watertight piece. |
| 3 | Drone model had a chunk of **rubble baked under it** (from the concept image's rocks) | Image-to-3D reconstructed the background rocks | Analysed connected components: drone body + 1 loose rubble component | Script drops the rubble component (`drop_below_y = 0.35`); drone hovers 0.6 m in game. |
| 4 | [FILL IN: any Unity-side errors, e.g. Input System exception / pink materials / glb not importing] | | | |

## 3. Tripo Generation Log (one block per asset — min. 3)

### Tripo input images (image-to-3D route)
Concept images for the four assets are in `Docs/concept_art/` and were used as Tripo **image-to-3D** inputs: `player_robot_concept.png`, `enemy_drone_concept.png`, `crystal_concept.png`, `pillar_concept.png`.
- Concept image generator: **[FILL IN: tool + exact prompt used for each image]**
- Route for all four: **tripo3d.ai image-to-3D (generate-and-import)** [edit if different]. Tripo options: [model version], texture on, PBR on, quality [standard/HD], face limit [n].
- Per-asset block below: add the Tripo task/prompt text, credits spent, and whether you re-rolled.

### Asset 1 — Player robot (`Player.glb`)
- **Source file from Tripo:** `cute_robot_3d_model.glb` (60.9 MB, 1,891,807 tris) · concept image `Docs/concept_art/player_robot_concept.png`
- **Prompt / route:** [FILL IN exact Tripo prompt + route: image-to-3D from the concept image, or text-to-3D] · credits used: [n]
- **Pipeline:** Tripo GLB → `Tools/optimize_tripo_glb.py` (weld seams, bake texture→vertex colors, quadric decimation to **40,000 tris**, matte PBR material) → 1.0 MB `Assets/Resources/Tripo/Player.glb` → Unity (glTFast) → `GameBootstrap.Visual("Player")` scales to 1.8 m
- **Placement:** the controllable character, on screen in every gameplay frame

### Asset 2 — Spiky drone (`Enemy.glb`)
- **Source file:** `spiky_flying_drone_3d_model.glb` (62.5 MB, 1,956,882 tris) · concept `enemy_drone_concept.png`
- **Prompt / route / credits:** [FILL IN]
- **Pipeline:** same script, **30,000 tris**, with rubble component removed (`drop_below_y 0.35`) → 0.8 MB `Enemy.glb`; scaled to 0.9 m tall (≈1.9 m wingspan), hovers 0.6 m
- **Placement:** 3 chasing drones, 20 damage on contact

### Asset 3 — Glowing crystal (`Crystal.glb`)
- **Source file:** `glowing_crystal_3d_model.glb` (60.5 MB, 1,856,894 tris) · concept `crystal_concept.png`
- **Prompt / route / credits:** [FILL IN]
- **Pipeline:** same script, **15,000 tris** → 0.4 MB `Crystal.glb`; scaled to 1.2 m, spins and bobs
- **Placement:** 8 collectibles (the Reward)

### Asset 4 — Runic stone pillar (`Pillar.glb`)
- **Source file:** `glowing_runic_stone_3d_model.glb` (65.2 MB, 1,964,912 tris) · concept `pillar_concept.png`
- **Prompt / route / credits:** [FILL IN]
- **Pipeline:** same script, **20,000 tris** → 0.5 MB `Pillar.glb`; scaled to 2.8 m with a 1.8 × 1.8 m solid collider
- **Placement:** 10 solid pillars (cover / obstacles)

Preview of the optimized models: `Docs/tripo_models_preview.png`. Original full-res GLBs are **not committed** (~250 MB); they remain in my Tripo account / local backup.

## 4. Trailer Shot Prompts
(see `Docs/TRAILER_PLAN.md` for the full shot list.) Log the exact prompt + tool + seed for each generated shot here: [FILL IN]

## 5. Audio Prompts
Audio was **synthesized in code** (no external audio model): Claude wrote `Tools/generate_audio.py` (numpy, sine/square/saw/noise + envelopes); running it outputs the six `.wav` files in `Assets/Resources/Audio/`.
- Prompt to Claude: "Finish the audio: make real .wav files for collect, damage, dash, win, lose, and a looping music track, wired to the Reward/Damage/End triad."
| Sound | Triad role | Design | File |
|---|---|---|---|
| collect | Reward | two-note bright chime + octave shimmer | `collect.wav` |
| damage | Damage | noise burst + low thump + zap sweep | `damage.wav` |
| dash | Control | filtered-noise whoosh | `dash.wav` |
| win | End (win) | major arpeggio fanfare | `win.wav` |
| lose | End (loss) | descending minor square tones | `lose.wav` |
| music | Loop | 70 bpm Am-F-C-G, bass+pad+arp+kick, ~27 s seamless | `music.wav` |
[If you replace any with Week 6 AI audio, log tool + prompt here.]

## 6. Analytical Reflection (200–300 words) — DRAFT, REWRITE IN YOUR OWN VOICE
In this project the AI wrote the structural code: a runtime level builder, the game-state manager and the enemy and player behaviours. It also generated my 3D assets through Tripo and some of the audio. What I did was decide the design — the Reward, Damage and End rules, the numbers (8 crystals, 20 damage drones, i-frames) — and direct the work through prompts. I also did everything that could not be delegated: installing and configuring Unity, importing the Tripo files, fixing errors that only appear in the editor, play-testing for feel, and packaging the build.

Compared with PG2, the engine changed the division of labor a lot. In the browser, the AI could hand me one file and I could see the result instantly. In Unity, much of the work is in the editor — scene setup, import settings, Project Settings, package installs — and the agent cannot see or click it. That pushed me to ask the AI for code that sets up its own scene at runtime, so there was less manual wiring to get wrong. Errors also changed: instead of JavaScript console messages, I hit things like Input System settings, model scale and axis, and shader differences, which the AI could only diagnose if I pasted the exact messages. The 3D pipeline added a new human role: art director and quality checker, rejecting Tripo outputs that did not match the style.

[ADD: one specific moment where you overruled the AI, one thing it got wrong, and one thing you'd do differently.]
