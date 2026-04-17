using UnityEngine;
using UnityEditor;

public class RageBaitLevelBuilder : EditorWindow
{
    [MenuItem("Tools/Build Rage Bait Level")]
    public static void BuildLevel()
    {
        CreateRageManagers();
        AddMovingEnemies();
        CreateFakePlatforms();
        CreateGravityZones();
        BuildSection2();
        BuildSection3();
        BuildSection4();
        BuildSection5();
        CreateInvisibleWalls();
        CreateSpeedTraps();
        CreateFakeCheckpoints();
        CreateJumpScareZones();
        CreateTeleportTraps();
        CreateFloatingCodeText();
        CreateDecorations();

        GameObject reverserObj = GameObject.Find("ControlReverser");
        if (reverserObj == null)
        {
            reverserObj = new GameObject("ControlReverser");
            reverserObj.AddComponent<ControlReverser>();
            EditorUtility.SetDirty(reverserObj);
        }

        GameObject timerObj = GameObject.Find("TrollingTimer");
        if (timerObj == null)
        {
            timerObj = new GameObject("TrollingTimer");
            timerObj.AddComponent<TrollingTimer>();
            EditorUtility.SetDirty(timerObj);
        }

        GameObject rageUIObj = GameObject.Find("RageUI");
        if (rageUIObj == null)
        {
            rageUIObj = new GameObject("RageUI");
            rageUIObj.AddComponent<RageUI>();
            EditorUtility.SetDirty(rageUIObj);
        }

        GameObject neonObj = GameObject.Find("NeonEnvironment");
        if (neonObj == null)
        {
            neonObj = new GameObject("NeonEnvironment");
            neonObj.AddComponent<NeonEnvironment>();
            neonObj.AddComponent<ParticleBackground>();
            EditorUtility.SetDirty(neonObj);
        }

        GameObject diffObj = GameObject.Find("DifficultyEscalator");
        if (diffObj == null)
        {
            diffObj = new GameObject("DifficultyEscalator");
            diffObj.AddComponent<DifficultyEscalator>();
            EditorUtility.SetDirty(diffObj);
        }

        GameObject startSetup = GameObject.Find("StartScreenSetup");
        if (startSetup == null)
        {
            startSetup = new GameObject("StartScreenSetup");
            startSetup.AddComponent<StartScreenSetup>();
            EditorUtility.SetDirty(startSetup);
        }

        GameObject winEnhancer = GameObject.Find("WinScreenEnhancer");
        if (winEnhancer == null)
        {
            winEnhancer = new GameObject("WinScreenEnhancer");
            winEnhancer.AddComponent<WinScreenEnhancer>();
            EditorUtility.SetDirty(winEnhancer);
        }

        CreateSawBlades();
        ExtendKillZone();

        GameObject endZone = GameObject.Find("EndZone");
        if (endZone != null)
        {
            endZone.transform.position = new Vector3(180, 14, 0);
            endZone.transform.localScale = new Vector3(5, 5, 5);
            EditorUtility.SetDirty(endZone);
        }

        GameSetupFixer.FixEverything();
        AssetIntegrator.IntegrateAssets();

        Debug.Log("[RageBait] FULL RAGE LEVEL BUILT! 5 sections, downloaded 3D assets integrated, fake platforms, invisible walls, speed traps, fake checkpoints, teleporters, jump scares, trolling timer, rage UI. MAXIMUM RAGE.");
    }

    static void CreateRageManagers()
    {
        if (Object.FindObjectOfType<RageBaitMessages>() == null)
        {
            GameObject obj = new GameObject("RageBaitMessages");
            obj.AddComponent<RageBaitMessages>();
            EditorUtility.SetDirty(obj);
        }
        if (Object.FindObjectOfType<DeathEffects>() == null)
        {
            GameObject obj = new GameObject("DeathEffects");
            obj.AddComponent<DeathEffects>();
            EditorUtility.SetDirty(obj);
        }
    }

    static void AddMovingEnemies()
    {
        string[] enemies = { "Hazard_Enemy_1", "Hazard_Enemy_2" };
        foreach (string name in enemies)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) continue;
            MovingEnemy me = go.GetComponent<MovingEnemy>();
            if (me == null)
            {
                me = go.AddComponent<MovingEnemy>();
                me.moveSpeed = 2.5f;
                me.moveRange = 2.5f;
            }
            EditorUtility.SetDirty(go);
        }
    }

    static void CreateFakePlatforms()
    {
        CreateFakePlatformAt("FakePlatform_1", new Vector3(15, 2.5f, 0), new Vector3(3, 0.5f, 3));
        CreateFakePlatformAt("FakePlatform_2", new Vector3(33, 2, 0), new Vector3(3, 0.5f, 3));
        CreateFakePlatformAt("FakePlatform_3", new Vector3(45, 4.5f, 0), new Vector3(4, 0.5f, 3));
    }

    static void CreateFakePlatformAt(string name, Vector3 pos, Vector3 scale)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<BoxCollider>().isTrigger = false;
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MAT_Platform.mat");
        if (mat != null) go.GetComponent<Renderer>().material = mat;
        go.AddComponent<FakePlatform>();
        EditorUtility.SetDirty(go);
    }

    static void CreateGravityZones()
    {
        CreateGravityZoneAt("GravityZone_1", new Vector3(38, 4, 0), new Vector3(2, 4, 3));
        CreateGravityZoneAt("GravityZone_2", new Vector3(85, 7, 0), new Vector3(3, 5, 3));
        CreateGravityZoneAt("GravityZone_3", new Vector3(140, 10, 0), new Vector3(2, 4, 3));
    }

    static void CreateGravityZoneAt(string name, Vector3 pos, Vector3 size)
    {
        if (GameObject.Find(name) != null) return;
        GameObject gz = new GameObject(name);
        gz.transform.position = pos;
        BoxCollider bc = gz.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = size;
        gz.AddComponent<GravityZone>();
        EditorUtility.SetDirty(gz);
    }

    static void CreateInvisibleWalls()
    {
        CreateInvisibleWallAt("InvisWall_1", new Vector3(28, 3, 0), new Vector3(0.3f, 4, 3));
        CreateInvisibleWallAt("InvisWall_2", new Vector3(72, 6, 0), new Vector3(0.3f, 5, 3));
        CreateInvisibleWallAt("InvisWall_3", new Vector3(120, 9, 0), new Vector3(0.3f, 6, 3));
        CreateInvisibleWallAt("InvisWall_4", new Vector3(155, 12, 0), new Vector3(0.3f, 5, 3));
    }

    static void CreateInvisibleWallAt(string name, Vector3 pos, Vector3 scale)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<BoxCollider>().isTrigger = false;

        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(0.5f, 0f, 1f, 0f);
        go.GetComponent<Renderer>().material = mat;

        go.AddComponent<InvisibleWall>();
        EditorUtility.SetDirty(go);
    }

    static void CreateSpeedTraps()
    {
        CreateSpeedTrapAt("SpeedTrap_Fast_1", new Vector3(50, 3, 0), new Vector3(3, 2, 3), false);
        CreateSpeedTrapAt("SpeedTrap_Slow_1", new Vector3(95, 7, 0), new Vector3(3, 2, 3), true);
        CreateSpeedTrapAt("SpeedTrap_Fast_2", new Vector3(130, 10, 0), new Vector3(3, 2, 3), false);
        CreateSpeedTrapAt("SpeedTrap_Slow_2", new Vector3(160, 13, 0), new Vector3(3, 2, 3), true);
    }

    static void CreateSpeedTrapAt(string name, Vector3 pos, Vector3 size, bool isSlow)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = size;
        SpeedTrap st = go.AddComponent<SpeedTrap>();
        st.isSlowTrap = isSlow;
        EditorUtility.SetDirty(go);
    }

    static void CreateFakeCheckpoints()
    {
        CreateFakeCheckpointAt("FakeCheckpoint_1", new Vector3(55, 4.5f, 0), true);
        CreateFakeCheckpointAt("FakeCheckpoint_2", new Vector3(100, 8.5f, 0), true);
        CreateFakeCheckpointAt("RealCheckpoint_1", new Vector3(75, 6, 0), false);
        CreateFakeCheckpointAt("FakeCheckpoint_3", new Vector3(145, 12, 0), true);
    }

    static void CreateFakeCheckpointAt(string name, Vector3 pos, bool isFake)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1, 0.3f, 1);
        go.GetComponent<CapsuleCollider>().isTrigger = true;
        FakeCheckpoint fc = go.AddComponent<FakeCheckpoint>();
        fc.isReallyAKillZone = isFake;

        Material mat = new Material(Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        mat.color = isFake ? new Color(0f, 1f, 0.5f) : new Color(0f, 0.8f, 1f);
        mat.SetColor("_EmissionColor", mat.color * 1.5f);
        go.GetComponent<Renderer>().material = mat;

        EditorUtility.SetDirty(go);
    }

    static void CreateJumpScareZones()
    {
        CreateJumpScareAt("JumpScare_1", new Vector3(42, 3, 0), new Vector3(2, 3, 3));
        CreateJumpScareAt("JumpScare_2", new Vector3(88, 7, 0), new Vector3(2, 3, 3));
        CreateJumpScareAt("JumpScare_3", new Vector3(135, 11, 0), new Vector3(2, 3, 3));
    }

    static void CreateJumpScareAt(string name, Vector3 pos, Vector3 size)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = size;
        go.AddComponent<JumpScareZone>();
        EditorUtility.SetDirty(go);
    }

    static void CreateTeleportTraps()
    {
        CreateTeleportAt("Teleport_Back_1", new Vector3(65, 5.5f, 0), true);
        CreateTeleportAt("Teleport_Back_2", new Vector3(115, 9, 0), false);
        CreateTeleportAt("Teleport_Start_1", new Vector3(165, 14, 0), true);
    }

    static void CreateTeleportAt(string name, Vector3 pos, bool toStart)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        go.GetComponent<SphereCollider>().isTrigger = true;

        TeleportTrap tt = go.AddComponent<TeleportTrap>();
        tt.teleportToStart = toStart;
        if (!toStart) tt.teleportOffset = new Vector3(-25f, -3f, 0f);

        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(0.6f, 0f, 1f, 0.3f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.6f, 0f, 1f) * 2f);
        go.GetComponent<Renderer>().material = mat;

        EditorUtility.SetDirty(go);
    }

    static void BuildSection2()
    {
        float baseX = 60f;
        CreatePlatform("S2_Platform_1", new Vector3(baseX, 3, 0), new Vector3(5, 1, 3), false);
        CreatePlatform("S2_Platform_2", new Vector3(baseX + 8, 5, 0), new Vector3(3, 1, 3), true);
        CreatePlatform("S2_Platform_3", new Vector3(baseX + 14, 4, 0), new Vector3(4, 1, 3), false);
        CreatePlatform("S2_Platform_4", new Vector3(baseX + 20, 6, 0), new Vector3(5, 1, 3), true);
        CreatePlatform("S2_Platform_5", new Vector3(baseX + 27, 5, 0), new Vector3(3, 1, 3), false);

        CreateHazard("S2_Fire_1", HazardType.Fire, new Vector3(baseX + 3, 4.5f, 0), PrimitiveType.Cube, 1.2f);
        CreateHazard("S2_Spike_1", HazardType.Spike, new Vector3(baseX + 12, 5.5f, 0), PrimitiveType.Cube, 1f);
        CreateHazard("S2_Enemy_1", HazardType.Enemy, new Vector3(baseX + 18, 5.5f, 0), PrimitiveType.Sphere, 1f, true);
        CreateHazard("S2_Fire_2", HazardType.Fire, new Vector3(baseX + 24, 7.5f, 0), PrimitiveType.Cube, 1.2f);

        CreateFakePlatformAt("S2_FakePlatform", new Vector3(baseX + 11, 5.5f, 0), new Vector3(3, 0.5f, 3));

        CreateShrinkPlatform("S2_Shrink_1", new Vector3(baseX + 5, 3, 0), new Vector3(4, 1, 3));
    }

    static void BuildSection3()
    {
        float baseX = 90f;
        CreatePlatform("S3_Platform_1", new Vector3(baseX, 6, 0), new Vector3(4, 1, 3), true);
        CreatePlatform("S3_Platform_2", new Vector3(baseX + 6, 8, 0), new Vector3(3, 1, 3), false);
        CreatePlatform("S3_Platform_3", new Vector3(baseX + 12, 7, 0), new Vector3(5, 1, 3), true);
        CreatePlatform("S3_Platform_4", new Vector3(baseX + 18, 10, 0), new Vector3(4, 1, 3), false);

        CreateHazard("S3_Enemy_1", HazardType.Enemy, new Vector3(baseX + 3, 7.5f, 0), PrimitiveType.Sphere, 1f, true);
        CreateHazard("S3_Spike_1", HazardType.Spike, new Vector3(baseX + 9, 9f, 0), PrimitiveType.Cube, 1f);
        CreateHazard("S3_Fire_1", HazardType.Fire, new Vector3(baseX + 15, 8.5f, 0), PrimitiveType.Cube, 1.3f);
        CreateHazard("S3_Enemy_2", HazardType.Enemy, new Vector3(baseX + 17, 11.5f, 0), PrimitiveType.Sphere, 1.2f, true);

        CreateFakePlatformAt("S3_FakePlatform_Final", new Vector3(baseX + 16, 10.5f, 0), new Vector3(3, 0.5f, 3));

        CreateShrinkPlatform("S3_Shrink_1", new Vector3(baseX + 8, 8, 0), new Vector3(3, 1, 3));
    }

    static void BuildSection4()
    {
        float baseX = 120f;

        CreatePlatform("S4_Platform_1", new Vector3(baseX, 8, 0), new Vector3(5, 1, 3), false);
        CreatePlatform("S4_Platform_2", new Vector3(baseX + 7, 10, 0), new Vector3(3, 1, 3), true);
        CreatePlatform("S4_Platform_3", new Vector3(baseX + 13, 9, 0), new Vector3(4, 1, 3), false);
        CreatePlatform("S4_Platform_4", new Vector3(baseX + 19, 11, 0), new Vector3(3, 1, 3), true);
        CreatePlatform("S4_Platform_5", new Vector3(baseX + 25, 10, 0), new Vector3(5, 1, 3), false);
        CreatePlatform("S4_Platform_6", new Vector3(baseX + 30, 12, 0), new Vector3(4, 1, 3), true);

        CreateHazard("S4_Enemy_1", HazardType.Enemy, new Vector3(baseX + 4, 9.5f, 0), PrimitiveType.Sphere, 1.3f, true);
        CreateHazard("S4_Fire_1", HazardType.Fire, new Vector3(baseX + 10, 11f, 0), PrimitiveType.Cube, 1.5f);
        CreateHazard("S4_Spike_1", HazardType.Spike, new Vector3(baseX + 16, 10.5f, 0), PrimitiveType.Cube, 1.2f);
        CreateHazard("S4_Enemy_2", HazardType.Enemy, new Vector3(baseX + 22, 12.5f, 0), PrimitiveType.Sphere, 1f, true);
        CreateHazard("S4_Fire_2", HazardType.Fire, new Vector3(baseX + 28, 11.5f, 0), PrimitiveType.Cube, 1.4f);

        CreateFakePlatformAt("S4_FakePlatform_1", new Vector3(baseX + 10, 10.5f, 0), new Vector3(3, 0.5f, 3));
        CreateFakePlatformAt("S4_FakePlatform_2", new Vector3(baseX + 22, 11.5f, 0), new Vector3(4, 0.5f, 3));

        CreateShrinkPlatform("S4_Shrink_1", new Vector3(baseX + 15, 9, 0), new Vector3(4, 1, 3));
        CreateShrinkPlatform("S4_Shrink_2", new Vector3(baseX + 27, 10, 0), new Vector3(3, 1, 3));
    }

    static void BuildSection5()
    {
        float baseX = 155f;

        CreatePlatform("S5_Platform_1", new Vector3(baseX, 11, 0), new Vector3(4, 1, 3), true);
        CreatePlatform("S5_Platform_2", new Vector3(baseX + 6, 13, 0), new Vector3(3, 1, 3), false);
        CreatePlatform("S5_Platform_3", new Vector3(baseX + 11, 12, 0), new Vector3(4, 1, 3), true);
        CreatePlatform("S5_Platform_4", new Vector3(baseX + 16, 14, 0), new Vector3(3, 1, 3), false);
        CreatePlatform("S5_Platform_5", new Vector3(baseX + 22, 13, 0), new Vector3(5, 1, 3), true);

        CreateHazard("S5_Enemy_1", HazardType.Enemy, new Vector3(baseX + 3, 12.5f, 0), PrimitiveType.Sphere, 1.5f, true);
        CreateHazard("S5_Spike_1", HazardType.Spike, new Vector3(baseX + 8, 14f, 0), PrimitiveType.Cube, 1.3f);
        CreateHazard("S5_Fire_1", HazardType.Fire, new Vector3(baseX + 14, 13.5f, 0), PrimitiveType.Cube, 1.5f);
        CreateHazard("S5_Enemy_2", HazardType.Enemy, new Vector3(baseX + 19, 15.5f, 0), PrimitiveType.Sphere, 1.3f, true);
        CreateHazard("S5_Fire_2", HazardType.Fire, new Vector3(baseX + 23, 14.5f, 0), PrimitiveType.Cube, 1.2f);

        CreateFakePlatformAt("S5_FakePlatform_FINAL", new Vector3(baseX + 20, 14, 0), new Vector3(3, 0.5f, 3));
    }

    static void CreateShrinkPlatform(string name, Vector3 pos, Vector3 size)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<BoxCollider>().isTrigger = false;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MAT_Platform.mat");
        if (mat != null) go.GetComponent<Renderer>().material = new Material(mat);

        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = new Color(0.6f, 0.4f, 0.8f);
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", new Color(0.3f, 0.1f, 0.5f));
        }

        go.AddComponent<PlatformShrink>();
        EditorUtility.SetDirty(go);
    }

    static void CreateFloatingCodeText()
    {
        string[][] textData = {
            new[] { "CodeText_1", "if (alive) {", "10", "8", "0" },
            new[] { "CodeText_2", "} else { die(); }", "25", "6", "0" },
            new[] { "CodeText_3", "while(true) { suffer(); }", "50", "10", "0" },
            new[] { "CodeText_4", "switch(hope) {\n  case 0: break;\n}", "75", "9", "0" },
            new[] { "CodeText_5", "try { win(); }", "100", "12", "0" },
            new[] { "CodeText_6", "catch(Exception) { lol(); }", "110", "11", "0" },
            new[] { "CodeText_7", "// POINT OF NO RETURN", "130", "13", "0" },
            new[] { "CodeText_8", "return ragequit();", "155", "15", "0" },
            new[] { "CodeText_9", "git push --force", "170", "16", "0" },
            new[] { "CodeText_10", "// you made it?\n// are you sure?", "175", "15", "0" },
        };

        foreach (string[] data in textData)
        {
            if (GameObject.Find(data[0]) != null) continue;
            GameObject go = new GameObject(data[0]);
            go.transform.position = new Vector3(float.Parse(data[2]), float.Parse(data[3]), float.Parse(data[4]));
            FloatingText3D ft = go.AddComponent<FloatingText3D>();
            ft.text = data[1];
            ft.color = new Color(
                Random.Range(0f, 0.3f),
                Random.Range(0.5f, 1f),
                Random.Range(0.3f, 1f),
                0.25f
            );
            ft.fontSize = Random.Range(2f, 4f);
            ft.bobSpeed = Random.Range(0.3f, 0.8f);
            ft.bobAmount = Random.Range(0.2f, 0.5f);
            EditorUtility.SetDirty(go);
        }
    }

    static void CreateDecorations()
    {
        for (int i = 0; i < 30; i++)
        {
            string name = "Deco_Pillar_" + i;
            if (GameObject.Find(name) != null) continue;

            float x = i * 6f + Random.Range(-1f, 1f);
            float height = Random.Range(3f, 12f);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = new Vector3(x, -height / 2f - 1f, Random.Range(3f, 8f));
            go.transform.localScale = new Vector3(0.3f, height, 0.3f);

            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            Material mat = new Material(Shader.Find("Standard"));
            float hue = Random.Range(0f, 1f);
            Color c = Color.HSVToRGB(hue, 0.8f, 0.15f);
            mat.color = c;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1f, 0.4f));
            go.GetComponent<Renderer>().material = mat;

            EditorUtility.SetDirty(go);
        }

        for (int i = 0; i < 15; i++)
        {
            string name = "Deco_Ring_" + i;
            if (GameObject.Find(name) != null) continue;

            float x = i * 12f + 5f;
            float y = Random.Range(5f, 15f);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.position = new Vector3(x, y, Random.Range(-2f, 4f));
            go.transform.localScale = new Vector3(2f, 0.1f, 2f);
            go.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), Random.Range(0f, 90f));

            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            float hue = Random.Range(0.5f, 0.9f);
            mat.color = new Color(
                Color.HSVToRGB(hue, 0.8f, 0.3f).r,
                Color.HSVToRGB(hue, 0.8f, 0.3f).g,
                Color.HSVToRGB(hue, 0.8f, 0.3f).b,
                0.3f
            );
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1f, 0.6f));
            go.GetComponent<Renderer>().material = mat;

            EditorUtility.SetDirty(go);
        }
    }

    static void CreateSawBlades()
    {
        CreateSawBladeAt("SawBlade_1", new Vector3(70, 5, 0), 4f, 5f, false);
        CreateSawBladeAt("SawBlade_2", new Vector3(105, 8, 0), 5f, 6f, true);
        CreateSawBladeAt("SawBlade_3", new Vector3(140, 11, 0), 6f, 4f, false);
        CreateSawBladeAt("SawBlade_4", new Vector3(170, 14, 0), 5f, 5f, true);
    }

    static void CreateSawBladeAt(string name, Vector3 pos, float speed, float range, bool vertical)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        go.GetComponent<CapsuleCollider>().isTrigger = true;

        MovingSawBlade sb = go.AddComponent<MovingSawBlade>();
        sb.moveSpeed = speed;
        sb.moveRange = range;
        sb.vertical = vertical;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.8f, 0.2f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 2f);
        go.GetComponent<Renderer>().material = mat;

        EditorUtility.SetDirty(go);
    }

    static void ExtendKillZone()
    {
        GameObject kz = GameObject.Find("KillZone");
        if (kz != null)
        {
            kz.transform.position = new Vector3(90, -8, 0);
            kz.transform.localScale = new Vector3(250, 1, 20);
            EditorUtility.SetDirty(kz);
        }
    }

    static void CreatePlatform(string name, Vector3 pos, Vector3 size, bool dark)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<BoxCollider>().isTrigger = false;

        string matPath = dark ? "Assets/Materials/MAT_Platform_Dark.mat" : "Assets/Materials/MAT_Platform.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) go.GetComponent<Renderer>().material = mat;

        EditorUtility.SetDirty(go);
    }

    static void CreateHazard(string name, HazardType type, Vector3 pos, PrimitiveType shape, float scale, bool moving = false)
    {
        if (GameObject.Find(name) != null) return;
        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;

        Collider col = go.GetComponent<Collider>();
        col.isTrigger = true;

        Hazard h = go.AddComponent<Hazard>();
        h.hazardType = type;
        go.AddComponent<HazardVisualFeedback>();
        go.AddComponent<HazardPulse>();
        go.AddComponent<HazardGlow>();

        string matPath = "Assets/Materials/MAT_Fire.mat";
        if (type == HazardType.Spike) matPath = "Assets/Materials/MAT_Spike.mat";
        if (type == HazardType.Enemy) matPath = "Assets/Materials/MAT_Enemy.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) go.GetComponent<Renderer>().material = mat;

        if (moving)
        {
            MovingEnemy me = go.AddComponent<MovingEnemy>();
            me.moveSpeed = Random.Range(2f, 4f);
            me.moveRange = Random.Range(2f, 4f);
            me.vertical = Random.value > 0.5f;
        }

        GameObject particles = new GameObject("Particles");
        particles.transform.SetParent(go.transform, false);
        ParticleSystem ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime = 1.5f;
        main.startSpeed = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        EditorUtility.SetDirty(go);
    }
}
