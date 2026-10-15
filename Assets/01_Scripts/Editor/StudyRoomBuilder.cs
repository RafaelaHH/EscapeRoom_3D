using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Arma "El Estudio del Coleccionista" dentro de MainScene: el cuarto, los muebles (Poly Haven y Sketchfab),
// las luces y el acertijo: pistas → candado (código de PuzzleManager) → cofre → llave → puerta.
// Menú: Escape Room > Construir el Estudio del Coleccionista.
// Se puede ejecutar las veces que haga falta: borra lo generado y lo vuelve a armar, así que
// no conviene acomodar cosas a mano dentro de "Estudio", "Chest" o "Door" (se pierden al reconstruir).
public static class StudyRoomBuilder
{
    private const string ScenePath = "Assets/00_Scenes/MainScene.unity";
    private const string PlayerPrefabPath = "Assets/02_Prefabs/Player.prefab";
    private const string PolyHaven = "Assets/04_Models/PolyHaven/";
    private const string Sketchfab = "Assets/04_Models/Sketchfab/";
    private const string Textures = "Assets/05_Textures/PolyHaven/";
    private const string MaterialsFolder = "Assets/03_Materials/Estudio";
    private const string GeneratedFolder = "Assets/04_Models/Generados";

    // Interior del cuarto, en metros
    private const float HalfX = 3.5f;
    private const float HalfZ = 3f;
    private const float Height = 3.2f;
    private const float WallThickness = 0.2f;
    private const float PanelThickness = 0.02f; // revestimiento de madera de la parte baja
    private const float DoorOpening = 1.96f;
    private const float DoorOpeningHeight = 2.92f;

    private static Material floorMat, wallpaperMat, panelMat, trimMat, ceilingMat, rugMat, rugBorderMat;
    private static Material brassMat, steelMat, ironMat, paperMat, redMat, velvetMat, plasterMat, daylightMat, glassMat, flameMat;

    [MenuItem("Escape Room/Construir el Estudio del Coleccionista")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PolyHaven + "treasure_chest/treasure_chest_1k.gltf") == null)
        {
            Debug.LogError("No se pueden cargar los modelos .gltf. Revisa que el paquete glTFast (com.unity.cloud.gltfast) " +
                           "esté instalado y que Unity haya terminado de importar Assets/04_Models.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureFolder(MaterialsFolder);
        EnsureFolder(GeneratedFolder);

        RemoveOldRoom(scene);
        CreateMaterials();

        Transform room = new GameObject("Estudio").transform;
        BuildArchitecture(Group("Arquitectura", room));
        Transform lights = Group("Luces", room);
        BuildFurniture(Group("Muebles", room), Group("Pistas", room), lights);

        GameObject player = SetupPlayer();
        GameHUD hud = SetupHud(player);
        BuildPuzzle(player, hud, lights);
        FixGlassAndFlames(room.gameObject);
        SetupEnvironment(room);

        AddSceneToBuildSettings(); // "Volver a jugar" carga la escena por nombre
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Estudio del Coleccionista construido. Dale Play para probarlo.");
    }

    // ------------------------------------------------------------------ limpieza

    private static void RemoveOldRoom(Scene scene)
    {
        // Lo generado antes y los cubos grises del prototipo
        string[] names = { "Estudio", "HUD", "Floor", "Key", "Directional Light" };
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (names.Contains(go.name))
                Object.DestroyImmediate(go);
        }
    }

    // Quita la malla gris y los hijos de un objeto que se reutiliza (Chest, Door)
    private static void StripVisual(GameObject go)
    {
        foreach (Component c in go.GetComponents<Component>())
        {
            if (c is MeshRenderer || c is MeshFilter || c is Collider || c is HingeAnimator || c is InteractionPrompt)
                Object.DestroyImmediate(c);
        }
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(go.transform.GetChild(i).gameObject);
        go.transform.localScale = Vector3.one;
    }

    // ------------------------------------------------------------------ materiales

    private static void CreateMaterials()
    {
        floorMat = TexturedMaterial("Piso_Parquet", "herringbone_parquet", new Color(0.66f, 0.53f, 0.42f), 0.45f);
        wallpaperMat = TexturedMaterial("Pared_Damasco", "quatrefoil_jacquard_fabric", new Color(0.78f, 0.74f, 0.74f), 0.12f);
        panelMat = TexturedMaterial("Pared_Paneles", "dark_paneled_wood", Color.white, 0.35f);
        trimMat = TexturedMaterial("Molduras_Nogal", "american_walnut_veneer", new Color(0.55f, 0.4f, 0.3f), 0.5f);
        ceilingMat = TexturedMaterial("Techo_Yeso", "ceiling_interior", new Color(0.82f, 0.77f, 0.68f), 0.08f);
        rugMat = TexturedMaterial("Alfombra", "floral_jacquard", new Color(1f, 0.8f, 0.52f), 0.05f);
        plasterMat = TexturedMaterial("Pasillo_Yeso", "ceiling_interior", new Color(0.95f, 0.93f, 0.88f), 0.1f);

        rugBorderMat = ColorMaterial("Alfombra_Borde", new Color(0.22f, 0.06f, 0.05f), 0.05f, 0f);
        brassMat = ColorMaterial("Laton", new Color(0.83f, 0.63f, 0.32f), 0.7f, 1f);
        steelMat = ColorMaterial("Acero", new Color(0.62f, 0.62f, 0.6f), 0.65f, 1f);
        ironMat = ColorMaterial("Hierro_Oscuro", new Color(0.08f, 0.07f, 0.06f), 0.35f, 0.7f);
        paperMat = ColorMaterial("Papel_Viejo", new Color(0.9f, 0.84f, 0.7f), 0.1f, 0f);
        redMat = ColorMaterial("Rojo", new Color(0.72f, 0.06f, 0.04f), 0.6f, 0f);
        velvetMat = ColorMaterial("Terciopelo", new Color(0.35f, 0.03f, 0.05f), 0.15f, 0f);

        daylightMat = EmissiveMaterial("Luz_Exterior", Color.white, new Color(1f, 0.96f, 0.88f) * 2.2f);
        flameMat = EmissiveMaterial("Llama", new Color(1f, 0.7f, 0.3f), new Color(1f, 0.62f, 0.25f) * 3f);

        // Vidrio transparente (modo "Transparent" del shader Standard)
        glassMat = ColorMaterial("Vidrio", new Color(0.8f, 0.85f, 0.9f, 0.12f), 0.95f, 0f);
        glassMat.SetFloat("_Mode", 3f);
        glassMat.SetInt("_SrcBlend", (int)BlendMode.One);
        glassMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        glassMat.SetInt("_ZWrite", 0);
        glassMat.DisableKeyword("_ALPHATEST_ON");
        glassMat.DisableKeyword("_ALPHABLEND_ON");
        glassMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        glassMat.renderQueue = (int)RenderQueue.Transparent;
    }

    private static Material EmissiveMaterial(string name, Color color, Color emission)
    {
        Material material = ColorMaterial(name, color, 0f, 0f);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        return material;
    }

    private static Material GetMaterial(string name)
    {
        string path = MaterialsFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    private static Material ColorMaterial(string name, Color color, float smoothness, float metallic)
    {
        Material material = GetMaterial(name);
        material.color = color;
        material.SetFloat("_Glossiness", smoothness);
        material.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Material con las texturas de Poly Haven (color + relieve)
    private static Material TexturedMaterial(string name, string texture, Color tint, float smoothness)
    {
        Material material = ColorMaterial(name, tint, smoothness, 0f);
        material.mainTexture = LoadTexture(texture, "diff", false);
        Texture2D normal = LoadTexture(texture, "nor_gl", true);
        if (normal != null)
        {
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D LoadTexture(string id, string map, bool isNormalMap)
    {
        string path = Textures + id + "/" + id + "_" + map + "_1k.jpg";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Falta la textura " + path);
            return null;
        }
        if (isNormalMap && importer.textureType != TextureImporterType.NormalMap)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ------------------------------------------------------------------ cuarto

    private static void BuildArchitecture(Transform parent)
    {
        float t = WallThickness;
        float width = 2f * HalfX + 2f * t;
        float depth = 2f * HalfZ + 2f * t;

        Box("Piso", parent, new Vector3(0f, -0.1f, 0f), new Vector3(width, 0.2f, depth), floorMat, 1.6f);
        Box("Techo", parent, new Vector3(0f, Height + 0.1f, 0f), new Vector3(width, 0.2f, depth), ceilingMat, 2f);

        Box("Pared sur", parent, new Vector3(0f, Height / 2f, -HalfZ - t / 2f), new Vector3(width, Height, t), wallpaperMat, 0.7f);
        Box("Pared este", parent, new Vector3(HalfX + t / 2f, Height / 2f, 0f), new Vector3(t, Height, 2f * HalfZ), wallpaperMat, 0.7f);
        Box("Pared oeste", parent, new Vector3(-HalfX - t / 2f, Height / 2f, 0f), new Vector3(t, Height, 2f * HalfZ), wallpaperMat, 0.7f);

        // Pared norte, con el hueco de la puerta
        float side = HalfX + t - DoorOpening / 2f;
        Box("Pared norte izquierda", parent, new Vector3(-(DoorOpening / 2f + side / 2f), Height / 2f, HalfZ + t / 2f), new Vector3(side, Height, t), wallpaperMat, 0.7f);
        Box("Pared norte derecha", parent, new Vector3(DoorOpening / 2f + side / 2f, Height / 2f, HalfZ + t / 2f), new Vector3(side, Height, t), wallpaperMat, 0.7f);
        Box("Dintel", parent, new Vector3(0f, (DoorOpeningHeight + Height) / 2f, HalfZ + t / 2f), new Vector3(DoorOpening, Height - DoorOpeningHeight, t), wallpaperMat, 0.7f);

        // Revestimiento de madera, zócalo, moldura a media altura y cornisa
        float doorEdge = DoorOpening / 2f + 0.03f;
        var runs = new List<(string name, Vector3 a, Vector3 b, Vector3 inward)>
        {
            ("sur", new Vector3(-HalfX, 0f, -HalfZ), new Vector3(HalfX, 0f, -HalfZ), Vector3.forward),
            ("este", new Vector3(HalfX, 0f, -HalfZ), new Vector3(HalfX, 0f, HalfZ), Vector3.left),
            ("oeste", new Vector3(-HalfX, 0f, -HalfZ), new Vector3(-HalfX, 0f, HalfZ), Vector3.right),
            ("norte izq.", new Vector3(-HalfX, 0f, HalfZ), new Vector3(-doorEdge, 0f, HalfZ), Vector3.back),
            ("norte der.", new Vector3(doorEdge, 0f, HalfZ), new Vector3(HalfX, 0f, HalfZ), Vector3.back),
        };
        foreach (var run in runs)
        {
            Strip("Paneles " + run.name, parent, run.a, run.b, run.inward, 0f, 1f, PanelThickness, panelMat, 1.1f);
            Strip("Zócalo " + run.name, parent, run.a, run.b, run.inward, 0f, 0.14f, 0.035f, trimMat, 1f);
            Strip("Moldura " + run.name, parent, run.a, run.b, run.inward, 0.97f, 1.03f, 0.04f, trimMat, 1f);
            Strip("Cornisa " + run.name, parent, run.a, run.b, run.inward, Height - 0.12f, Height, 0.06f, trimMat, 1f);
        }

        // Vigas de madera en el techo
        Box("Viga oeste", parent, new Vector3(-1.75f, Height - 0.11f, 0f), new Vector3(0.18f, 0.22f, 2f * HalfZ), trimMat, 1f, false);
        Box("Viga este", parent, new Vector3(1.75f, Height - 0.11f, 0f), new Vector3(0.18f, 0.22f, 2f * HalfZ), trimMat, 1f, false);

        // Pasillo detrás de la puerta: se ve iluminado cuando la puerta se abre
        Transform hall = Group("Pasillo de salida", parent);
        float hallStart = HalfZ + t;
        float hallDepth = 2.6f;
        float hallZ = hallStart + hallDepth / 2f;
        Box("Piso pasillo", hall, new Vector3(0f, -0.1f, hallZ), new Vector3(2.8f, 0.2f, hallDepth), floorMat, 1.6f);
        Box("Techo pasillo", hall, new Vector3(0f, 3.15f, hallZ), new Vector3(2.8f, 0.2f, hallDepth), plasterMat, 2f);
        Box("Pared pasillo oeste", hall, new Vector3(-1.4f, 1.5f, hallZ), new Vector3(0.2f, 3.2f, hallDepth), plasterMat, 1.5f);
        Box("Pared pasillo este", hall, new Vector3(1.4f, 1.5f, hallZ), new Vector3(0.2f, 3.2f, hallDepth), plasterMat, 1.5f);
        Box("Luz del exterior", hall, new Vector3(0f, 1.5f, hallStart + hallDepth + 0.05f), new Vector3(2.8f, 3.2f, 0.1f), daylightMat, 3f);

        MarkStatic(parent.gameObject);
    }

    // Tira horizontal pegada a una pared (paneles, zócalo, molduras)
    private static void Strip(string name, Transform parent, Vector3 a, Vector3 b, Vector3 inward, float y0, float y1, float thickness, Material material, float tile)
    {
        Vector3 center = (a + b) / 2f + inward * thickness / 2f + Vector3.up * (y0 + y1) / 2f;
        float length = Vector3.Distance(a, b);
        Vector3 size = Mathf.Abs(inward.z) > 0.5f
            ? new Vector3(length, y1 - y0, thickness)
            : new Vector3(thickness, y1 - y0, length);
        Box(name, parent, center, size, material, tile, false);
    }

    // ------------------------------------------------------------------ muebles y pistas

    private static void BuildFurniture(Transform parent, Transform clues, Transform lights)
    {
        // Pared norte: armario gótico, (puerta en el centro), reloj de pie y maletas
        GameObject cabinet = Place(PolyHaven + "GothicCabinet_01/GothicCabinet_01_1k.gltf", "Armario gotico", parent, -2.35f, 0f, 2.5f, 180f);
        CloseDoors(cabinet);
        AgainstWall(cabinet, Vector3.forward);
        Solid(cabinet);
        Bounds cabinetBounds = Bounds(cabinet);
        Solid(Place(PolyHaven + "marble_bust_01/marble_bust_01_1k.gltf", "Busto", parent, cabinetBounds.center.x, cabinetBounds.max.y, cabinetBounds.center.z, 200f), false);
        GameObject vase = Place(PolyHaven + "brass_vase_01/brass_vase_01_1k.gltf", "Jarron de laton", parent, -1.27f, 0f, 2.7f, 0f);
        AgainstWall(vase, Vector3.forward, 0.06f);
        Solid(vase);

        GameObject clock = Place(PolyHaven + "vintage_grandfather_clock_01/vintage_grandfather_clock_01_1k.gltf", "Reloj de pie", parent, 2.1f, 0f, 2.6f, 180f);
        AgainstWall(clock, Vector3.forward);
        SetClockHands(clock, 1, 0);
        Clue(clock, clues, "Pista - Reloj", "Examinar el reloj de pie",
            "El reloj de pie está detenido. Sus agujas marcan la 1 en punto.");

        GameObject suitcases = Place(PolyHaven + "vintage_suitcase/vintage_suitcase_1k.gltf", "Maletas", parent, 3.2f, 0f, 2.12f, -90f);
        AgainstWall(suitcases, Vector3.right);
        Solid(suitcases);

        // Pared este: librero (pista) y globo terráqueo (pista)
        GameObject shelf = Place(PolyHaven + "wooden_bookshelf_worn/wooden_bookshelf_worn_1k.gltf", "Librero", parent, 3.2f, 0f, 0.55f, -90f);
        AgainstWall(shelf, Vector3.right);
        GameObject shelfClue = Clue(shelf, clues, "Pista - Librero", "Examinar el librero",
            "En el librero, 4 libros sobresalen con el lomo pintado de rojo.");
        FillBookshelf(shelf, shelfClue.transform);
        Solid(shelfClue);

        GameObject globe = Place(Sketchfab + "globo_terraqueo/scene.gltf", "Globo terraqueo", parent, 2.55f, 0f, -1.3f, -120f, 1.15f);
        AddGlobePins(globe, 3);
        Clue(globe, clues, "Pista - Globo", "Examinar el globo terráqueo",
            "Hay 3 alfileres rojos clavados en el globo terráqueo.");

        // Pared sur: consola con la nota, lámpara de aceite y espejo
        GameObject console = Place(PolyHaven + "ClassicConsole_01/ClassicConsole_01_1k.gltf", "Consola", parent, 1.6f, 0f, -2.7f, 0f);
        AgainstWall(console, Vector3.back);
        Solid(console);
        Bounds consoleBounds = Bounds(console);
        float consoleTop = consoleBounds.max.y;
        float consoleZ = consoleBounds.center.z;

        GameObject lamp = Place(PolyHaven + "vintage_oil_lamp/vintage_oil_lamp_1k.gltf", "Lampara de aceite", parent, consoleBounds.max.x - 0.25f, consoleTop, consoleZ, 0f);
        NoShadows(lamp);
        Transform flame = FindChild(lamp.transform, "vintage_oil_lamp_flame");
        Vector3 lampLight = flame != null ? Bounds(flame.gameObject).center : Bounds(lamp).center + Vector3.up * 0.2f;
        PointLight("Luz lampara de aceite", lights, lampLight + Vector3.up * 0.02f, new Color(1f, 0.64f, 0.34f), 1.1f, 4f, LightShadows.Soft, true);

        Solid(Place(PolyHaven + "seadogs_compass/seadogs_compass_1k.gltf", "Brujula", parent, consoleBounds.min.x + 0.25f, consoleTop, consoleZ + 0.04f, 35f), false);
        CreateNote(clues, new Vector3(consoleBounds.center.x - 0.12f, consoleTop, consoleZ + 0.03f));

        GameObject mirror = Place(PolyHaven + "ornate_mirror_01/ornate_mirror_01_1k.gltf", "Espejo", parent, consoleBounds.center.x, 1.4f, -2.9f, 0f);
        AgainstWall(mirror, Vector3.back, 0.005f);
        Solid(mirror, false);

        // Rincón suroeste: sillón y mesita con candelabro
        Solid(Place(PolyHaven + "ArmChair_01/ArmChair_01_1k.gltf", "Sillon", parent, -2.35f, 0f, -2.0f, 35f));
        GameObject table = Place(PolyHaven + "side_table_01/side_table_01_1k.gltf", "Mesita", parent, -3.1f, 0f, -2.65f, 0f);
        AgainstWall(table, Vector3.left);
        AgainstWall(table, Vector3.back);
        Solid(table);
        Bounds tableBounds = Bounds(table);
        GameObject candelabra = Place(PolyHaven + "brass_candleholders/brass_candleholders_1k.gltf", "Candelabro", parent, tableBounds.center.x, tableBounds.max.y, tableBounds.center.z, 30f);
        KeepOnly(candelabra, "brass_candleholder_03");
        Recenter(candelabra, tableBounds.center.x, tableBounds.max.y, tableBounds.center.z);
        NoShadows(candelabra);
        Solid(candelabra, false);
        Bounds candelabraBounds = Bounds(candelabra);
        PointLight("Luz candelabro", lights, new Vector3(candelabraBounds.center.x, candelabraBounds.max.y + 0.03f, candelabraBounds.center.z),
            new Color(1f, 0.6f, 0.3f), 1.3f, 5f, LightShadows.Soft, true);

        // Pared oeste: cómoda con adornos y el cuadro (pista)
        GameObject commode = Place(PolyHaven + "GothicCommode_01/GothicCommode_01_1k.gltf", "Comoda", parent, -3.2f, 0f, 0.35f, 90f);
        AgainstWall(commode, Vector3.left);
        Solid(commode);
        Bounds commodeBounds = Bounds(commode);
        float commodeTop = commodeBounds.max.y;
        float commodeX = commodeBounds.center.x;
        Solid(Place(PolyHaven + "bronze_whale_statue/bronze_whale_statue_1k.gltf", "Ballena de bronce", parent, commodeX, commodeTop, commodeBounds.min.z + 0.3f, 70f, 0.3f), false);
        GameObject candle = Place(PolyHaven + "wooden_candlestick/wooden_candlestick_1k.gltf", "Vela", parent, commodeX, commodeTop, commodeBounds.max.z - 0.12f, 0f);
        NoShadows(candle);
        Solid(candle, false);
        PointLight("Luz vela", lights, Bounds(candle).center + Vector3.up * 0.16f, new Color(1f, 0.6f, 0.3f), 0.7f, 3f, LightShadows.None, true);

        GameObject painting = Place(Sketchfab + "cuadro_oleo/scene.gltf", "Cuadro victoriano", parent, -3.4f, 1.62f, commodeBounds.center.z, 90f);
        FaceDirection(painting, "Painting_Victorian Paintings_0", Vector3.right);
        AgainstWall(painting, Vector3.left, 0.005f);
        Clue(painting, clues, "Pista - Cuadro", "Examinar el cuadro",
            "En una esquina del cuadro, junto a la firma del pintor, hay un 2 pintado en dorado.");

        // Centro: alfombra y lámpara de techo
        Box("Alfombra", parent, new Vector3(-0.7f, 0.006f, 0.45f), new Vector3(3.0f, 0.012f, 2.1f), rugMat, 0.9f, false);
        Box("Borde alfombra", parent, new Vector3(-0.7f, 0.004f, 0.45f), new Vector3(3.2f, 0.008f, 2.3f), rugBorderMat, 1f, false);

        GameObject chandelier = Place(PolyHaven + "Chandelier_02/Chandelier_02_1k.gltf", "Lampara de techo", parent, 0f, 0f, 0.3f, 0f);
        chandelier.transform.position += Vector3.up * (Height - Bounds(chandelier).max.y);
        NoShadows(chandelier);
        Solid(chandelier, false);
        Bounds chandelierBounds = Bounds(chandelier);
        PointLight("Luz lampara de techo", lights, new Vector3(chandelierBounds.center.x, chandelierBounds.min.y + 0.12f, chandelierBounds.center.z),
            new Color(1f, 0.78f, 0.52f), 1.7f, 9f, LightShadows.Soft, false);

        // Luz suave sobre la puerta, para que se vea desde el inicio
        PointLight("Luz puerta", lights, new Vector3(0f, 2.7f, HalfZ - 0.9f), new Color(1f, 0.74f, 0.48f), 0.6f, 3.5f, LightShadows.None, false);
    }

    private static void FillBookshelf(GameObject shelf, Transform parent)
    {
        Bounds b = Bounds(shelf);
        float frontX = b.min.x;    // el librero mira hacia el oeste (-x)
        float bottom = b.min.y;
        float z = b.center.z;
        string books = PolyHaven + "book_encyclopedia_set_01/book_encyclopedia_set_01_1k.gltf";

        // Alturas de las tablas medidas en el modelo de Poly Haven
        GameObject marked = BookSet(books, "Libros marcados", parent, frontX, bottom + 1.255f, z - 0.33f);
        BookSet(books, "Libros", parent, frontX, bottom + 1.255f, z + 0.33f);
        BookSet(books, "Libros", parent, frontX, bottom + 0.955f, z + 0.3f);
        BookSet(books, "Libros", parent, frontX, bottom + 0.655f, z - 0.3f);
        BookSet(books, "Libros", parent, frontX, bottom + 1.655f, z + 0.25f);
        Place(PolyHaven + "carved_wooden_elephant/carved_wooden_elephant_1k.gltf", "Elefante tallado", parent, frontX + 0.2f, bottom + 0.955f, z - 0.35f, -60f, 0.16f);

        // Pista: 4 libros que sobresalen, con el lomo rojo
        Material redCover = MarkedBookMaterial(marked);
        foreach (string name in new[] { "book03", "book07", "book12", "book16" })
        {
            Transform book = FindChild(marked.transform, "book_encyclopedia_set_01_" + name);
            if (book == null)
                continue;
            book.localPosition += Vector3.forward * 0.04f; // hacia el cuarto
            Renderer renderer = book.GetComponentInChildren<Renderer>();
            if (renderer != null && redCover != null)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && materials[i].name.Contains("cover"))
                        materials[i] = redCover;
                }
                renderer.sharedMaterials = materials;
            }
        }
    }

    private static GameObject BookSet(string path, string name, Transform parent, float frontX, float y, float z)
    {
        GameObject set = Place(path, name, parent, frontX, y + 0.003f, z, -90f);
        Bounds b = Bounds(set);
        set.transform.position += Vector3.right * (frontX + 0.03f - b.min.x);
        return set;
    }

    private static Material MarkedBookMaterial(GameObject bookSet)
    {
        Material cover = bookSet.GetComponentsInChildren<Renderer>()
            .SelectMany(r => r.sharedMaterials)
            .FirstOrDefault(m => m != null && m.name.Contains("cover"));
        if (cover == null)
            return null;

        string path = MaterialsFolder + "/Libro_Marcado.mat";
        AssetDatabase.DeleteAsset(path);
        var red = new Material(cover) { name = "Libro_Marcado" };
        foreach (string property in new[] { "baseColorFactor", "_BaseColor", "_Color" })
        {
            if (red.HasProperty(property))
                red.SetColor(property, new Color(0.9f, 0.12f, 0.08f));
        }
        AssetDatabase.CreateAsset(red, path);
        return red;
    }

    // Tres alfileres rojos clavados en el globo, del lado que mira al cuarto
    private static void AddGlobePins(GameObject globe, int count)
    {
        Transform map = FindChild(globe.transform, "Map_1");
        MeshFilter mapMesh = map != null ? map.GetComponentInChildren<MeshFilter>() : null;
        if (mapMesh == null)
            return;

        Bounds mapBounds = Bounds(mapMesh.gameObject);
        Vector3 center = mapBounds.center;
        Vector3 toRoom = Vector3.ProjectOnPlane(new Vector3(0f, center.y, 0f) - center, Vector3.up).normalized;
        var collider = mapMesh.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mapMesh.sharedMesh;

        float[] yaw = { -28f, 6f, 30f };
        float[] pitch = { 18f, -8f, 28f };
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Quaternion.AngleAxis(yaw[i % 3], Vector3.up) *
                                Quaternion.AngleAxis(-pitch[i % 3], Vector3.Cross(Vector3.up, toRoom)) * toRoom;
            Vector3 surface = center + direction * mapBounds.extents.x;
            if (collider.Raycast(new Ray(center + direction * 3f, -direction), out RaycastHit hit, 6f))
                surface = hit.point;

            var pin = new GameObject("Alfiler " + (i + 1));
            pin.transform.SetParent(globe.transform, true);
            pin.transform.position = surface;
            pin.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            Part(PrimitiveType.Sphere, "Cabeza", pin.transform, Vector3.up * 0.03f, Vector3.one * 0.022f, Quaternion.identity, redMat);
            Part(PrimitiveType.Cylinder, "Aguja", pin.transform, Vector3.up * 0.012f, new Vector3(0.004f, 0.016f, 0.004f), Quaternion.identity, steelMat);
        }
        Object.DestroyImmediate(collider);
    }

    // Hoja de papel con un sello de lacre: la primera pista
    private static void CreateNote(Transform parent, Vector3 position)
    {
        var note = new GameObject("Pista - Nota del coleccionista");
        note.transform.SetParent(parent, false);
        note.transform.position = position + Vector3.up * 0.03f;
        note.transform.rotation = Quaternion.Euler(0f, 12f, 0f);
        Part(PrimitiveType.Cube, "Papel", note.transform, Vector3.down * 0.028f, new Vector3(0.21f, 0.003f, 0.29f), Quaternion.identity, paperMat);
        Part(PrimitiveType.Cylinder, "Sello de lacre", note.transform, new Vector3(0.05f, -0.024f, -0.1f), new Vector3(0.035f, 0.003f, 0.035f), Quaternion.identity, redMat);
        BoxCollider box = note.AddComponent<BoxCollider>();
        box.center = Vector3.down * 0.02f;
        box.size = new Vector3(0.25f, 0.04f, 0.32f);
        SetString(note.AddComponent<ClueObject>(), "clue",
            "«Mi cofre guarda la llave de esta puerta. Su candado se abre con mi colección, en este orden: " +
            "primero el TIEMPO, luego el ARTE, después el MUNDO y por último el SABER.» — El Coleccionista");
        SetString(note.AddComponent<InteractionPrompt>(), "text", "Leer la nota");
    }

    // ------------------------------------------------------------------ acertijo: cofre, candado, llave y puerta

    private static void BuildPuzzle(GameObject player, GameHUD hud, Transform lights)
    {
        Behaviour[] playerControls = PlayerControls(player);

        // Cofre (se reutiliza el objeto Chest que ya usan PuzzleManager y PuzzleTest)
        ChestController chest = Object.FindObjectOfType<ChestController>();
        if (chest == null)
            chest = new GameObject("Chest").AddComponent<ChestController>();
        GameObject chestRoot = chest.gameObject;
        StripVisual(chestRoot);
        chestRoot.name = "Chest";
        chestRoot.transform.SetParent(null);
        chestRoot.transform.SetPositionAndRotation(new Vector3(-0.95f, 0f, 0.95f), Quaternion.Euler(0f, 180f, 0f));
        SetBool(chest, "unlocked", false);
        SetString(chestRoot.AddComponent<InteractionPrompt>(), "text", "Abrir el cofre");

        GameObject chestModel = Model(PolyHaven + "treasure_chest/treasure_chest_1k.gltf", "Cofre del tesoro", chestRoot.transform);
        Transform latch = FindChild(chestModel.transform, "treasure_chest_lock");
        if (latch != null && LocalBounds(chestRoot.transform, latch.gameObject).center.z < 0f)
            chestModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // el cierre siempre al frente (+z local)
        Recenter(chestModel, chestRoot.transform.position.x, 0f, chestRoot.transform.position.z);
        FitCollider(chestRoot, chestModel);

        Transform lid = FindChild(chestModel.transform, "treasure_chest_lid");
        var lidHinge = chestRoot.AddComponent<HingeAnimator>();
        if (lid != null)
        {
            // Gira hacia atrás: el frente de la tapa sube
            float front = lid.InverseTransformPoint(Bounds(lid.gameObject).center).z;
            SetHinges(lidHinge, 1.3f, 0f, (lid, new Vector3(front >= 0f ? -105f : 105f, 0f, 0f)));
        }

        // Cojín de terciopelo y llave adentro del cofre
        Part(PrimitiveType.Cube, "Cojin", chestRoot.transform, new Vector3(0f, 0.27f, 0f), new Vector3(0.55f, 0.05f, 0.3f), Quaternion.identity, velvetMat);
        GameObject key = CreateKey(chestRoot.transform, new Vector3(0f, 0.305f, 0f));
        KeyPickup pickup = key.AddComponent<KeyPickup>();

        // Candado de combinación colgando del cierre del cofre
        Bounds latchBounds = latch != null ? LocalBounds(chestRoot.transform, latch.gameObject) : new Bounds(new Vector3(0f, 0.34f, 0.26f), Vector3.zero);
        GameObject codeLock = CreatePadlock(chestRoot.transform, new Vector3(0f, latchBounds.min.y - 0.05f, latchBounds.max.z + 0.025f), out Transform padlockVisual);
        CodeLock lockLogic = codeLock.AddComponent<CodeLock>();
        SetObject(lockLogic, "puzzleManager", Object.FindObjectOfType<PuzzleManager>());
        SetObject(lockLogic, "lockVisual", padlockVisual);
        SetObjects(lockLogic, "disableWhileOpen", playerControls);
        SetString(codeLock.AddComponent<InteractionPrompt>(), "text", "Usar el candado");

        ClearListeners(chest.OnOpened);
        UnityEventTools.AddPersistentListener(chest.OnOpened, lidHinge.Open);
        UnityEventTools.AddPersistentListener(chest.OnOpened, pickup.Collect);
        EditorUtility.SetDirty(chest);

        // Puerta de salida (se reutiliza el objeto Door)
        DoorController door = Object.FindObjectOfType<DoorController>();
        if (door == null)
            door = new GameObject("Door").AddComponent<DoorController>();
        GameObject doorRoot = door.gameObject;
        StripVisual(doorRoot);
        doorRoot.name = "Door";
        doorRoot.transform.SetParent(null);
        doorRoot.transform.SetPositionAndRotation(new Vector3(0f, 1.3f, HalfZ + WallThickness / 2f), Quaternion.Euler(0f, 180f, 0f));
        SetString(doorRoot.AddComponent<InteractionPrompt>(), "text", "Abrir la puerta");

        GameObject doorModel = Model(PolyHaven + "large_castle_door/large_castle_door_1k.gltf", "Puerta de castillo", doorRoot.transform);
        Transform leftLeaf = FindChild(doorModel.transform, "large_castle_door_left");
        Transform rightLeaf = FindChild(doorModel.transform, "large_castle_door_right");
        if (leftLeaf != null) leftLeaf.localRotation = Quaternion.identity;   // el modelo viene entreabierto
        if (rightLeaf != null) rightLeaf.localRotation = Quaternion.identity;
        Recenter(doorModel, 0f, 0f, HalfZ + WallThickness / 2f);
        FitCollider(doorRoot, doorModel);

        var doorHinges = doorRoot.AddComponent<HingeAnimator>();
        var leaves = new List<(Transform, Vector3)>();
        foreach (Transform leaf in new[] { leftLeaf, rightLeaf })
        {
            if (leaf == null)
                continue;
            // Abre hacia afuera del cuarto (local -z de la puerta)
            float freeEdge = leaf.InverseTransformPoint(Bounds(leaf.gameObject).center).x;
            leaves.Add((leaf, new Vector3(0f, freeEdge >= 0f ? 100f : -100f, 0f)));
        }
        SetHinges(doorHinges, 2.2f, 0.2f, leaves.ToArray());

        // Luz del día que entra al abrir la puerta
        var outside = new GameObject("Luz exterior");
        outside.transform.SetParent(lights, false);
        Light sun = PointLight("Luz del pasillo", outside.transform, new Vector3(0f, 2.4f, HalfZ + 1.6f), new Color(1f, 0.95f, 0.85f), 2.2f, 5f, LightShadows.None, false);
        sun.name = "Luz del pasillo";
        var spotObject = new GameObject("Luz que entra");
        spotObject.transform.SetParent(outside.transform, false);
        spotObject.transform.position = new Vector3(0f, 2.5f, HalfZ + 2.4f);
        spotObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.2f, HalfZ - 2.2f) - spotObject.transform.position);
        Light spot = spotObject.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = new Color(1f, 0.95f, 0.86f);
        spot.intensity = 4f;
        spot.range = 10f;
        spot.spotAngle = 70f;
        spot.shadows = LightShadows.Soft;
        outside.SetActive(false);

        ClearListeners(door.OnOpened);
        UnityEventTools.AddPersistentListener(door.OnOpened, doorHinges.Open);
        UnityEventTools.AddBoolPersistentListener(door.OnOpened, outside.SetActive, true);
        UnityEventTools.AddPersistentListener(door.OnOpened, hud.ShowVictory);
        EditorUtility.SetDirty(door);

        // PuzzleManager sigue validando el mismo código; el panel de prueba queda apagado
        PuzzleManager puzzleManager = Object.FindObjectOfType<PuzzleManager>();
        if (puzzleManager == null)
            puzzleManager = new GameObject("PuzzleManager").AddComponent<PuzzleManager>();
        SetObject(puzzleManager, "chest", chest);
        SetObject(lockLogic, "puzzleManager", puzzleManager);

        PuzzleTestPanel testPanel = Object.FindObjectOfType<PuzzleTestPanel>(true);
        if (testPanel != null)
        {
            SetObject(testPanel, "door", door);
            testPanel.enabled = false; // activarlo en el Inspector para volver a usar los botones de prueba
            EditorUtility.SetDirty(testPanel);
        }

        if (Object.FindObjectOfType<GameManager>() == null)
            new GameObject("GameManager").AddComponent<GameManager>();
    }

    // Llave antigua de latón, armada con piezas (Poly Haven no tiene llaves)
    private static GameObject CreateKey(Transform parent, Vector3 localPosition)
    {
        var key = new GameObject("Llave");
        key.transform.SetParent(parent, false);
        key.transform.localPosition = localPosition;
        key.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
        key.transform.localScale = Vector3.one * 1.3f;

        Mesh ring = TorusMesh("Toro_Llave", 0.028f, 0.007f, 360f);
        MeshPart("Aro", key.transform, ring, new Vector3(-0.075f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), brassMat);
        Part(PrimitiveType.Cylinder, "Cana", key.transform, new Vector3(0.012f, 0f, 0f), new Vector3(0.011f, 0.065f, 0.011f), Quaternion.Euler(0f, 0f, 90f), brassMat);
        Part(PrimitiveType.Cylinder, "Collar", key.transform, new Vector3(-0.04f, 0f, 0f), new Vector3(0.02f, 0.004f, 0.02f), Quaternion.Euler(0f, 0f, 90f), brassMat);
        Part(PrimitiveType.Cube, "Paleton", key.transform, new Vector3(0.062f, 0f, 0.018f), new Vector3(0.026f, 0.007f, 0.03f), Quaternion.identity, brassMat);
        Part(PrimitiveType.Cube, "Diente", key.transform, new Vector3(0.071f, 0f, 0.038f), new Vector3(0.008f, 0.007f, 0.012f), Quaternion.identity, brassMat);
        return key;
    }

    // Candado de combinación armado con piezas. Devuelve el objeto con el collider (para CodeLock)
    private static GameObject CreatePadlock(Transform parent, Vector3 localPosition, out Transform visual)
    {
        var host = new GameObject("Candado");
        host.transform.SetParent(parent, false);
        host.transform.localPosition = localPosition;
        BoxCollider box = host.AddComponent<BoxCollider>();
        box.size = new Vector3(0.2f, 0.2f, 0.14f);

        visual = new GameObject("Candado (modelo)").transform;
        visual.SetParent(host.transform, false);
        Part(PrimitiveType.Cube, "Cuerpo", visual, new Vector3(0f, -0.01f, 0f), new Vector3(0.075f, 0.062f, 0.028f), Quaternion.identity, brassMat);
        Mesh arc = TorusMesh("Medio_Toro_Candado", 0.024f, 0.005f, 180f);
        MeshPart("Arco", visual, arc, new Vector3(0f, 0.04f, 0f), Quaternion.identity, steelMat);
        Part(PrimitiveType.Cylinder, "Pata izquierda", visual, new Vector3(-0.024f, 0.03f, 0f), new Vector3(0.01f, 0.01f, 0.01f), Quaternion.identity, steelMat);
        Part(PrimitiveType.Cylinder, "Pata derecha", visual, new Vector3(0.024f, 0.03f, 0f), new Vector3(0.01f, 0.01f, 0.01f), Quaternion.identity, steelMat);
        for (int i = 0; i < 4; i++)
        {
            float x = -0.024f + i * 0.016f;
            Part(PrimitiveType.Cube, "Rueda " + (i + 1), visual, new Vector3(x, -0.016f, 0.0145f), new Vector3(0.011f, 0.018f, 0.004f), Quaternion.identity, ironMat);
        }
        return host;
    }

    private static Behaviour[] PlayerControls(GameObject player)
    {
        return new Behaviour[]
        {
            player.GetComponent<PlayerController>(),
            player.GetComponent<PlayerInteraction>(),
            player.GetComponent<FirstPersonLook>()
        }.Where(b => b != null).ToArray();
    }

    // ------------------------------------------------------------------ jugador, HUD y ambiente

    private static GameObject SetupPlayer()
    {
        PlayerController controller = Object.FindObjectOfType<PlayerController>();
        if (controller == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            controller = ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).GetComponent<PlayerController>();
        }

        GameObject player = controller.gameObject;
        player.transform.SetPositionAndRotation(new Vector3(0.3f, 1.02f, -1.75f), Quaternion.identity);

        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule != null)
            capsule.radius = 0.3f;
        MeshRenderer body = player.GetComponent<MeshRenderer>();
        if (body != null)
            body.enabled = false; // en primera persona la cápsula no se ve (ni su sombra)

        if (player.GetComponent<PlayerInteraction>() == null)
            player.AddComponent<PlayerInteraction>();
        if (player.GetComponent<PlayerCollision>() == null)
            player.AddComponent<PlayerCollision>();
        FirstPersonLook look = player.GetComponent<FirstPersonLook>();
        if (look == null)
            look = player.AddComponent<FirstPersonLook>();

        // La cámara pasa a ser los ojos del jugador
        Camera camera = Object.FindObjectsOfType<Camera>().FirstOrDefault(c => c.CompareTag("MainCamera"));
        if (camera == null)
        {
            camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.gameObject.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
        }
        camera.transform.SetParent(player.transform, false);
        camera.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        camera.transform.localRotation = Quaternion.identity;
        camera.fieldOfView = 65f;
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = 60f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.renderingPath = RenderingPath.DeferredShading; // muchas luces de velas sin perder rendimiento
        camera.allowHDR = true;
        camera.allowMSAA = false;
        SetObject(look, "cameraPivot", camera.transform);

        EditorUtility.SetDirty(player);
        return player;
    }

    private static GameHUD SetupHud(GameObject player)
    {
        GameHUD hud = new GameObject("HUD").AddComponent<GameHUD>();
        SetObject(hud, "player", player.transform);
        SetObjects(hud, "disableOnVictory", PlayerControls(player));

        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            float distance = new SerializedObject(interaction).FindProperty("interactionDistance").floatValue;
            Set(hud, "interactionDistance", p => p.floatValue = distance);
        }
        return hud;
    }

    private static void SetupEnvironment(Transform room)
    {
        RenderSettings.skybox = null;
        RenderSettings.sun = null;
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.16f, 0.13f, 0.1f);
        RenderSettings.ambientEquatorColor = new Color(0.1f, 0.08f, 0.06f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.04f, 0.03f);

        // Reflejos del cuarto para el latón y los barnices
        var probeObject = new GameObject("Reflejos");
        probeObject.transform.SetParent(room, false);
        probeObject.transform.position = new Vector3(0f, 1.6f, 0f);
        ReflectionProbe probe = probeObject.AddComponent<ReflectionProbe>();
        probe.size = new Vector3(2f * HalfX, Height, 2f * HalfZ);
        probe.boxProjection = true;
        probe.resolution = 256;
        probe.mode = ReflectionProbeMode.Custom;
        string probePath = GeneratedFolder + "/Reflejos_Estudio.exr";
        try
        {
            if (Lightmapping.BakeReflectionProbe(probe, probePath))
            {
                AssetDatabase.ImportAsset(probePath);
                probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Texture>(probePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se pudo hornear el reflejo, se calcula al dar Play: " + e.Message);
        }
        if (probe.customBakedTexture == null)
        {
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        }
    }

    private static void AddSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == ScenePath))
            return;
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ------------------------------------------------------------------ ayudantes: modelos

    private static GameObject Model(string path, string name, Transform parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject go;
        if (asset != null)
        {
            go = (GameObject)Object.Instantiate(asset);
        }
        else
        {
            Debug.LogWarning("Falta el modelo " + path + ": se usa un cubo gris en su lugar.");
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        go.name = name;
        go.transform.SetParent(parent, false);
        foreach (Collider c in go.GetComponentsInChildren<Collider>())
            Object.DestroyImmediate(c);
        return go;
    }

    // Coloca un modelo apoyado a la altura y, centrado en (x, z) y girado "yaw" grados.
    // Con "height" se escala para que mida esa altura.
    private static GameObject Place(string path, string name, Transform parent, float x, float y, float z, float yaw, float height = 0f)
    {
        GameObject go = Model(path, name, parent);
        // Se suma el giro al que trae el modelo (los de Sketchfab vienen girados para quedar parados)
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * go.transform.localRotation;
        if (height > 0f)
        {
            float current = Bounds(go).size.y;
            if (current > 0.001f)
                go.transform.localScale *= height / current;
        }
        Recenter(go, x, y, z);
        return go;
    }

    private static void Recenter(GameObject go, float x, float y, float z)
    {
        Bounds b = Bounds(go);
        go.transform.position += new Vector3(x - b.center.x, y - b.min.y, z - b.center.z);
    }

    // Empuja el objeto hasta que toque la pared indicada (Vector3.right = pared este, etc.)
    private static void AgainstWall(GameObject go, Vector3 wall, float gap = 0.03f)
    {
        Bounds b = Bounds(go);
        float inner = PanelThickness + gap;
        Vector3 shift = Vector3.zero;
        if (wall.x > 0.5f) shift.x = HalfX - inner - b.max.x;
        else if (wall.x < -0.5f) shift.x = -HalfX + inner - b.min.x;
        if (wall.z > 0.5f) shift.z = HalfZ - inner - b.max.z;
        else if (wall.z < -0.5f) shift.z = -HalfZ + inner - b.min.z;
        go.transform.position += shift;
    }

    private static Bounds Bounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers)
            b.Encapsulate(r.bounds);
        return b;
    }

    // Caja del objeto medida en el espacio local de "space" (para colliders que giran con el objeto)
    private static Bounds LocalBounds(Transform space, GameObject content)
    {
        bool first = true;
        var result = new Bounds();
        foreach (MeshFilter filter in content.GetComponentsInChildren<MeshFilter>())
        {
            if (filter.sharedMesh == null)
                continue;
            Bounds mesh = filter.sharedMesh.bounds;
            Matrix4x4 toSpace = space.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = mesh.center + Vector3.Scale(mesh.extents,
                    new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                Vector3 point = toSpace.MultiplyPoint3x4(corner);
                if (first)
                {
                    result = new Bounds(point, Vector3.zero);
                    first = false;
                }
                else
                {
                    result.Encapsulate(point);
                }
            }
        }
        return result;
    }

    private static BoxCollider FitCollider(GameObject host, GameObject content = null)
    {
        Bounds local = LocalBounds(host.transform, content != null ? content : host);
        BoxCollider box = host.GetComponent<BoxCollider>();
        if (box == null)
            box = host.AddComponent<BoxCollider>();
        box.center = local.center;
        box.size = local.size;
        return box;
    }

    // Mueble que no se mueve: collider para chocar y marcado como estático
    private static void Solid(GameObject go, bool collider = true, bool markStatic = true)
    {
        if (collider)
            FitCollider(go);
        if (markStatic)
            MarkStatic(go);
    }

    // Sin "Batching Static": al combinar las mallas de glTFast en Play se deforman
    private static void MarkStatic(GameObject go)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
        {
            GameObjectUtility.SetStaticEditorFlags(t.gameObject,
                StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }
    }

    // Poly Haven guarda la transparencia del vidrio y de las llamas en un canal alfa que el JPG de 1k
    // no tiene, así que se verían opacos: se cambian por materiales propios.
    private static void FixGlassAndFlames(GameObject go)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                string name = materials[i] != null ? materials[i].name.ToLower() : "";
                if (name.Contains("glass")) { materials[i] = glassMat; changed = true; }
                else if (name.Contains("flame")) { materials[i] = flameMat; changed = true; }
            }
            if (changed)
                renderer.sharedMaterials = materials;
        }
    }

    private static void NoShadows(GameObject go)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = ShadowCastingMode.Off;
    }

    // Convierte un modelo en pista: objeto con collider, ClueObject y el texto del aviso
    private static GameObject Clue(GameObject model, Transform parent, string name, string prompt, string clue)
    {
        var host = new GameObject(name);
        host.transform.SetParent(parent, false);
        host.transform.SetPositionAndRotation(Bounds(model).center, model.transform.rotation);
        model.transform.SetParent(host.transform, true);
        FitCollider(host);
        MarkStatic(host);
        SetString(host.AddComponent<ClueObject>(), "clue", clue);
        SetString(host.AddComponent<InteractionPrompt>(), "text", prompt);
        return host;
    }

    private static Transform FindChild(Transform parent, string name)
    {
        return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    }

    // Cierra las puertas de un mueble que en el modelo vienen entreabiertas
    private static void CloseDoors(GameObject furniture)
    {
        foreach (Transform t in furniture.GetComponentsInChildren<Transform>(true))
        {
            if (t != furniture.transform && t.name.ToLower().Contains("door"))
                t.localRotation = Quaternion.identity;
        }
    }

    // Deja solo una pieza de un modelo que trae varias (por ejemplo, un candelabro de un juego de tres)
    private static void KeepOnly(GameObject go, string childName)
    {
        Transform keep = FindChild(go.transform, childName);
        if (keep == null)
            return;
        foreach (Transform sibling in keep.parent.Cast<Transform>().ToArray())
        {
            if (sibling != keep)
                Object.DestroyImmediate(sibling.gameObject);
        }
    }

    // Gira el objeto 180° si su cara "meshName" quedó mirando a la pared
    private static void FaceDirection(GameObject go, string meshName, Vector3 desired)
    {
        Transform face = FindChild(go.transform, meshName);
        MeshFilter filter = face != null ? face.GetComponentInChildren<MeshFilter>() : null;
        if (filter == null || filter.sharedMesh == null)
            return;

        Vector3 normal = Vector3.zero;
        try
        {
            foreach (Vector3 n in filter.sharedMesh.normals)
                normal += n;
        }
        catch (System.Exception)
        {
            return;
        }
        normal = filter.transform.TransformDirection(normal);
        if (Vector3.Dot(normal, desired) < 0f)
        {
            Vector3 center = Bounds(go).center;
            go.transform.RotateAround(center, Vector3.up, 180f);
        }
    }

    // Pone las agujas del reloj de pie en la hora de la pista
    private static void SetClockHands(GameObject clock, int hour, int minute)
    {
        PointHand(FindChild(clock.transform, "vintage_grandfather_clock_01_HourHand"), (hour % 12 + minute / 60f) * 30f);
        PointHand(FindChild(clock.transform, "vintage_grandfather_clock_01_MinuteHand"), minute * 6f);
    }

    // clockAngle: 0 = las 12, 90 = las 3 (en sentido horario, mirando el reloj de frente)
    private static void PointHand(Transform hand, float clockAngle)
    {
        MeshFilter filter = hand != null ? hand.GetComponentInChildren<MeshFilter>() : null;
        if (filter == null || filter.sharedMesh == null)
            return;

        Transform space = hand.parent;
        Vector3 pivot = space.InverseTransformPoint(hand.position);
        Vector3 tip = pivot;
        float farthest = -1f;
        IEnumerable<Vector3> points;
        try
        {
            points = filter.sharedMesh.vertices;
        }
        catch (System.Exception)
        {
            Bounds mb = filter.sharedMesh.bounds;
            points = Enumerable.Range(0, 8).Select(i => mb.center + Vector3.Scale(mb.extents,
                new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f)));
        }
        foreach (Vector3 v in points)
        {
            Vector3 p = space.InverseTransformPoint(filter.transform.TransformPoint(v));
            float d = new Vector2(p.x - pivot.x, p.y - pivot.y).sqrMagnitude;
            if (d > farthest)
            {
                farthest = d;
                tip = p;
            }
        }

        Vector3 current = tip - pivot;
        current.z = 0f;
        Vector3 front = pivot.z >= 0f ? Vector3.forward : Vector3.back; // las agujas están delante del cuerpo
        Vector3 viewerRight = Vector3.Cross(Vector3.up, -front);
        float radians = clockAngle * Mathf.Deg2Rad;
        Vector3 target = Vector3.up * Mathf.Cos(radians) + viewerRight * Mathf.Sin(radians);
        float angle = Vector3.SignedAngle(current, target, Vector3.forward);
        hand.localRotation = Quaternion.AngleAxis(angle, Vector3.forward) * hand.localRotation;
    }

    // ------------------------------------------------------------------ ayudantes: piezas y mallas

    private static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.transform.localScale = localScale;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        return go;
    }

    private static GameObject MeshPart(string name, Transform parent, Mesh mesh, Vector3 localPosition, Quaternion localRotation, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        return go;
    }

    // Caja con UV en metros: la textura se repite cada "tile" metros sin estirarse
    private static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material material, float tile, bool collider = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(name, size, tile);
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        if (collider)
            go.AddComponent<BoxCollider>().size = size;
        return go;
    }

    private static Mesh BoxMesh(string name, Vector3 size, float tile)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        Vector3 half = size / 2f;
        Vector3[] faceNormals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

        foreach (Vector3 n in faceNormals)
        {
            Vector3 up = Mathf.Abs(n.y) > 0.5f ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(n, up);
            float halfRight = Mathf.Abs(Vector3.Dot(half, right));
            float halfUp = Mathf.Abs(Vector3.Dot(half, up));
            Vector3 center = Vector3.Scale(n, half);
            int start = vertices.Count;

            vertices.Add(center - right * halfRight - up * halfUp);
            vertices.Add(center - right * halfRight + up * halfUp);
            vertices.Add(center + right * halfRight + up * halfUp);
            vertices.Add(center + right * halfRight - up * halfUp);
            float u = 2f * halfRight / tile;
            float v = 2f * halfUp / tile;
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, v));
            uvs.Add(new Vector2(u, v));
            uvs.Add(new Vector2(u, 0f));
            for (int i = 0; i < 4; i++)
                normals.Add(n);
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    // Toro (o parte de un toro) en el plano XY: aro de la llave y arco del candado
    private static Mesh TorusMesh(string name, float radius, float tube, float arcDegrees, int segments = 32, int sides = 12)
    {
        string path = GeneratedFolder + "/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = new Mesh { name = name };
            AssetDatabase.CreateAsset(mesh, path);
        }

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        float arc = arcDegrees * Mathf.Deg2Rad;
        for (int i = 0; i <= segments; i++)
        {
            float theta = arc * i / segments;
            var ringCenter = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f);
            for (int j = 0; j <= sides; j++)
            {
                float phi = 2f * Mathf.PI * j / sides;
                Vector3 normal = ringCenter * Mathf.Cos(phi) + Vector3.forward * Mathf.Sin(phi);
                vertices.Add(ringCenter * radius + normal * tube);
                normals.Add(normal);
            }
        }
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int a = i * (sides + 1) + j;
                int b = a + sides + 1;
                triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
        }

        // Si las caras quedaron al revés, se invierte el orden de los vértices
        Vector3 v0 = vertices[triangles[0]], v1 = vertices[triangles[1]], v2 = vertices[triangles[2]];
        if (Vector3.Dot(Vector3.Cross(v1 - v0, v2 - v0), normals[triangles[0]]) < 0f)
        {
            for (int k = 0; k < triangles.Count; k += 3)
                (triangles[k + 1], triangles[k + 2]) = (triangles[k + 2], triangles[k + 1]);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static Light PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, LightShadows shadows, bool flicker)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = shadows;
        light.shadowStrength = 0.85f;
        if (flicker)
            go.AddComponent<CandleFlicker>();
        return light;
    }

    // ------------------------------------------------------------------ ayudantes: campos privados y eventos

    private static void Set(Object target, string field, System.Action<SerializedProperty> assign)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            Debug.LogError(target.GetType().Name + " no tiene el campo " + field);
            return;
        }
        assign(property);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value) => Set(target, field, p => p.stringValue = value);
    private static void SetBool(Object target, string field, bool value) => Set(target, field, p => p.boolValue = value);
    private static void SetObject(Object target, string field, Object value) => Set(target, field, p => p.objectReferenceValue = value);

    private static void SetObjects(Object target, string field, Object[] values)
    {
        Set(target, field, p =>
        {
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        });
    }

    private static void SetHinges(HingeAnimator animator, float duration, float delay, params (Transform pivot, Vector3 angles)[] hinges)
    {
        var serialized = new SerializedObject(animator);
        SerializedProperty list = serialized.FindProperty("hinges");
        list.arraySize = hinges.Length;
        for (int i = 0; i < hinges.Length; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("pivot").objectReferenceValue = hinges[i].pivot;
            element.FindPropertyRelative("openAngles").vector3Value = hinges[i].angles;
        }
        serialized.FindProperty("duration").floatValue = duration;
        serialized.FindProperty("delay").floatValue = delay;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearListeners(UnityEngine.Events.UnityEventBase unityEvent)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(unityEvent, i);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
