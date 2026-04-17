using UnityEngine;

public class DeathExplosion : MonoBehaviour
{
    public static void SpawnAt(Vector3 position)
    {
        GameObject obj = new GameObject("DeathExplosion");
        obj.transform.position = position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.2f, 0f),
            new Color(1f, 0f, 0f)
        );
        main.maxParticles = 50;
        main.loop = false;
        main.gravityModifier = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30, 50)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0f, 0f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.3f, 0f);
        light.intensity = 5f;
        light.range = 8f;

        Destroy(obj, 2f);
    }
}
