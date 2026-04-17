using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetIntegrator : EditorWindow
{
    [MenuItem("Tools/Integrate Downloaded Assets")]
    public static void IntegrateAssets()
    {
        ReplacePlayerModel();
        ReplaceEnemyModels();
        ReplacePlatformModels();
        AddEnvironmentDecorations();
        AddCollectibles();

        if (!EditorApplication.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("[AssetIntegrator] Assets integrated! Player, enemies, platforms, and decorations replaced with downloaded 3D models.");
    }

    static void ReplacePlayerModel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        string characterPath = "Assets/Downloaded_Assets/Quaternius_Platformer/Character/FBX/Character.fbx";
        GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);

        if (characterPrefab != null)
        {
            Renderer oldRenderer = player.GetComponent<Renderer>();
            if (oldRenderer != null)
                oldRenderer.enabled = false;

            Transform existingModel = player.transform.Find("CharacterModel");
            if (existingModel != null)
                Object.DestroyImmediate(existingModel.gameObject);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
            model.name = "CharacterModel";
            model.transform.SetParent(player.transform, false);
            model.transform.localPosition = new Vector3(0, -0.5f, 0);
            model.transform.localScale = Vector3.one * 0.5f;
            model.transform.localRotation = Quaternion.Euler(0, 90, 0);

            Collider[] modelColliders = model.GetComponentsInChildren<Collider>();
            foreach (Collider c in modelColliders)
                Object.DestroyImmediate(c);

            EditorUtility.SetDirty(player);
            Debug.Log("[AssetIntegrator] Player model replaced with Quaternius Character.");
        }
        else
        {
            Debug.LogWarning("[AssetIntegrator] Character.fbx not found at: " + characterPath);
        }
    }

    static void ReplaceEnemyModels()
    {
        string[] enemyFBXPaths = {
            "Assets/Downloaded_Assets/Quaternius_Platformer/Enemies/FBX/Enemy.fbx",
            "Assets/Downloaded_Assets/Quaternius_Platformer/Enemies/FBX/Skull.fbx",
            "Assets/Downloaded_Assets/Quaternius_Platformer/Enemies/FBX/Bee.fbx",
            "Assets/Downloaded_Assets/Quaternius_Platformer/Enemies/FBX/Crab.fbx"
        };

        Hazard[] hazards = Object.FindObjectsOfType<Hazard>();
        int enemyIdx = 0;

        foreach (Hazard h in hazards)
        {
            if (h.hazardType != HazardType.Enemy) continue;

            string fbxPath = enemyFBXPaths[enemyIdx % enemyFBXPaths.Length];
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

            if (enemyPrefab != null)
            {
                Renderer oldRenderer = h.GetComponent<Renderer>();
                if (oldRenderer != null)
                    oldRenderer.enabled = false;

                Transform existingModel = h.transform.Find("EnemyModel");
                if (existingModel != null)
                    Object.DestroyImmediate(existingModel.gameObject);

                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                model.name = "EnemyModel";
                model.transform.SetParent(h.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localScale = Vector3.one * 0.8f;

                Collider[] modelColliders = model.GetComponentsInChildren<Collider>();
                foreach (Collider c in modelColliders)
                    Object.DestroyImmediate(c);

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    foreach (Material m in r.materials)
                    {
                        m.EnableKeyword("_EMISSION");
                        m.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.1f) * 1.5f);
                    }
                }

                EditorUtility.SetDirty(h.gameObject);
                enemyIdx++;
            }
        }
        Debug.Log($"[AssetIntegrator] Replaced {enemyIdx} enemy models.");
    }

    static void ReplacePlatformModels()
    {
        string[] platformFBXNames = {
            "block-grass-large.fbx",
            "block-grass-long.fbx",
            "block-grass-low-large.fbx"
        };

        string[] spikeNames = {
            "spike-block.fbx",
            "spike-block-wide.fbx",
            "trap-spikes.fbx"
        };

        string basePath = "Assets/Downloaded_Assets/Kenney_Platformer_Kit/Models/FBX format/";

        Hazard[] hazards = Object.FindObjectsOfType<Hazard>();
        int spikeIdx = 0;
        foreach (Hazard h in hazards)
        {
            if (h.hazardType != HazardType.Spike) continue;

            string spikePath = basePath + spikeNames[spikeIdx % spikeNames.Length];
            GameObject spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spikePath);

            if (spikePrefab != null)
            {
                Renderer oldRenderer = h.GetComponent<Renderer>();
                if (oldRenderer != null)
                    oldRenderer.enabled = false;

                Transform existingModel = h.transform.Find("SpikeModel");
                if (existingModel != null)
                    Object.DestroyImmediate(existingModel.gameObject);

                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(spikePrefab);
                model.name = "SpikeModel";
                model.transform.SetParent(h.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localScale = Vector3.one * 1.5f;

                Collider[] modelColliders = model.GetComponentsInChildren<Collider>();
                foreach (Collider c in modelColliders)
                    Object.DestroyImmediate(c);

                EditorUtility.SetDirty(h.gameObject);
                spikeIdx++;
            }
        }
        Debug.Log($"[AssetIntegrator] Replaced {spikeIdx} spike models with Kenney assets.");
    }

    static void AddEnvironmentDecorations()
    {
        string basePath = "Assets/Downloaded_Assets/Kenney_Platformer_Kit/Models/FBX format/";

        string[] decoModels = {
            "tree-pine.fbx", "tree-pine-small.fbx", "tree.fbx",
            "mushrooms.fbx", "barrel.fbx", "crate.fbx",
            "fence-low-straight.fbx"
        };

        int decoCount = 0;
        for (int i = 0; i < 20; i++)
        {
            string name = "EnvDeco_" + i;
            if (GameObject.Find(name) != null) continue;

            string modelFile = decoModels[i % decoModels.Length];
            string modelPath = basePath + modelFile;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (prefab == null) continue;

            float x = i * 9f + Random.Range(-2f, 2f);
            float y = GetGroundLevel(x);

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.name = name;
            obj.transform.position = new Vector3(x, y, Random.Range(2f, 5f));
            obj.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            Collider[] cols = obj.GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
                Object.DestroyImmediate(c);

            EditorUtility.SetDirty(obj);
            decoCount++;
        }
        Debug.Log($"[AssetIntegrator] Added {decoCount} environment decorations.");
    }

    static void AddCollectibles()
    {
        string basePath = "Assets/Downloaded_Assets/Kenney_Platformer_Kit/Models/FBX format/";

        string[] coinModels = { "coin-gold.fbx", "coin-silver.fbx", "coin-bronze.fbx" };

        int coinCount = 0;
        for (int i = 0; i < 25; i++)
        {
            string name = "Collectible_" + i;
            if (GameObject.Find(name) != null) continue;

            string modelFile = coinModels[i % coinModels.Length];
            string modelPath = basePath + modelFile;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (prefab == null) continue;

            float x = i * 7f + 5f;
            float y = GetGroundLevel(x) + Random.Range(2f, 4f);

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.name = name;
            obj.transform.position = new Vector3(x, y, 0);
            obj.transform.localScale = Vector3.one * 0.5f;

            Collider[] cols = obj.GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
                Object.DestroyImmediate(c);

            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1f;

            obj.AddComponent<Collectible>();

            HazardPulse pulse = obj.AddComponent<HazardPulse>();
            pulse.pulseSpeed = 3f;
            pulse.floatSpeed = 2f;
            pulse.floatAmount = 0.3f;
            pulse.rotateSpeed = 90f;

            Light glow = obj.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 3f;
            glow.intensity = 1.5f;
            glow.color = i % 3 == 0 ? new Color(1f, 0.84f, 0f) :
                         i % 3 == 1 ? new Color(0.8f, 0.8f, 0.9f) :
                                      new Color(0.8f, 0.5f, 0.2f);

            EditorUtility.SetDirty(obj);
            coinCount++;
        }
        Debug.Log($"[AssetIntegrator] Added {coinCount} collectible coins.");
    }

    static float GetGroundLevel(float x)
    {
        if (x < 60) return 0f;
        if (x < 90) return 3f + (x - 60) * 0.1f;
        if (x < 120) return 6f + (x - 90) * 0.1f;
        if (x < 155) return 8f + (x - 120) * 0.1f;
        return 11f + (x - 155) * 0.1f;
    }
}
