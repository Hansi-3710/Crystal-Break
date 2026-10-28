using System.Collections.Generic;
using UnityEngine;

/// Plays Resources/Audio/<name> (collect, damage, win, lose, dash, music).
/// If a file is missing, a synthesized placeholder plays so the build is never silent.
/// Drop your Week 6 .wav/.mp3 files into Assets/Resources/Audio with those exact names.
public static class AudioManager
{
    static AudioSource sfx, music;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static void Init(GameObject host)
    {
        sfx = host.AddComponent<AudioSource>();
        music = host.AddComponent<AudioSource>(); music.loop = true; music.volume = 0.35f;
    }
    public static void Play(string n, float vol = 1f) { if (sfx) sfx.PlayOneShot(Get(n), vol); }
    public static void StartMusic() { if (music) { music.clip = Get("music"); music.Play(); } }
    public static void StopMusic() { if (music) music.Stop(); }

    static AudioClip Get(string n)
    {
        AudioClip c;
        if (cache.TryGetValue(n, out c) && c != null) return c;
        c = Resources.Load<AudioClip>("Audio/" + n);
        if (c == null) c = Synth(n);
        cache[n] = c; return c;
    }

    static AudioClip Synth(string n)
    {
        switch (n)
        {
            case "collect": return Seq(n, new[] { 660f, 990f }, 0.12f, 0, 0.5f);
            case "damage": return Seq(n, new[] { 0f }, 0.3f, 2, 0.6f);
            case "dash": return Seq(n, new[] { 300f, 500f, 700f }, 0.05f, 1, 0.3f);
            case "win": return Seq(n, new[] { 523f, 659f, 784f, 1046f }, 0.2f, 0, 0.5f);
            case "lose": return Seq(n, new[] { 392f, 330f, 262f, 196f }, 0.3f, 1, 0.4f);
            default: return Seq(n, new[] { 220f, 262f, 330f, 392f, 330f, 262f, 330f, 440f, 392f, 330f, 262f, 330f, 220f, 262f, 392f, 330f }, 0.25f, 0, 0.3f);
        }
    }

    // wave: 0 sine, 1 square, 2 noise
    static AudioClip Seq(string n, float[] f, float noteDur, int wave, float vol)
    {
        int sr = 44100, per = (int)(noteDur * sr);
        var d = new float[per * f.Length];
        for (int k = 0; k < f.Length; k++)
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / sr, env = Mathf.Exp(-3f * i / per) * Mathf.Min(1f, i / 200f);
                float ph = 2f * Mathf.PI * f[k] * t;
                float s = wave == 0 ? Mathf.Sin(ph) : wave == 1 ? Mathf.Sign(Mathf.Sin(ph)) * 0.5f : Random.value * 2f - 1f;
                d[k * per + i] = s * env * vol;
            }
        var c = AudioClip.Create(n, d.Length, 1, sr, false); c.SetData(d, 0); return c;
    }
}
