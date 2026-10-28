# Do-This-Now Checklist (mapped to the rubric)

**Fast order (≈2–3 hrs):**
1. [20 min] Unity project → copy `Assets/` → Play. Confirm the game works with primitives. *(Core Loop 40)*
2. [30 min] Tripo: generate Player, Enemy, Crystal, Pillar at tripo3d.ai (note credits before/after). Download `.glb`. Install glTFast. Drop into `Assets/Resources/Tripo/` named exactly `Player`, `Enemy`, `Crystal`, `Pillar`. Play; fix facing with the model's export orientation. *(Tripo 30)*
3. [20 min] Audio: generate 6 sounds, name per README, drop in `Resources/Audio`. *(Build 20)*
4. [20 min] Build (Windows/Mac/WebGL). **Test on a second machine**. Zip → GitHub Release. *(Build 20)*
5. [40 min] Trailer: key art → image-to-video 3 shots; OBS record gameplay; cut in CapCut; export. *(Trailer 20)*
6. [20 min] Fill every `[FILL IN]` in README / PROMPT_LOG / ATTRIBUTION / REFERENCES. Write reflection in your voice. *(Log 30 + Docs 10)*
7. [10 min] `git init; git add .; git commit; push` — verify no `Library/` `Temp/` `Logs/` committed. Submit repo URL on Canvas.
8. Bonus: Itch.io upload (WebGL zip or Windows zip + cover 630×500 + instructions) → +15. Show up and present → +15.

**Rubric self-check**
- [ ] Reward works & visible/audible  - [ ] Damage works (enemy + lava)  - [ ] Win + Loss + restart without editor
- [ ] ≥3 Tripo assets visible in build, each documented (prompt, route, format, placement)
- [ ] Trailer 30–60 s, hook, real gameplay, CTA, shot prompts logged
- [ ] Prompt log: chronological, errors, Tripo, trailer, audio, reflection 200–300 words
- [ ] `.gitignore` correct, build runs elsewhere, audio wired
- [ ] README: play instructions, build steps, references, toolchain, license sheet incl. Tripo free-plan-public note

**Git commands**
```
git init && git add . && git commit -m "PG3: Crystal Break"
git branch -M main
git remote add origin https://github.com/<you>/<repo>.git
git push -u origin main
```
Large build → GitHub Release (not committed).
