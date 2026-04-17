using UnityEngine;

// Picks a maze theme based on the run seed and produces wall/floor/ceiling
// materials. No texture imports needed — purely color + smoothness driven.
public static class ThemeSystem
{
    public struct Theme
    {
        public string name;
        public Color wall;
        public Color floor;
        public Color ceiling;
        public float wallSmoothness;
        public float floorSmoothness;
        public Color exitColor;
    }

    static readonly Theme[] Themes = new[]
    {
        new Theme { name = "Concrete",
            wall = new Color(0.16f, 0.18f, 0.20f),
            floor = new Color(0.10f, 0.11f, 0.12f),
            ceiling = new Color(0.08f, 0.08f, 0.09f),
            wallSmoothness = 0.15f, floorSmoothness = 0.55f,
            exitColor = new Color(0.75f, 0.88f, 1f) },
        new Theme { name = "Rust",
            wall = new Color(0.28f, 0.16f, 0.10f),
            floor = new Color(0.20f, 0.13f, 0.08f),
            ceiling = new Color(0.10f, 0.07f, 0.05f),
            wallSmoothness = 0.30f, floorSmoothness = 0.40f,
            exitColor = new Color(1f, 0.82f, 0.55f) },
        new Theme { name = "Bone",
            wall = new Color(0.55f, 0.52f, 0.45f),
            floor = new Color(0.30f, 0.28f, 0.25f),
            ceiling = new Color(0.18f, 0.17f, 0.16f),
            wallSmoothness = 0.10f, floorSmoothness = 0.25f,
            exitColor = new Color(0.95f, 0.30f, 0.30f) },
        new Theme { name = "Stone",
            wall = new Color(0.22f, 0.24f, 0.28f),
            floor = new Color(0.12f, 0.14f, 0.16f),
            ceiling = new Color(0.06f, 0.07f, 0.08f),
            wallSmoothness = 0.20f, floorSmoothness = 0.45f,
            exitColor = new Color(0.55f, 0.95f, 0.85f) },
        new Theme { name = "Crimson",
            wall = new Color(0.20f, 0.05f, 0.07f),
            floor = new Color(0.10f, 0.03f, 0.04f),
            ceiling = new Color(0.05f, 0.01f, 0.02f),
            wallSmoothness = 0.40f, floorSmoothness = 0.55f,
            exitColor = new Color(0.30f, 0.95f, 0.95f) },
    };

    public static Theme PickFromSeed(int seed)
    {
        int idx = (Mathf.Abs(seed) % Themes.Length);
        return Themes[idx];
    }

    public static Material MakeMaterial(Color color, float smoothness, bool emissive = false)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.05f);
        if (emissive)
        {
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * 2f);
        }
        return m;
    }
}
