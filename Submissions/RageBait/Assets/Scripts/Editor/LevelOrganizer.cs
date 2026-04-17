using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelOrganizer : EditorWindow
{
    [MenuItem("Tools/Organize Into Levels")]
    public static void Organize()
    {
        // Create 3 level zone parent objects
        GameObject level1 = GetOrCreateZone("Level_1_TrustBuilding", 1);
        GameObject level2 = GetOrCreateZone("Level_2_FirstBetrayal", 2);
        GameObject level3 = GetOrCreateZone("Level_3_TotalChaos", 3);

        // === LEVEL 1: Original platforms + first hazards (X: 0 to 55) ===
        ParentByName(level1, new string[] {
            "Platform_Start", "Platform_2", "Platform_3", "Platform_4",
            "Platform_5", "Platform_6", "Platform_7", "Platform_8", "Platform_End",
            "Hazard_Fire_1", "Hazard_Fire_2", "Hazard_Fire_3",
            "Hazard_Spike_1", "Hazard_Spike_2", "Hazard_Spike_3",
            "Hazard_Enemy_1", "Hazard_Enemy_2",
            "FakePlatform_1", "FakePlatform_2", "FakePlatform_3",
            "GravityZone_1",
            "InvisWall_1", "InvisWall_2",
            "SpeedTrap_Fast_1",
            "FakeCheckpoint_1",
            "JumpScare_1",
            "SawBlade_1"
        });

        // Create L1 spawn point
        GameObject l1Spawn = GetOrCreate("L1_Spawn", level1.transform);
        l1Spawn.transform.position = new Vector3(0, 1.5f, 0);

        // L1 EndZone — move Platform_End to act as end trigger area
        // Create a new end trigger for L1 at the end of section 1
        GameObject l1End = GetOrCreate("L1_EndTrigger", level1.transform);
        l1End.transform.position = new Vector3(55, 8, 0);
        BoxCollider l1EndCol = l1End.GetComponent<BoxCollider>();
        if (l1EndCol == null) l1EndCol = l1End.AddComponent<BoxCollider>();
        l1EndCol.isTrigger = true;
        l1EndCol.size = new Vector3(4, 4, 3);
        if (l1End.GetComponent<EndZone>() == null) l1End.AddComponent<EndZone>();

        // Wire LevelZone
        LevelZone lz1 = level1.GetComponent<LevelZone>();
        lz1.spawnPoint = l1Spawn.transform;

        // === LEVEL 2: Section 2 + 3 content (repositioned to start at X:0) ===
        string[] l2Objects = {
            "S2_Platform_1", "S2_Platform_2", "S2_Platform_3", "S2_Platform_4", "S2_Platform_5",
            "S2_Fire_1", "S2_Spike_1", "S2_Enemy_1", "S2_Fire_2",
            "S2_FakePlatform", "S2_Shrink_1",
            "S3_Platform_1", "S3_Platform_2", "S3_Platform_3", "S3_Platform_4",
            "S3_Enemy_1", "S3_Spike_1", "S3_Fire_1", "S3_Enemy_2",
            "S3_FakePlatform_Final", "S3_Shrink_1",
            "GravityZone_2",
            "InvisWall_3",
            "SpeedTrap_Slow_1", "SpeedTrap_Fast_2",
            "FakeCheckpoint_2", "RealCheckpoint_1",
            "JumpScare_2",
            "Teleport_Back_1",
            "SawBlade_2"
        };
        ParentByName(level2, l2Objects);

        // Reposition L2 platforms to create proper walkable path
        RepositionL2(level2);

        // L2 spawn
        GameObject l2Spawn = GetOrCreate("L2_Spawn", level2.transform);
        l2Spawn.transform.localPosition = new Vector3(0, 1.5f, 0);

        // L2 EndTrigger
        GameObject l2End = GetOrCreate("L2_EndTrigger", level2.transform);
        l2End.transform.localPosition = new Vector3(48, 8, 0);
        BoxCollider l2EndCol = l2End.GetComponent<BoxCollider>();
        if (l2EndCol == null) l2EndCol = l2End.AddComponent<BoxCollider>();
        l2EndCol.isTrigger = true;
        l2EndCol.size = new Vector3(4, 4, 3);
        if (l2End.GetComponent<EndZone>() == null) l2End.AddComponent<EndZone>();

        LevelZone lz2 = level2.GetComponent<LevelZone>();
        lz2.spawnPoint = l2Spawn.transform;

        // === LEVEL 3: Section 4 + 5 content (repositioned to start at X:0) ===
        string[] l3Objects = {
            "S4_Platform_1", "S4_Platform_2", "S4_Platform_3", "S4_Platform_4", "S4_Platform_5", "S4_Platform_6",
            "S4_Enemy_1", "S4_Fire_1", "S4_Spike_1", "S4_Enemy_2", "S4_Fire_2",
            "S4_FakePlatform_1", "S4_FakePlatform_2", "S4_Shrink_1", "S4_Shrink_2",
            "S5_Platform_1", "S5_Platform_2", "S5_Platform_3", "S5_Platform_4", "S5_Platform_5",
            "S5_Enemy_1", "S5_Spike_1", "S5_Fire_1", "S5_Enemy_2", "S5_Fire_2",
            "S5_FakePlatform_FINAL",
            "GravityZone_3",
            "InvisWall_4",
            "SpeedTrap_Slow_2",
            "FakeCheckpoint_3",
            "JumpScare_3",
            "Teleport_Back_2", "Teleport_Start_1",
            "SawBlade_3", "SawBlade_4"
        };
        ParentByName(level3, l3Objects);

        // Reposition L3 platforms to create proper walkable path
        RepositionL3(level3);

        // L3 spawn
        GameObject l3Spawn = GetOrCreate("L3_Spawn", level3.transform);
        l3Spawn.transform.localPosition = new Vector3(0, 1.5f, 0);

        // Move the real EndZone into L3
        GameObject endZone = GameObject.Find("EndZone");
        if (endZone != null)
        {
            endZone.transform.SetParent(level3.transform, false);
            endZone.transform.localPosition = new Vector3(60, 8, 0);
        }

        LevelZone lz3 = level3.GetComponent<LevelZone>();
        lz3.spawnPoint = l3Spawn.transform;

        // Update SpawnPoint to match L1
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint != null)
            spawnPoint.transform.position = new Vector3(0, 1.5f, 0);

        // Deactivate L2 and L3 (L1 active by default)
        level2.SetActive(false);
        level3.SetActive(false);

        // Update KillZone to be global (not parented to any level)
        GameObject killZone = GameObject.Find("KillZone");
        if (killZone != null)
        {
            killZone.transform.SetParent(null);
            killZone.transform.position = new Vector3(50, -8, 0);
            killZone.transform.localScale = new Vector3(300, 1, 20);
        }

        // Mark dirty and save
        EditorUtility.SetDirty(level1);
        EditorUtility.SetDirty(level2);
        EditorUtility.SetDirty(level3);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[LevelOrganizer] Organized 208 objects into 3 level zones. L2 and L3 deactivated. Scene saved.");
    }

    static GameObject GetOrCreateZone(string name, int levelNum)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.position = Vector3.zero;
        }
        LevelZone lz = go.GetComponent<LevelZone>();
        if (lz == null) lz = go.AddComponent<LevelZone>();
        lz.levelNumber = levelNum;
        return go;
    }

    static void ParentByName(GameObject parent, string[] names)
    {
        foreach (string name in names)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj.transform.parent == null)
            {
                obj.transform.SetParent(parent.transform, true); // Keep world position
                EditorUtility.SetDirty(obj);
            }
        }
    }

    static void RepositionL2(GameObject l2)
    {
        SP(l2, "S2_Platform_1", 0f, -0.5f);
        SP(l2, "S2_Platform_2", 8f, 1f);
        SP(l2, "S2_Platform_3", 14f, 0.5f);
        SP(l2, "S2_Platform_4", 20f, 2f);
        SP(l2, "S2_Platform_5", 27f, 1.5f);
        SP(l2, "S3_Platform_1", 33f, 3f);
        SP(l2, "S3_Platform_2", 39f, 4f);
        SP(l2, "S3_Platform_3", 44f, 3.5f);
        SP(l2, "S3_Platform_4", 48f, 5f);
        SP(l2, "S2_Fire_1", 3f, 1f); SP(l2, "S2_Spike_1", 12f, 2f);
        SP(l2, "S2_Enemy_1", 18f, 2f); SP(l2, "S2_Fire_2", 24f, 3.5f);
        SP(l2, "S3_Enemy_1", 36f, 4.5f); SP(l2, "S3_Spike_1", 41f, 5f);
        SP(l2, "S3_Fire_1", 46f, 5f); SP(l2, "S3_Enemy_2", 47f, 6.5f);
        SP(l2, "S2_FakePlatform", 11f, 1.5f); SP(l2, "S3_FakePlatform_Final", 46f, 5.5f);
        SP(l2, "S2_Shrink_1", 5f, -0.5f); SP(l2, "S3_Shrink_1", 38f, 4f);
        SP(l2, "GravityZone_2", 25f, 3f); SP(l2, "InvisWall_3", 15f, 2f);
        SP(l2, "SpeedTrap_Slow_1", 30f, 3f); SP(l2, "SpeedTrap_Fast_2", 42f, 4f);
        SP(l2, "FakeCheckpoint_2", 20f, 3f); SP(l2, "RealCheckpoint_1", 10f, 1.5f);
        SP(l2, "JumpScare_2", 22f, 3f); SP(l2, "Teleport_Back_1", 35f, 4f);
        SP(l2, "SawBlade_2", 28f, 2.5f); SP(l2, "Ground_L2", 25f, -2f);
        Transform e2 = l2.transform.Find("L2_EndTrigger");
        if (e2 != null) e2.localPosition = new Vector3(50f, 6f, 0f);
    }

    static void RepositionL3(GameObject l3)
    {
        SP(l3, "S4_Platform_1", 0f, -0.5f);
        SP(l3, "S4_Platform_2", 7f, 1f);
        SP(l3, "S4_Platform_3", 13f, 0.5f);
        SP(l3, "S4_Platform_4", 19f, 2f);
        SP(l3, "S4_Platform_5", 25f, 1.5f);
        SP(l3, "S4_Platform_6", 30f, 3f);
        SP(l3, "S5_Platform_1", 36f, 2.5f);
        SP(l3, "S5_Platform_2", 42f, 4f);
        SP(l3, "S5_Platform_3", 47f, 3.5f);
        SP(l3, "S5_Platform_4", 52f, 5f);
        SP(l3, "S5_Platform_5", 57f, 4.5f);
        SP(l3, "S4_Enemy_1", 4f, 1f); SP(l3, "S4_Fire_1", 10f, 2f);
        SP(l3, "S4_Spike_1", 16f, 1.5f); SP(l3, "S4_Enemy_2", 22f, 3f);
        SP(l3, "S4_Fire_2", 28f, 2.5f); SP(l3, "S5_Enemy_1", 39f, 4f);
        SP(l3, "S5_Spike_1", 44f, 5f); SP(l3, "S5_Fire_1", 50f, 4.5f);
        SP(l3, "S5_Enemy_2", 55f, 6f); SP(l3, "S5_Fire_2", 58f, 6f);
        SP(l3, "S4_FakePlatform_1", 10f, 1.5f); SP(l3, "S4_FakePlatform_2", 22f, 2.5f);
        SP(l3, "S5_FakePlatform_FINAL", 55f, 5f);
        SP(l3, "S4_Shrink_1", 15f, 0.5f); SP(l3, "S4_Shrink_2", 27f, 1.5f);
        SP(l3, "GravityZone_3", 20f, 3f); SP(l3, "InvisWall_4", 35f, 3f);
        SP(l3, "SpeedTrap_Slow_2", 40f, 3.5f); SP(l3, "FakeCheckpoint_3", 45f, 4f);
        SP(l3, "JumpScare_3", 15f, 2f); SP(l3, "Teleport_Back_2", 30f, 3.5f);
        SP(l3, "Teleport_Start_1", 53f, 5.5f);
        SP(l3, "SawBlade_3", 18f, 2f); SP(l3, "SawBlade_4", 48f, 5f);
        SP(l3, "Ground_L3", 30f, -2f);
        Transform endZone = l3.transform.Find("EndZone");
        if (endZone != null) endZone.localPosition = new Vector3(62f, 6f, 0f);
    }

    static void SP(GameObject parent, string name, float x, float y)
    {
        Transform t = parent.transform.Find(name);
        if (t != null) { t.localPosition = new Vector3(x, y, 0f); EditorUtility.SetDirty(t.gameObject); }
    }

    static GameObject GetOrCreate(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }
}
