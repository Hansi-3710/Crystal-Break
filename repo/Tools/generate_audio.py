"""Synthesizes all game audio (Week 6 pipeline: code-generated via Claude). Run: python Tools/generate_audio.py"""
import numpy as np, wave, os
SR = 44100
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Audio")
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(3)

def save(name, x):
    x = x / max(1e-9, np.max(np.abs(x))) * 0.85
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())

def t(d): return np.arange(int(SR * d)) / SR
def env(n, a=0.005, decay=6):
    e = np.exp(-decay * np.linspace(0, 1, n)); k = int(a * SR); e[:k] *= np.linspace(0, 1, k); return e
def tone(f, d, wave_="sin", decay=6):
    x = t(d); ph = 2 * np.pi * f * x
    s = np.sin(ph) if wave_ == "sin" else np.sign(np.sin(ph)) * 0.5 if wave_ == "sq" else 2 * (f * x % 1) - 1
    return s * env(len(x), decay=decay)
def sweep(f0, f1, d, decay=4):
    x = t(d); f = np.linspace(f0, f1, len(x)); ph = 2 * np.pi * np.cumsum(f) / SR
    return np.sin(ph) * env(len(x), decay=decay)

# collect: bright two-note chime with octave shimmer
save("collect", np.concatenate([tone(880, .09) + .4 * tone(1760, .09), tone(1320, .25, decay=5) + .4 * tone(2640, .25, decay=5)]))
# damage: noise burst + low thump + zap
n = int(SR * .4); noise = rng.uniform(-1, 1, n) * env(n, decay=8)
save("damage", noise * .7 + sweep(160, 50, .4, 5) + .3 * sweep(1800, 200, .4, 10))
# dash: filtered noise whoosh
n = int(SR * .25); w = rng.uniform(-1, 1, n); k = np.ones(40) / 40
whoosh = np.convolve(w, k, "same") * np.sin(np.linspace(0, np.pi, n))
save("dash", whoosh * 2 + .3 * sweep(300, 900, .25, 3))
# win: major arpeggio fanfare
notes = [523.25, 659.25, 783.99, 1046.5, 783.99, 1046.5]
save("win", np.concatenate([tone(f, .18, "sin", 3) + .3 * tone(f * 2, .18, "sin", 3) for f in notes[:-1]] + [tone(1046.5, 1.0, "sin", 2.5) + .5 * tone(1318.5, 1.0, "sin", 2.5) + .5 * tone(1568, 1.0, "sin", 2.5)]))
# lose: descending minor, square
save("lose", np.concatenate([tone(f, .35, "sq", 3) * .6 for f in [392, 349.2, 311.1, 261.6]] + [tone(130.8, 1.0, "sq", 3) * .6]))
# music: 70 bpm minor arena loop, bass + arp + pad (~27s), seamless
bpm = 70; beat = 60 / bpm; bars = 8; total = bars * 4 * beat
M = np.zeros(int(SR * total) + SR)
prog = [(110, [220, 261.6, 329.6]), (87.3, [174.6, 220, 261.6]), (130.8, [261.6, 329.6, 392]), (98, [196, 246.9, 293.7])]
for bar in range(bars):
    root, chord = prog[bar % 4]; s0 = bar * 4 * beat
    def add(x, at):
        i = int(at * SR); M[i:i + len(x)] += x[:len(M) - i]
    add(tone(root, 4 * beat, "saw", 0.8) * .22, s0)                        # bass
    for c in chord: add(tone(c, 4 * beat, "sin", 0.3) * .10, s0)           # pad
    for step in range(8): add(tone(chord[step % 3] * 2, beat / 2, "sin", 7) * .18, s0 + step * beat / 2)  # arp
    for b in range(4): add(sweep(120, 45, .2, 8) * .5, s0 + b * beat)      # kick
M = M[:int(SR * total)]
save("music", M)
print("done")
