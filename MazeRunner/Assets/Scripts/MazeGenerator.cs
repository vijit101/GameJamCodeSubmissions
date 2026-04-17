using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int mazeWidth = 15;
    public int mazeHeight = 15;
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.3f;

    [Header("Materials (overridden by RunConfig theme at runtime)")]
    public Material wallMaterial;
    public Material floorMaterial;

    [Header("References")]
    public Transform player;
    public GameObject exitMarker;

    private bool[,] visited;
    private bool[,,] walls; // [x, z, direction] — 0=N, 1=E, 2=S, 3=W
    private GameObject mazeParent;
    private List<Vector2Int> pathCells = new List<Vector2Int>();

    public bool[,,] Walls => walls;
    public bool IsBuilt => walls != null;
    public ThemeSystem.Theme ActiveTheme { get; private set; }

    public Vector2Int WorldToCell(Vector3 world)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(world.x / cellSize), 0, mazeWidth - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(world.z / cellSize), 0, mazeHeight - 1);
        return new Vector2Int(x, z);
    }

    public Vector3 CellToWorld(Vector2Int cell, float y = 1f)
    {
        return new Vector3(cell.x * cellSize + cellSize / 2f, y, cell.y * cellSize + cellSize / 2f);
    }

    public Vector2Int NextStepToward(Vector2Int from, Vector2Int to)
    {
        if (walls == null || from == to) return new Vector2Int(-1, -1);

        var prev = new Vector2Int[mazeWidth, mazeHeight];
        var seen = new bool[mazeWidth, mazeHeight];
        for (int x = 0; x < mazeWidth; x++)
            for (int z = 0; z < mazeHeight; z++)
                prev[x, z] = new Vector2Int(-1, -1);

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(from);
        seen[from.x, from.y] = true;

        bool found = false;
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == to) { found = true; break; }

            for (int d = 0; d < 4; d++)
            {
                if (walls[cur.x, cur.y, d]) continue;
                Vector2Int n = cur;
                if (d == 0) n.y += 1;
                else if (d == 1) n.x += 1;
                else if (d == 2) n.y -= 1;
                else n.x -= 1;
                if (n.x < 0 || n.x >= mazeWidth || n.y < 0 || n.y >= mazeHeight) continue;
                if (seen[n.x, n.y]) continue;
                seen[n.x, n.y] = true;
                prev[n.x, n.y] = cur;
                queue.Enqueue(n);
            }
        }

        if (!found) return new Vector2Int(-1, -1);

        var step = to;
        while (prev[step.x, step.y] != from)
        {
            var p = prev[step.x, step.y];
            if (p.x < 0) return new Vector2Int(-1, -1);
            step = p;
        }
        return step;
    }

    void Awake()
    {
        // Pick maze size from RunConfig before any other system queries us.
        if (RunConfig.MazeSize > 0)
        {
            mazeWidth = mazeHeight = RunConfig.MazeSize;
        }

        // Seed RNG so daily/specific seeds reproduce.
        int seed = RunConfig.seed != 0 ? RunConfig.seed : RunConfig.RandomSeed();
        if (RunConfig.dailyMode) seed = RunConfig.TodaysSeed();
        RunConfig.seed = seed;
        Random.InitState(seed);

        // Theme materials.
        ActiveTheme = ThemeSystem.PickFromSeed(seed);
        wallMaterial = ThemeSystem.MakeMaterial(ActiveTheme.wall, ActiveTheme.wallSmoothness);
        floorMaterial = ThemeSystem.MakeMaterial(ActiveTheme.floor, ActiveTheme.floorSmoothness);
    }

    void Start()
    {
        GenerateMaze();
        BuildMaze3D();
        PlacePlayerAndExit();
        SpawnPickups();
        SpawnCeilingFixtures();
        SpawnCeilingPipes();
        SpawnFloorRituals();
        SpawnFloorPuddles();
        SpawnFloorBones();
        SpawnFloorCracks();
        SpawnWallDecals();
        SpawnWallFrames();
        SpawnWallWriting();
        SpawnWallCandles();
        SpawnPillars();
        SpawnHangingFigures();
        SpawnEmergencyLights();
        SpawnToxicGlow();
        SpawnDripEmitters();

        // New: big, obvious horror elements.
        SpawnMannequins();
        SpawnWallEyes();
        SpawnMeatHooks();
        SpawnDrippingBlood();
        SpawnFloorHearts();
        SpawnHandprintTrails();
    }

    void GenerateMaze()
    {
        visited = new bool[mazeWidth, mazeHeight];
        walls = new bool[mazeWidth, mazeHeight, 4];

        for (int x = 0; x < mazeWidth; x++)
            for (int z = 0; z < mazeHeight; z++)
                for (int d = 0; d < 4; d++) walls[x, z, d] = true;

        var stack = new Stack<Vector2Int>();
        var start = new Vector2Int(0, 0);
        visited[0, 0] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var unvisited = GetUnvisitedNeighbors(current.x, current.y);

            if (unvisited.Count > 0)
            {
                int direction = unvisited[Random.Range(0, unvisited.Count)];
                var neighbor = GetNeighbor(current, direction);
                RemoveWall(current.x, current.y, direction);
                visited[neighbor.x, neighbor.y] = true;
                stack.Push(neighbor);
            }
            else stack.Pop();
        }

        // Cache flat list of all cells for pickup placement.
        pathCells.Clear();
        for (int x = 0; x < mazeWidth; x++)
            for (int z = 0; z < mazeHeight; z++)
                pathCells.Add(new Vector2Int(x, z));
    }

    List<int> GetUnvisitedNeighbors(int x, int z)
    {
        var n = new List<int>();
        if (z + 1 < mazeHeight && !visited[x, z + 1]) n.Add(0);
        if (x + 1 < mazeWidth  && !visited[x + 1, z]) n.Add(1);
        if (z - 1 >= 0          && !visited[x, z - 1]) n.Add(2);
        if (x - 1 >= 0          && !visited[x - 1, z]) n.Add(3);
        return n;
    }

    Vector2Int GetNeighbor(Vector2Int cell, int direction)
    {
        return direction switch
        {
            0 => new Vector2Int(cell.x, cell.y + 1),
            1 => new Vector2Int(cell.x + 1, cell.y),
            2 => new Vector2Int(cell.x, cell.y - 1),
            3 => new Vector2Int(cell.x - 1, cell.y),
            _ => cell,
        };
    }

    void RemoveWall(int x, int z, int direction)
    {
        walls[x, z, direction] = false;
        var neighbor = GetNeighbor(new Vector2Int(x, z), direction);
        int opposite = (direction + 2) % 4;
        walls[neighbor.x, neighbor.y, opposite] = false;
    }

    void BuildMaze3D()
    {
        mazeParent = new GameObject("Maze");

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        float worldW = mazeWidth * cellSize;
        float worldH = mazeHeight * cellSize;
        floor.transform.position = new Vector3(worldW / 2f, 0, worldH / 2f);
        floor.transform.localScale = new Vector3(worldW / 10f, 1, worldH / 10f);
        floor.transform.parent = mazeParent.transform;
        if (floorMaterial != null) floor.GetComponent<Renderer>().material = floorMaterial;

        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector3(worldW / 2f, wallHeight, worldH / 2f);
        ceiling.transform.localScale = new Vector3(worldW / 10f, 1, worldH / 10f);
        ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
        ceiling.transform.parent = mazeParent.transform;
        var ceilMat = ThemeSystem.MakeMaterial(ActiveTheme.ceiling, 0.05f);
        ceiling.GetComponent<Renderer>().material = ceilMat;

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                float cellX = x * cellSize;
                float cellZ = z * cellSize;

                if (walls[x, z, 0])
                    CreateWall(new Vector3(cellX + cellSize / 2f, wallHeight / 2f, cellZ + cellSize),
                        new Vector3(cellSize, wallHeight, wallThickness), $"Wall_N_{x}_{z}");

                if (walls[x, z, 1])
                    CreateWall(new Vector3(cellX + cellSize, wallHeight / 2f, cellZ + cellSize / 2f),
                        new Vector3(wallThickness, wallHeight, cellSize), $"Wall_E_{x}_{z}");

                if (z == 0 && walls[x, z, 2])
                    CreateWall(new Vector3(cellX + cellSize / 2f, wallHeight / 2f, cellZ),
                        new Vector3(cellSize, wallHeight, wallThickness), $"Wall_S_{x}_{z}");

                if (x == 0 && walls[x, z, 3])
                    CreateWall(new Vector3(cellX, wallHeight / 2f, cellZ + cellSize / 2f),
                        new Vector3(wallThickness, wallHeight, cellSize), $"Wall_W_{x}_{z}");
            }
        }
    }

    void CreateWall(Vector3 position, Vector3 scale, string wallName)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = wallName;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = mazeParent.transform;
        if (wallMaterial != null) wall.GetComponent<Renderer>().material = wallMaterial;
    }

    void PlacePlayerAndExit()
    {
        if (player != null)
            player.position = new Vector3(cellSize / 2f, 1f, cellSize / 2f);

        if (exitMarker != null)
        {
            exitMarker.transform.position = new Vector3(
                (mazeWidth - 1) * cellSize + cellSize / 2f,
                0.5f,
                (mazeHeight - 1) * cellSize + cellSize / 2f);

            var box = exitMarker.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.isTrigger = true;
                if (box.size.x < 2f || box.size.y < 2f || box.size.z < 2f)
                    box.size = new Vector3(3f, 3f, 3f);
            }

            // Tint the exit marker to the theme's exit color.
            foreach (var l in exitMarker.GetComponentsInChildren<Light>())
                l.color = ActiveTheme.exitColor;
            foreach (var r in exitMarker.GetComponentsInChildren<Renderer>())
            {
                var em = ThemeSystem.MakeMaterial(ActiveTheme.exitColor, 0.6f, true);
                r.material = em;
            }

            Physics.SyncTransforms();
        }
    }

    void SpawnPickups()
    {
        int wantPages = RunConfig.PageCount;
        int wantBatts = RunConfig.BatteryCount;

        // Avoid the start cell and the exit cell.
        var startCell = new Vector2Int(0, 0);
        var exitCell  = new Vector2Int(mazeWidth - 1, mazeHeight - 1);

        // Sort cells by distance from start (Manhattan) and skip the closest few.
        var candidates = new List<Vector2Int>(pathCells);
        candidates.RemoveAll(c => c == startCell || c == exitCell);
        // Shuffle deterministically with the same RNG so seed reproduces layout.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // Pages first, batteries after — both pulled from the shuffled list.
        int placed = 0;
        for (int i = 0; i < candidates.Count && placed < wantPages; i++)
        {
            // Avoid clustering near spawn for the first few pages.
            if ((candidates[i] - startCell).sqrMagnitude < 4) continue;
            SpawnPickup(candidates[i], PickupType.Page);
            candidates[i] = new Vector2Int(-1, -1);
            placed++;
        }
        candidates.RemoveAll(c => c.x < 0);

        int placedB = 0;
        for (int i = 0; i < candidates.Count && placedB < wantBatts; i++)
        {
            SpawnPickup(candidates[i], PickupType.Battery);
            placedB++;
        }

        ScoreSystem.totalPages = wantPages;
        ScoreSystem.totalBatteries = wantBatts;
    }

    void SpawnCeilingFixtures()
    {
        // Hanging bulbs on a sparse grid. Colour varies so the corridors feel
        // lived-in and broken: mostly amber, some red emergency, a rare sickly green.
        Color[] palette =
        {
            new Color(1.00f, 0.78f, 0.42f),  // amber
            new Color(1.00f, 0.78f, 0.42f),
            new Color(1.00f, 0.78f, 0.42f),
            new Color(1.00f, 0.55f, 0.30f),  // dim orange
            new Color(0.95f, 0.25f, 0.22f),  // red emergency
            new Color(0.55f, 0.95f, 0.50f),  // sickly green (rare)
        };

        for (int x = 1; x < mazeWidth; x += 3)
        {
            for (int z = 1; z < mazeHeight; z += 3)
            {
                if (Random.value < 0.25f) continue; // skip some for irregularity

                float cx = x * cellSize + cellSize / 2f;
                float cz = z * cellSize + cellSize / 2f;
                float hangY = wallHeight - Random.Range(0.4f, 0.9f);

                var bulbColor = palette[Random.Range(0, palette.Length)];

                var fixture = new GameObject("CeilingBulb");
                fixture.transform.position = new Vector3(cx, hangY, cz);
                fixture.transform.parent = mazeParent.transform;

                var bulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bulb.name = "Bulb";
                bulb.transform.SetParent(fixture.transform, false);
                bulb.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                Destroy(bulb.GetComponent<Collider>());
                bulb.GetComponent<Renderer>().material =
                    ThemeSystem.MakeMaterial(bulbColor, 0.4f, true);

                var wire = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wire.name = "Wire";
                wire.transform.SetParent(fixture.transform, false);
                wire.transform.localPosition = new Vector3(0f, 0.32f, 0f);
                wire.transform.localScale = new Vector3(0.035f, 0.6f, 0.035f);
                Destroy(wire.GetComponent<Collider>());
                wire.GetComponent<Renderer>().material =
                    ThemeSystem.MakeMaterial(new Color(0.04f, 0.04f, 0.04f), 0.9f);

                var lightGo = new GameObject("Light");
                lightGo.transform.SetParent(fixture.transform, false);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = bulbColor;
                light.range = Random.Range(3.8f, 5.5f);
                light.intensity = Random.Range(0.45f, 1.05f);
                lightGo.AddComponent<CeilingLight>();
            }
        }
    }

    // ─── HORROR DRESSING ──────────────────────────────────────────────────────

    static readonly string[] WallMessages =
    {
        "HELP",
        "IT SEES YOU",
        "TURN BACK",
        "DON'T LOOK",
        "RUN",
        "HE KNOWS",
        "TOO LATE",
        "GET OUT",
        "THEY WATCH",
        "NO ESCAPE",
    };

    void SpawnFloorRituals()
    {
        int count = Mathf.Clamp(mazeWidth / 5, 1, 3);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(2, mazeWidth - 1);
            int z = Random.Range(2, mazeHeight - 1);
            if (x == 0 && z == 0) continue;

            float cx = x * cellSize + cellSize / 2f;
            float cz = z * cellSize + cellSize / 2f;

            var glyph = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glyph.name = "Ritual";
            glyph.transform.parent = mazeParent.transform;
            glyph.transform.position = new Vector3(cx, 0.02f, cz);
            glyph.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            glyph.transform.localScale = Vector3.one * (cellSize * 0.75f);
            Destroy(glyph.GetComponent<Collider>());
            glyph.GetComponent<Renderer>().material =
                ThemeSystem.MakeMaterial(new Color(0.55f, 0.02f, 0.02f), 0.2f, true);

            // Pulsing blood-red light underneath.
            var lightGo = new GameObject("RitualGlow");
            lightGo.transform.SetParent(glyph.transform.parent, false);
            lightGo.transform.position = new Vector3(cx, 0.5f, cz);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.95f, 0.12f, 0.12f);
            light.range = 3.5f;
            light.intensity = 1.4f;
            lightGo.AddComponent<PulsatingLight>();
        }
    }

    void SpawnWallDecals()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                if (Random.value > 0.22f) continue;
                for (int d = 0; d < 4; d++)
                {
                    int dir = (d + Random.Range(0, 4)) % 4;
                    if (!walls[x, z, dir]) continue;
                    // Avoid duplicates: only draw S/W on bottom/left edges.
                    if (dir == 2 && z != 0) continue;
                    if (dir == 3 && x != 0) continue;
                    PlaceWallDecal(x, z, dir);
                    break;
                }
            }
        }
    }

    void PlaceWallDecal(int x, int z, int direction)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = Random.Range(0.3f, 2.4f);
        float inset = wallThickness * 0.5f + 0.02f;

        Vector3 pos; Quaternion rot;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX + Random.Range(-0.7f, 0.7f), y, z * cellSize + cellSize - inset);
                    rot = Quaternion.Euler(0f, 180f, 0f); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ + Random.Range(-0.7f, 0.7f));
                    rot = Quaternion.Euler(0f, -90f, 0f); break;
            case 2: pos = new Vector3(cellX + Random.Range(-0.7f, 0.7f), y, z * cellSize + inset);
                    rot = Quaternion.Euler(0f, 0f, 0f); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ + Random.Range(-0.7f, 0.7f));
                    rot = Quaternion.Euler(0f, 90f, 0f); break;
        }

        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "WallDecal";
        q.transform.parent = mazeParent.transform;
        q.transform.position = pos;
        q.transform.rotation = rot;
        q.transform.localScale = new Vector3(Random.Range(0.45f, 1.1f), Random.Range(0.45f, 1.1f), 1f);
        Destroy(q.GetComponent<Collider>());

        Color c = Random.value < 0.65f
            ? new Color(0.25f, 0.02f, 0.02f)        // dark blood
            : new Color(0.04f, 0.04f, 0.04f);       // scratch / scorch
        q.GetComponent<Renderer>().material = ThemeSystem.MakeMaterial(c, 0.1f);
    }

    void SpawnWallWriting()
    {
        int count = Mathf.Clamp(mazeWidth / 5, 2, 4);
        int attempts = 0;
        int placed = 0;
        while (placed < count && attempts < 40)
        {
            attempts++;
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;

            PlaceWallText(x, z, dir, WallMessages[Random.Range(0, WallMessages.Length)]);
            placed++;
        }
    }

    void PlaceWallText(int x, int z, int direction, string msg)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = Random.Range(1.1f, 2.1f);
        float inset = wallThickness * 0.5f + 0.02f;

        Vector3 pos; Quaternion rot;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX, y, z * cellSize + cellSize - inset);
                    rot = Quaternion.Euler(0f, 180f, 0f); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ);
                    rot = Quaternion.Euler(0f, -90f, 0f); break;
            case 2: pos = new Vector3(cellX, y, z * cellSize + inset);
                    rot = Quaternion.Euler(0f, 0f, 0f); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ);
                    rot = Quaternion.Euler(0f, 90f, 0f); break;
        }

        var go = new GameObject("WallText");
        go.transform.parent = mazeParent.transform;
        go.transform.position = pos;
        go.transform.rotation = rot;

        var tmp = go.AddComponent<TMPro.TextMeshPro>();
        tmp.text = msg;
        tmp.fontSize = 7;
        tmp.color = new Color(0.78f, 0.08f, 0.06f);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.fontStyle = TMPro.FontStyles.Bold;

        var rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(2.8f, 0.8f);

        // Slight random tilt so each writing looks hand-scrawled.
        go.transform.Rotate(0f, 0f, Random.Range(-6f, 6f));
    }

    void SpawnPillars()
    {
        int count = Mathf.Clamp(mazeWidth / 4, 2, 5);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(2, mazeWidth - 2);
            int z = Random.Range(2, mazeHeight - 2);
            // Skip start and exit corners.
            if (x + z < 3 || x >= mazeWidth - 2 && z >= mazeHeight - 2) continue;

            float cx = x * cellSize + cellSize / 2f;
            float cz = z * cellSize + cellSize / 2f;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Pillar";
            pillar.transform.parent = mazeParent.transform;
            pillar.transform.position = new Vector3(cx, wallHeight / 2f, cz);
            pillar.transform.localScale = new Vector3(0.7f, wallHeight, 0.7f);
            pillar.GetComponent<Renderer>().material =
                ThemeSystem.MakeMaterial(ActiveTheme.wall * 0.85f, ActiveTheme.wallSmoothness);
        }
    }

    void SpawnHangingFigures()
    {
        // Only dead-ends (3 walls present). Place figures high above head height.
        var deadEnds = new System.Collections.Generic.List<Vector2Int>();
        for (int x = 0; x < mazeWidth; x++)
            for (int z = 0; z < mazeHeight; z++)
            {
                int walled = 0;
                for (int d = 0; d < 4; d++) if (walls[x, z, d]) walled++;
                if (walled >= 3 && !(x == 0 && z == 0))
                    deadEnds.Add(new Vector2Int(x, z));
            }

        // Shuffle.
        for (int i = deadEnds.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (deadEnds[i], deadEnds[j]) = (deadEnds[j], deadEnds[i]);
        }

        int count = Mathf.Min(deadEnds.Count, Mathf.Clamp(mazeWidth / 6, 1, 3));
        for (int i = 0; i < count; i++) PlaceHangingFigure(deadEnds[i]);
    }

    void PlaceHangingFigure(Vector2Int cell)
    {
        float cx = cell.x * cellSize + cellSize / 2f;
        float cz = cell.y * cellSize + cellSize / 2f;

        var root = new GameObject("Hanging");
        root.transform.parent = mazeParent.transform;
        root.transform.position = new Vector3(cx, wallHeight, cz);

        // Chain from ceiling.
        var chain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chain.name = "Chain";
        chain.transform.SetParent(root.transform, false);
        chain.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        chain.transform.localScale = new Vector3(0.045f, 0.7f, 0.045f);
        Destroy(chain.GetComponent<Collider>());
        chain.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.06f, 0.06f, 0.06f), 0.9f);

        // Dangling body — kept high enough to clear player head.
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, -1.1f, 0f);
        body.transform.localScale = new Vector3(0.38f, 0.52f, 0.38f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.04f, 0.03f, 0.03f), 0.35f);

        root.AddComponent<SwayAnimator>();
    }

    void SpawnDripEmitters()
    {
        int count = Mathf.Clamp(mazeWidth / 4, 2, 5);
        var clip = ProceduralAudio.MakeDrip();
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);

            var go = new GameObject("Drip");
            go.transform.parent = mazeParent.transform;
            go.transform.position = new Vector3(
                x * cellSize + cellSize / 2f,
                wallHeight - 0.1f,
                z * cellSize + cellSize / 2f);

            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.spatialBlend = 1f;
            src.minDistance = 1f;
            src.maxDistance = 10f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.playOnAwake = false;

            go.AddComponent<DripPlayer>();
        }
    }

    void SpawnCeilingPipes()
    {
        // A few rusty pipes running along the ceiling for silhouette detail.
        var pipeMat = ThemeSystem.MakeMaterial(new Color(0.10f, 0.08f, 0.07f), 0.55f);
        int pipeCount = Mathf.Clamp(mazeWidth / 3, 2, 6);
        for (int i = 0; i < pipeCount; i++)
        {
            int x = Random.Range(0, mazeWidth);
            int z0 = Random.Range(0, mazeHeight - 2);
            int runZ = Random.Range(2, 5);
            float cx = x * cellSize + cellSize / 2f;
            float cz = (z0 + runZ * 0.5f) * cellSize + cellSize / 2f;
            float length = runZ * cellSize;

            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pipe.name = "Pipe";
            pipe.transform.parent = mazeParent.transform;
            pipe.transform.position = new Vector3(cx, wallHeight - 0.18f, cz);
            pipe.transform.localScale = new Vector3(0.18f, 0.18f, length);
            Destroy(pipe.GetComponent<Collider>());
            pipe.GetComponent<Renderer>().material = pipeMat;
        }
    }

    // ─── FLOOR DRESSING ───────────────────────────────────────────────────────

    void SpawnFloorPuddles()
    {
        int count = Mathf.Clamp(mazeWidth / 2, 4, 12);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            if (x == 0 && z == 0) continue;

            float cx = x * cellSize + cellSize / 2f + Random.Range(-1.2f, 1.2f);
            float cz = z * cellSize + cellSize / 2f + Random.Range(-1.2f, 1.2f);

            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Puddle";
            q.transform.parent = mazeParent.transform;
            q.transform.position = new Vector3(cx, 0.015f, cz);
            q.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            q.transform.localScale = Vector3.one * Random.Range(0.6f, 1.6f);
            Destroy(q.GetComponent<Collider>());

            Color c = Random.value < 0.75f
                ? new Color(0.22f, 0.01f, 0.01f)   // dark blood
                : new Color(0.03f, 0.05f, 0.02f);  // black oily water
            q.GetComponent<Renderer>().material = ThemeSystem.MakeMaterial(c, 0.7f, true);
        }
    }

    void SpawnFloorBones()
    {
        int count = Mathf.Clamp(mazeWidth * 2, 8, 24);
        var boneColor = new Color(0.72f, 0.69f, 0.58f);
        var boneMat = ThemeSystem.MakeMaterial(boneColor, 0.25f);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            if (x == 0 && z == 0) continue;

            float cx = x * cellSize + cellSize / 2f + Random.Range(-1.3f, 1.3f);
            float cz = z * cellSize + cellSize / 2f + Random.Range(-1.3f, 1.3f);

            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = "Bone";
            b.transform.parent = mazeParent.transform;
            b.transform.position = new Vector3(cx, 0.05f, cz);
            b.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float len = Random.Range(0.18f, 0.38f);
            b.transform.localScale = new Vector3(len, 0.05f, 0.06f);
            Destroy(b.GetComponent<Collider>());
            b.GetComponent<Renderer>().material = boneMat;
        }
    }

    void SpawnFloorCracks()
    {
        int count = Mathf.Clamp(mazeWidth, 6, 16);
        var crackMat = ThemeSystem.MakeMaterial(new Color(0.008f, 0.008f, 0.012f), 0.1f);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);

            float cx = x * cellSize + cellSize / 2f + Random.Range(-1.5f, 1.5f);
            float cz = z * cellSize + cellSize / 2f + Random.Range(-1.5f, 1.5f);

            var c = GameObject.CreatePrimitive(PrimitiveType.Quad);
            c.name = "Crack";
            c.transform.parent = mazeParent.transform;
            c.transform.position = new Vector3(cx, 0.012f, cz);
            c.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            c.transform.localScale = new Vector3(Random.Range(0.04f, 0.12f),
                                                 Random.Range(1.2f, 2.6f), 1f);
            Destroy(c.GetComponent<Collider>());
            c.GetComponent<Renderer>().material = crackMat;
        }
    }

    // ─── WALL DRESSING (extra) ────────────────────────────────────────────────

    void SpawnWallFrames()
    {
        int count = Mathf.Clamp(mazeWidth / 3, 3, 6);
        int placed = 0, attempts = 0;
        var frameMat = ThemeSystem.MakeMaterial(new Color(0.04f, 0.03f, 0.03f), 0.15f);
        while (placed < count && attempts < 50)
        {
            attempts++;
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;
            PlaceWallFrame(x, z, dir, frameMat);
            placed++;
        }
    }

    void PlaceWallFrame(int x, int z, int direction, Material mat)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = Random.Range(1.2f, 2.1f);
        float inset = wallThickness * 0.5f + 0.015f;

        Vector3 pos; Quaternion rot;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX, y, z * cellSize + cellSize - inset);
                    rot = Quaternion.Euler(0f, 180f, 0f); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ);
                    rot = Quaternion.Euler(0f, -90f, 0f); break;
            case 2: pos = new Vector3(cellX, y, z * cellSize + inset);
                    rot = Quaternion.Euler(0f, 0f, 0f); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ);
                    rot = Quaternion.Euler(0f, 90f, 0f); break;
        }

        var f = GameObject.CreatePrimitive(PrimitiveType.Quad);
        f.name = "Frame";
        f.transform.parent = mazeParent.transform;
        f.transform.position = pos;
        f.transform.rotation = rot;
        f.transform.Rotate(0f, 0f, Random.Range(-5f, 5f)); // crooked
        f.transform.localScale = new Vector3(Random.Range(0.55f, 0.95f),
                                             Random.Range(0.75f, 1.25f), 1f);
        Destroy(f.GetComponent<Collider>());
        f.GetComponent<Renderer>().material = mat;
    }

    void SpawnWallCandles()
    {
        // Wall-mounted candles with warm, flickering flames.
        int count = Mathf.Clamp(mazeWidth / 3, 3, 7);
        int placed = 0, attempts = 0;
        while (placed < count && attempts < 60)
        {
            attempts++;
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;
            PlaceWallCandle(x, z, dir);
            placed++;
        }
    }

    void PlaceWallCandle(int x, int z, int direction)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = Random.Range(1.2f, 2.0f);
        float inset = wallThickness * 0.5f + 0.12f;

        Vector3 pos;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX, y, z * cellSize + cellSize - inset); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ); break;
            case 2: pos = new Vector3(cellX, y, z * cellSize + inset); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ); break;
        }

        // Wax body
        var wax = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wax.name = "Candle";
        wax.transform.parent = mazeParent.transform;
        wax.transform.position = pos;
        wax.transform.localScale = new Vector3(0.08f, 0.26f, 0.08f);
        Destroy(wax.GetComponent<Collider>());
        wax.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.82f, 0.78f, 0.65f), 0.3f);

        // Tiny wick
        var wick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wick.transform.SetParent(wax.transform, false);
        wick.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        wick.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        Destroy(wick.GetComponent<Collider>());
        wick.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.04f, 0.04f, 0.04f), 0.6f);

        // Flame (emissive blob + point light)
        var flameGo = new GameObject("Flame");
        flameGo.transform.SetParent(wax.transform, false);
        flameGo.transform.localPosition = new Vector3(0f, 0.18f, 0f);

        var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.transform.SetParent(flameGo.transform, false);
        flame.transform.localScale = new Vector3(0.6f, 1.4f, 0.6f);
        Destroy(flame.GetComponent<Collider>());
        flame.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(1f, 0.65f, 0.25f), 0.8f, true);

        var light = flameGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.62f, 0.28f);
        light.range = 3.2f;
        light.intensity = 1.05f;
        flameGo.AddComponent<CeilingLight>(); // reuse flicker behaviour
    }

    void SpawnEmergencyLights()
    {
        // Sparse pulsing red lights near the floor — horror game emergency vibe.
        int count = Mathf.Clamp(mazeWidth / 4, 2, 5);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);

            var go = new GameObject("EmergencyLight");
            go.transform.parent = mazeParent.transform;
            go.transform.position = new Vector3(
                x * cellSize + cellSize / 2f,
                0.4f,
                z * cellSize + cellSize / 2f);

            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.95f, 0.08f, 0.08f);
            l.range = 4.2f;
            l.intensity = 0.75f;
            var pulse = go.AddComponent<PulsatingLight>();
            pulse.speed = 1.4f;
            pulse.amplitude = 0.8f;
        }
    }

    void SpawnToxicGlow()
    {
        // One sickly green hotspot somewhere in the maze for colour contrast.
        int x = Random.Range(1, mazeWidth - 1);
        int z = Random.Range(1, mazeHeight - 1);
        float cx = x * cellSize + cellSize / 2f;
        float cz = z * cellSize + cellSize / 2f;

        // Bright quad on the floor
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "ToxicSpill";
        q.transform.parent = mazeParent.transform;
        q.transform.position = new Vector3(cx, 0.018f, cz);
        q.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
        q.transform.localScale = Vector3.one * 1.6f;
        Destroy(q.GetComponent<Collider>());
        q.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.25f, 0.95f, 0.35f), 0.7f, true);

        var go = new GameObject("ToxicGlow");
        go.transform.parent = mazeParent.transform;
        go.transform.position = new Vector3(cx, 0.6f, cz);
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.3f, 1f, 0.35f);
        l.range = 5.5f;
        l.intensity = 1.2f;
        var pulse = go.AddComponent<PulsatingLight>();
        pulse.speed = 1.1f;
        pulse.amplitude = 0.35f;
    }

    // ─── CREATIVE HORROR SET-PIECES ───────────────────────────────────────────

    void SpawnMannequins()
    {
        // Dark humanoid figures standing perfectly still in corridors.
        int count = Mathf.Clamp(mazeWidth / 4, 3, 7);
        var bodyMat = ThemeSystem.MakeMaterial(new Color(0.04f, 0.03f, 0.03f), 0.5f);
        var headMat = ThemeSystem.MakeMaterial(new Color(0.06f, 0.05f, 0.05f), 0.2f);

        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);
            if ((x + z) < 4) continue; // not near the spawn

            float cx = x * cellSize + cellSize / 2f + Random.Range(-0.8f, 0.8f);
            float cz = z * cellSize + cellSize / 2f + Random.Range(-0.8f, 0.8f);
            float yaw = Random.Range(0f, 360f);

            var root = new GameObject("Mannequin");
            root.transform.parent = mazeParent.transform;
            root.transform.position = new Vector3(cx, 0f, cz);
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Torso (elongated capsule)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            torso.transform.localScale = new Vector3(0.28f, 0.55f, 0.20f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().material = bodyMat;

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            head.transform.localScale = Vector3.one * 0.24f;
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = headMat;

            // Arms (thin cubes)
            for (int s = -1; s <= 1; s += 2)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arm.name = "Arm";
                arm.transform.SetParent(root.transform, false);
                arm.transform.localPosition = new Vector3(s * 0.22f, 1.05f, 0f);
                arm.transform.localScale = new Vector3(0.09f, 0.85f, 0.09f);
                Destroy(arm.GetComponent<Collider>());
                arm.GetComponent<Renderer>().material = bodyMat;
            }

            // Faint blue-white pinpoint where the eyes would be — draws your gaze.
            var eyeGlow = new GameObject("EyeGlow");
            eyeGlow.transform.SetParent(head.transform, false);
            eyeGlow.transform.localPosition = new Vector3(0f, 0.05f, 0.4f);
            var l = eyeGlow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.7f, 0.85f, 1f);
            l.range = 0.6f;
            l.intensity = 1.2f;
        }
    }

    void SpawnWallEyes()
    {
        // 1-2 big painted eyes on walls whose pupils track the player.
        int count = Mathf.Clamp(mazeWidth / 6, 1, 3);
        int placed = 0, attempts = 0;
        while (placed < count && attempts < 40)
        {
            attempts++;
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;
            PlaceWallEye(x, z, dir);
            placed++;
        }
    }

    void PlaceWallEye(int x, int z, int direction)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = 1.6f;
        float inset = wallThickness * 0.5f + 0.03f;

        Vector3 pos; Quaternion rot;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX, y, z * cellSize + cellSize - inset); rot = Quaternion.Euler(0f, 180f, 0f); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ); rot = Quaternion.Euler(0f, -90f, 0f); break;
            case 2: pos = new Vector3(cellX, y, z * cellSize + inset); rot = Quaternion.Euler(0f, 0f, 0f); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ); rot = Quaternion.Euler(0f, 90f, 0f); break;
        }

        var root = new GameObject("WallEye");
        root.transform.parent = mazeParent.transform;
        root.transform.position = pos;
        root.transform.rotation = rot;

        // Sclera (yellowish white disc)
        var sclera = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sclera.name = "Sclera";
        sclera.transform.SetParent(root.transform, false);
        sclera.transform.localScale = Vector3.one * 1.3f;
        Destroy(sclera.GetComponent<Collider>());
        sclera.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.9f, 0.85f, 0.65f), 0.4f, true);

        // Iris
        var iris = GameObject.CreatePrimitive(PrimitiveType.Quad);
        iris.name = "Iris";
        iris.transform.SetParent(root.transform, false);
        iris.transform.localPosition = new Vector3(0f, 0f, -0.005f);
        iris.transform.localScale = Vector3.one * 0.65f;
        Destroy(iris.GetComponent<Collider>());
        iris.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.12f, 0.02f, 0.02f), 0.6f);

        // Pupil — child that EyeTracker moves.
        var pupil = GameObject.CreatePrimitive(PrimitiveType.Quad);
        pupil.name = "Pupil";
        pupil.transform.SetParent(root.transform, false);
        pupil.transform.localPosition = new Vector3(0f, 0f, -0.012f);
        pupil.transform.localScale = Vector3.one * 0.30f;
        Destroy(pupil.GetComponent<Collider>());
        pupil.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(Color.black, 0.0f);

        var tracker = root.AddComponent<EyeTracker>();
        tracker.pupil = pupil.transform;

        // Creepy cold-red rim light.
        var rim = new GameObject("EyeRim");
        rim.transform.SetParent(root.transform, false);
        rim.transform.localPosition = new Vector3(0f, 0f, -0.3f);
        var rl = rim.AddComponent<Light>();
        rl.type = LightType.Point;
        rl.color = new Color(0.95f, 0.15f, 0.18f);
        rl.range = 2.8f;
        rl.intensity = 0.7f;
    }

    void SpawnMeatHooks()
    {
        // Ceiling hooks with chunks of meat swinging in dead corridors.
        int count = Mathf.Clamp(mazeWidth / 4, 2, 5);
        var chainMat = ThemeSystem.MakeMaterial(new Color(0.07f, 0.06f, 0.05f), 0.85f);
        var hookMat = ThemeSystem.MakeMaterial(new Color(0.20f, 0.18f, 0.15f), 0.7f);
        var meatMat = ThemeSystem.MakeMaterial(new Color(0.40f, 0.04f, 0.05f), 0.65f, true);

        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int z = Random.Range(1, mazeHeight - 1);
            float cx = x * cellSize + cellSize / 2f + Random.Range(-0.7f, 0.7f);
            float cz = z * cellSize + cellSize / 2f + Random.Range(-0.7f, 0.7f);

            var root = new GameObject("MeatHook");
            root.transform.parent = mazeParent.transform;
            root.transform.position = new Vector3(cx, wallHeight, cz);

            var chain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chain.transform.SetParent(root.transform, false);
            chain.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            chain.transform.localScale = new Vector3(0.04f, 1.2f, 0.04f);
            Destroy(chain.GetComponent<Collider>());
            chain.GetComponent<Renderer>().material = chainMat;

            // Hook: small cube + angled cube
            var hookBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hookBase.transform.SetParent(root.transform, false);
            hookBase.transform.localPosition = new Vector3(0f, -1.25f, 0f);
            hookBase.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            Destroy(hookBase.GetComponent<Collider>());
            hookBase.GetComponent<Renderer>().material = hookMat;

            var hookCurve = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hookCurve.transform.SetParent(root.transform, false);
            hookCurve.transform.localPosition = new Vector3(0.06f, -1.38f, 0f);
            hookCurve.transform.localEulerAngles = new Vector3(0f, 0f, 30f);
            hookCurve.transform.localScale = new Vector3(0.05f, 0.22f, 0.05f);
            Destroy(hookCurve.GetComponent<Collider>());
            hookCurve.GetComponent<Renderer>().material = hookMat;

            var meat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meat.transform.SetParent(root.transform, false);
            meat.transform.localPosition = new Vector3(0.06f, -1.60f, 0f);
            meat.transform.localScale = new Vector3(0.35f, 0.35f, 0.28f);
            Destroy(meat.GetComponent<Collider>());
            meat.GetComponent<Renderer>().material = meatMat;

            root.AddComponent<SwayAnimator>();
        }
    }

    void SpawnDrippingBlood()
    {
        // Animated blood drips on walls that grow and shrink.
        int count = Mathf.Clamp(mazeWidth / 3, 3, 6);
        int placed = 0, attempts = 0;
        while (placed < count && attempts < 60)
        {
            attempts++;
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;
            PlaceDrippingBlood(x, z, dir);
            placed++;
        }
    }

    void PlaceDrippingBlood(int x, int z, int direction)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float y = wallHeight - 0.3f; // starts near the top of the wall
        float inset = wallThickness * 0.5f + 0.025f;

        Vector3 pos; Quaternion rot;
        switch (direction)
        {
            case 0: pos = new Vector3(cellX + Random.Range(-1f, 1f), y, z * cellSize + cellSize - inset); rot = Quaternion.Euler(0f, 180f, 0f); break;
            case 1: pos = new Vector3(x * cellSize + cellSize - inset, y, cellZ + Random.Range(-1f, 1f)); rot = Quaternion.Euler(0f, -90f, 0f); break;
            case 2: pos = new Vector3(cellX + Random.Range(-1f, 1f), y, z * cellSize + inset); rot = Quaternion.Euler(0f, 0f, 0f); break;
            default: pos = new Vector3(x * cellSize + inset, y, cellZ + Random.Range(-1f, 1f)); rot = Quaternion.Euler(0f, 90f, 0f); break;
        }

        var root = new GameObject("BloodDrip");
        root.transform.parent = mazeParent.transform;
        root.transform.position = pos;
        root.transform.rotation = rot;

        var streak = GameObject.CreatePrimitive(PrimitiveType.Quad);
        streak.name = "Streak";
        streak.transform.SetParent(root.transform, false);
        streak.transform.localScale = new Vector3(0.07f, 0.2f, 1f);
        streak.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        Destroy(streak.GetComponent<Collider>());
        streak.GetComponent<Renderer>().material =
            ThemeSystem.MakeMaterial(new Color(0.35f, 0.02f, 0.02f), 0.7f, true);

        var drip = root.AddComponent<DrippingBlood>();
        drip.streak = streak.transform;
        drip.maxLength = Random.Range(0.7f, 1.3f);
        drip.growSpeed = Random.Range(0.08f, 0.18f);
    }

    void SpawnFloorHearts()
    {
        // Fleshy pulsing "hearts" on the floor — bright red, scale-pulse.
        int count = Mathf.Clamp(mazeWidth / 5, 1, 3);
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(2, mazeWidth - 1);
            int z = Random.Range(2, mazeHeight - 1);
            float cx = x * cellSize + cellSize / 2f;
            float cz = z * cellSize + cellSize / 2f;

            var heart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            heart.name = "Heart";
            heart.transform.parent = mazeParent.transform;
            heart.transform.position = new Vector3(cx, 0.25f, cz);
            heart.transform.localScale = new Vector3(0.55f, 0.35f, 0.55f);
            Destroy(heart.GetComponent<Collider>());
            heart.GetComponent<Renderer>().material =
                ThemeSystem.MakeMaterial(new Color(0.55f, 0.03f, 0.03f), 0.8f, true);

            var pulse = heart.AddComponent<PulseScale>();
            pulse.speed = 1.2f;
            pulse.amount = 0.25f;

            var lightGo = new GameObject("HeartGlow");
            lightGo.transform.SetParent(heart.transform, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.10f, 0.10f);
            l.range = 3f;
            l.intensity = 1.6f;
            lightGo.AddComponent<PulsatingLight>();
        }
    }

    void SpawnHandprintTrails()
    {
        // Clusters of small dark handprints, dragged across walls like someone
        // was being pulled backward.
        int trailCount = Mathf.Clamp(mazeWidth / 4, 2, 4);
        var printMat = ThemeSystem.MakeMaterial(new Color(0.18f, 0.02f, 0.02f), 0.1f);

        int placed = 0, attempts = 0;
        while (placed < trailCount && attempts < 40)
        {
            attempts++;
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);
            int dir = Random.Range(0, 4);
            if (!walls[x, z, dir]) continue;
            if (dir == 2 && z != 0) continue;
            if (dir == 3 && x != 0) continue;

            PlaceHandprintTrail(x, z, dir, printMat);
            placed++;
        }
    }

    void PlaceHandprintTrail(int x, int z, int direction, Material mat)
    {
        float cellX = x * cellSize + cellSize / 2f;
        float cellZ = z * cellSize + cellSize / 2f;
        float inset = wallThickness * 0.5f + 0.02f;

        Vector3 anchor; Quaternion rot; Vector3 along;
        switch (direction)
        {
            case 0: anchor = new Vector3(cellX, 0f, z * cellSize + cellSize - inset); rot = Quaternion.Euler(0f, 180f, 0f); along = Vector3.right; break;
            case 1: anchor = new Vector3(x * cellSize + cellSize - inset, 0f, cellZ); rot = Quaternion.Euler(0f, -90f, 0f); along = Vector3.forward; break;
            case 2: anchor = new Vector3(cellX, 0f, z * cellSize + inset); rot = Quaternion.Euler(0f, 0f, 0f); along = Vector3.right; break;
            default: anchor = new Vector3(x * cellSize + inset, 0f, cellZ); rot = Quaternion.Euler(0f, 90f, 0f); along = Vector3.forward; break;
        }

        int handCount = Random.Range(3, 6);
        float startY = Random.Range(1.0f, 1.8f);
        float startOff = Random.Range(-1.2f, 1.2f);
        float spacing = 0.35f * (Random.value < 0.5f ? 1f : -1f);

        for (int i = 0; i < handCount; i++)
        {
            var hand = GameObject.CreatePrimitive(PrimitiveType.Quad);
            hand.name = "Handprint";
            hand.transform.parent = mazeParent.transform;
            hand.transform.position = anchor + Vector3.up * (startY - i * 0.08f) +
                                      along * (startOff + i * spacing);
            hand.transform.rotation = rot;
            hand.transform.Rotate(0f, 0f, Random.Range(-20f, 20f));
            hand.transform.localScale = Vector3.one * Random.Range(0.22f, 0.32f);
            Destroy(hand.GetComponent<Collider>());
            hand.GetComponent<Renderer>().material = mat;
        }
    }

    void SpawnPickup(Vector2Int cell, PickupType type)
    {
        var go = GameObject.CreatePrimitive(type == PickupType.Page ? PrimitiveType.Cube : PrimitiveType.Cylinder);
        go.name = type == PickupType.Page ? "Page" : "Battery";
        go.transform.parent = mazeParent.transform;
        go.transform.position = CellToWorld(cell, 1f);
        go.transform.localScale = type == PickupType.Page
            ? new Vector3(0.7f, 0.05f, 0.55f)
            : new Vector3(0.4f, 0.4f, 0.4f);

        // Original primitive collider stays — but the player needs a generous
        // trigger zone to pick things up while walking past, so add a sphere.
        var existing = go.GetComponent<Collider>();
        if (existing != null) Object.Destroy(existing);
        var sphere = go.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.0f;

        Color tint = type == PickupType.Page
            ? new Color(0.95f, 0.92f, 0.78f)
            : new Color(0.45f, 1f, 0.55f);
        var mat = ThemeSystem.MakeMaterial(tint, 0.5f, true);
        go.GetComponent<Renderer>().material = mat;

        // Pulsing point light.
        var lightGo = new GameObject("Glow");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = Vector3.up * 0.3f;
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = tint;
        l.range = 3.5f;
        l.intensity = 1.3f;

        var p = go.AddComponent<Pickup>();
        p.type = type;
    }
}
