using UnityEngine;

// Synthesises horror audio so the game has sound without any imported assets.
// Footsteps, ambient drone, scares, heartbeat, breathing, whispers.
public static class ProceduralAudio
{
    const int SampleRate = 44100;

    // ─── FOOTSTEPS ────────────────────────────────────────────────────────────
    // Concrete-on-concrete: low impact thud + high-frequency scuff transient.

    public static AudioClip[] MakeFootsteps(int count)
    {
        var clips = new AudioClip[count];
        for (int i = 0; i < count; i++) clips[i] = MakeFootstep(i);
        return clips;
    }

    public static AudioClip MakeFootstep(int seed)
    {
        var rng = new System.Random(seed * 31 + 7);
        float duration = 0.18f + (float)rng.NextDouble() * 0.06f;
        int samples = Mathf.RoundToInt(duration * SampleRate);
        float[] data = new float[samples];

        // Layer 1: low impact thud, pitch sweeps down, fast decay.
        float thudStart = 110f + (float)rng.NextDouble() * 30f;
        float thudEnd = thudStart * 0.55f;

        // Layer 2: high-frequency scuff (filtered white noise burst).
        float scuffLp = 0.35f;
        float scuffPrev = 0f;
        float scuffHpPrev = 0f;
        float scuffHpAlpha = 0.92f;

        // Layer 3: very low body resonance.
        float bodyFreq = 55f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float p = t / duration;

            // Thud: pitch envelope + amplitude envelope
            float thudFreq = Mathf.Lerp(thudStart, thudEnd, p);
            float thudEnv = Mathf.Exp(-t * 18f);
            float thud = Mathf.Sin(2f * Mathf.PI * thudFreq * t) * thudEnv;

            // Scuff: bright impact, ~25ms decay
            float scuffEnv = Mathf.Exp(-t * 70f);
            float n = ((float)rng.NextDouble() * 2f - 1f);
            scuffPrev = scuffPrev + scuffLp * (n - scuffPrev);
            float scuffHp = scuffPrev - scuffHpPrev * scuffHpAlpha;
            scuffHpPrev = scuffPrev;
            float scuff = scuffHp * scuffEnv * 0.55f;

            // Body: deep boom, slower decay
            float bodyEnv = Mathf.Exp(-t * 9f);
            float body = Mathf.Sin(2f * Mathf.PI * bodyFreq * t) * bodyEnv * 0.35f;

            float v = thud * 0.55f + scuff + body;
            data[i] = Mathf.Clamp(v, -1f, 1f) * 0.85f;
        }

        var clip = AudioClip.Create("procFootstep_" + seed, samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── AMBIENT DRONE ────────────────────────────────────────────────────────

    public static AudioClip MakeDrone(float seconds)
    {
        int samples = Mathf.RoundToInt(seconds * SampleRate);
        float[] data = new float[samples];
        float baseFreq = 48f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float lfo = Mathf.Sin(2f * Mathf.PI * 0.07f * t);
            float lfo2 = Mathf.Sin(2f * Mathf.PI * 0.13f * t + 1.7f);

            float v = 0f;
            v += Mathf.Sin(2f * Mathf.PI * baseFreq * t) * (0.30f + 0.05f * lfo);
            v += Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.5f + lfo * 0.6f) * t) * 0.18f;
            v += Mathf.Sin(2f * Mathf.PI * (baseFreq * 2.02f) * t) * 0.10f;
            v += Mathf.Sin(2f * Mathf.PI * (baseFreq * 4.01f + lfo2 * 0.4f) * t) * 0.04f;
            v += (Random.value * 2f - 1f) * 0.025f;

            data[i] = Mathf.Clamp(v, -1f, 1f) * 0.45f;
        }

        var clip = AudioClip.Create("procDrone", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── SCARES (door slam, distant scrape, etc.) ─────────────────────────────

    public static AudioClip MakeScare(float seconds, float startFreq)
    {
        int samples = Mathf.RoundToInt(seconds * SampleRate);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float p = t / seconds;
            float env = Mathf.Sin(p * Mathf.PI);
            float freq = Mathf.Lerp(startFreq, startFreq * 0.5f, p);

            float v = 0f;
            v += Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f;
            v += Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.2f;
            v += Mathf.Sin(2f * Mathf.PI * freq * 2.07f * t) * 0.15f;
            v += (Random.value * 2f - 1f) * 0.08f;

            data[i] = Mathf.Clamp(v * env, -1f, 1f) * 0.6f;
        }

        var clip = AudioClip.Create("procScare_" + startFreq, samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── HEARTBEAT (lub-dub, looped) ──────────────────────────────────────────

    public static AudioClip MakeHeartbeat(float bpm)
    {
        float beatPeriod = 60f / bpm;
        int samples = Mathf.RoundToInt(beatPeriod * SampleRate);
        float[] data = new float[samples];

        // Lub at t=0, dub ~0.18s after lub.
        float dubOffset = Mathf.Min(0.18f, beatPeriod * 0.35f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float v = 0f;

            // Lub (stronger, lower)
            float tl = t;
            if (tl < 0.12f)
            {
                float env = Mathf.Exp(-tl * 28f);
                v += Mathf.Sin(2f * Mathf.PI * 70f * tl) * env * 0.95f;
                v += Mathf.Sin(2f * Mathf.PI * 35f * tl) * env * 0.5f;
            }

            // Dub (slightly higher, weaker)
            float td = t - dubOffset;
            if (td > 0f && td < 0.10f)
            {
                float env = Mathf.Exp(-td * 32f);
                v += Mathf.Sin(2f * Mathf.PI * 90f * td) * env * 0.7f;
                v += Mathf.Sin(2f * Mathf.PI * 45f * td) * env * 0.35f;
            }

            data[i] = Mathf.Clamp(v, -1f, 1f) * 0.7f;
        }

        var clip = AudioClip.Create("procHeartbeat_" + Mathf.RoundToInt(bpm), samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── MONSTER BREATHING (loopable) ─────────────────────────────────────────

    public static AudioClip MakeBreathing(float seconds)
    {
        int samples = Mathf.RoundToInt(seconds * SampleRate);
        float[] data = new float[samples];
        float cycle = 3.5f; // one inhale+exhale every 3.5s
        float lpAlpha = 0.08f;
        float prev = 0f;
        float prev2 = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float phase = (t % cycle) / cycle;

            // Inhale (0..0.5) and exhale (0.5..1) envelopes — sharper, raspier.
            float env;
            if (phase < 0.5f) env = Mathf.Sin(phase * 2f * Mathf.PI) * 0.7f;
            else env = Mathf.Sin((phase - 0.5f) * 2f * Mathf.PI) * 1.0f;
            env = Mathf.Max(0f, env);

            float n = (Random.value * 2f - 1f);
            prev = prev + lpAlpha * (n - prev);
            prev2 = prev2 + lpAlpha * 0.6f * (prev - prev2);

            // Add a low growl element.
            float growl = Mathf.Sin(2f * Mathf.PI * (35f + 3f * Mathf.Sin(t * 1.3f)) * t) * 0.15f;

            data[i] = Mathf.Clamp(prev2 * env * 0.9f + growl * env, -1f, 1f) * 0.55f;
        }

        var clip = AudioClip.Create("procBreathing", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── CHASE STINGER (intense pursuit drone) ────────────────────────────────

    public static AudioClip MakeChase(float seconds)
    {
        int samples = Mathf.RoundToInt(seconds * SampleRate);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float dissonance = Mathf.Sin(2f * Mathf.PI * 0.4f * t);

            float v = 0f;
            v += Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.30f;
            v += Mathf.Sin(2f * Mathf.PI * (113f + 4f * dissonance) * t) * 0.25f; // slightly detuned
            v += Mathf.Sin(2f * Mathf.PI * 170f * t) * 0.15f;
            v += (Random.value * 2f - 1f) * 0.08f;

            // Periodic hits
            float hitPhase = (t * 3f) % 1f;
            float hitEnv = Mathf.Exp(-hitPhase * 10f);
            v += Mathf.Sin(2f * Mathf.PI * 55f * t) * hitEnv * 0.45f;

            data[i] = Mathf.Clamp(v, -1f, 1f) * 0.7f;
        }

        var clip = AudioClip.Create("procChase", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── DRIP (short water plink) ─────────────────────────────────────────────

    public static AudioClip MakeDrip()
    {
        float duration = 0.45f;
        int samples = Mathf.RoundToInt(duration * SampleRate);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            // Fast high-frequency plink
            float impactEnv = Mathf.Exp(-t * 55f);
            float impact = Mathf.Sin(2f * Mathf.PI * 1700f * t) * impactEnv * 0.35f;
            // Lower resonance
            float resEnv = Mathf.Exp(-t * 9f);
            float res = Mathf.Sin(2f * Mathf.PI * 620f * t) * resEnv * 0.40f;
            // Pitch-swept chirp
            float freq = Mathf.Lerp(1200f, 800f, Mathf.Clamp01(t / 0.1f));
            float chirpEnv = Mathf.Exp(-t * 38f);
            float chirp = Mathf.Sin(2f * Mathf.PI * freq * t) * chirpEnv * 0.25f;

            data[i] = Mathf.Clamp(impact + res + chirp, -1f, 1f) * 0.7f;
        }

        var clip = AudioClip.Create("procDrip", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─── WHISPERS (creepy ambience) ───────────────────────────────────────────

    public static AudioClip MakeWhisper(float seconds, int seed)
    {
        var rng = new System.Random(seed);
        int samples = Mathf.RoundToInt(seconds * SampleRate);
        float[] data = new float[samples];
        float bpAlpha = 0.4f;
        float prev = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;

            // Speech-rate AM envelope.
            float syllable = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * (4f + (float)rng.NextDouble()) * t)), 3f);
            float overall = Mathf.Sin(t * Mathf.PI / seconds);

            float n = ((float)rng.NextDouble() * 2f - 1f);
            // Band-pass-ish: subtract running average of running average
            prev = prev + bpAlpha * (n - prev);
            float bp = n - prev;

            data[i] = Mathf.Clamp(bp * syllable * overall * 0.9f, -1f, 1f) * 0.5f;
        }

        var clip = AudioClip.Create("procWhisper_" + seed, samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
