using UnityEngine;

public class ParticleBackground : MonoBehaviour
{
    void Start()
    {
        CreateAmbientParticles();
    }

    void CreateAmbientParticles()
    {
        GameObject obj = new GameObject("AmbientParticles");
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(50, 10, -3);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 10f;
        main.startSpeed = 0.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 0.7f, 0.2f),
            new Color(0.8f, 0.9f, 1f, 0.15f)
        );
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(120, 15, 5);

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }
}
