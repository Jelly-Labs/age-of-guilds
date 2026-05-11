using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class CitySceneSetup
{
    private const string ScenePath = "Assets/Scenes/CityScene.unity";
    private const string TownPrefabPath = "Assets/Models/Prefabs/Town.prefab";
    private const string VolumeProfilePath = "Assets/Settings/SampleSceneProfile.asset";

    [MenuItem("Tools/Setup City Scene")]
    public static void SetupCitySceneMenu()
    {
        SetupCityScene();
    }

    public static void SetupCityScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Transform environment = GetOrCreateRoot("_Environment");
        Transform lighting = GetOrCreateChild(environment, "Lighting");
        Transform terrain = GetOrCreateChild(environment, "_Terrain");
        Transform architecture = GetOrCreateChild(environment, "_Architecture");
        Transform water = GetOrCreateChild(environment, "_Water");
        Transform foliage = GetOrCreateChild(environment, "_Foliage");
        Transform background = GetOrCreateChild(environment, "_Background");

        Transform props = GetOrCreateRoot("_Props");
        Transform staticProps = GetOrCreateChild(props, "_Static");
        Transform dynamicProps = GetOrCreateChild(props, "_Dynamic");
        Transform vfx = GetOrCreateChild(props, "_VFX");

        Transform interactables = GetOrCreateRoot("_Interactables");
        Transform buildings = GetOrCreateChild(interactables, "_Buildings");
        Transform clickableProps = GetOrCreateChild(interactables, "_ClickableProps");
        Transform entrances = GetOrCreateChild(interactables, "_Entrances");
        Transform triggers = GetOrCreateChild(interactables, "_Triggers");

        Transform characters = GetOrCreateRoot("_Characters");
        Transform npcs = GetOrCreateChild(characters, "_NPCs");
        Transform vehicles = GetOrCreateChild(characters, "_Vehicles");
        Transform playerSpawn = GetOrCreateChild(characters, "_PlayerSpawn");

        Transform systems = GetOrCreateRoot("_Systems");
        Transform sceneRefs = GetOrCreateChild(systems, "_SceneRefs");
        Transform navigation = GetOrCreateChild(systems, "_Navigation");
        Transform audio = GetOrCreateChild(systems, "_Audio");
        Transform gameplay = GetOrCreateChild(systems, "_Gameplay");
        Transform debug = GetOrCreateChild(systems, "_Debug");

        ReparentIfExists("Directional Light", lighting);
        ReparentIfExists("Global Volume", lighting);

        EnsureGlobalVolume(lighting);
        ReparentByName("Water", water);
        ReparentByName("Ocean", water);
        ReparentByName("Sea", water);

        ReparentByName("Tree", foliage);
        ReparentByName("Bush", foliage);
        ReparentByName("Rock", foliage);

        ReparentByName("Road", architecture);
        ReparentByName("Street", architecture);
        ReparentByName("Bridge", architecture);
        ReparentByName("Wall", architecture);
        ReparentByName("Gate", architecture);
        ReparentByName("Tower", architecture);
        ReparentByName("Building", buildings);
        ReparentByName("House", buildings);
        ReparentByName("Shop", buildings);
        ReparentByName("Market", buildings);
        ReparentByName("Town", buildings);
        ReparentByName("City", buildings);

        ReparentByName("Crate", staticProps);
        ReparentByName("Barrel", staticProps);
        ReparentByName("Bench", staticProps);
        ReparentByName("Lamp", staticProps);
        ReparentByName("Cart", dynamicProps);

        ReparentByName("NPC", npcs);
        ReparentByName("Guard", npcs);
        ReparentByName("Vendor", npcs);
        ReparentByName("Citizen", npcs);
        ReparentByName("Ship", vehicles);
        ReparentByName("Boat", vehicles);

        EnsureStarterCity(buildings);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("CityScene hierarchy prepared.");
    }

    private static Transform GetOrCreateRoot(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing.transform;
        }

        return new GameObject(name).transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing;
        }

        Transform child = new GameObject(name).transform;
        child.SetParent(parent, false);
        return child;
    }

    private static void ReparentIfExists(string objectName, Transform parent)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        if (target.transform == parent || target.transform.parent == parent)
        {
            return;
        }

        target.transform.SetParent(parent, true);
    }

    private static void ReparentByName(string nameFragment, Transform parent)
    {
        foreach (GameObject rootObject in parent.gameObject.scene.GetRootGameObjects())
        {
            ReparentMatchingChildren(rootObject.transform, nameFragment, parent);
        }
    }

    private static void ReparentMatchingChildren(Transform current, string nameFragment, Transform parent)
    {
        if (current == parent)
        {
            return;
        }

        if (ShouldSkipRoot(current))
        {
            Transform[] children = GetChildren(current);
            for (int i = 0; i < children.Length; i++)
            {
                ReparentMatchingChildren(children[i], nameFragment, parent);
            }

            return;
        }

        if (current.name.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (!current.IsChildOf(parent))
            {
                current.SetParent(parent, true);
            }

            return;
        }

        Transform[] nestedChildren = GetChildren(current);
        for (int i = 0; i < nestedChildren.Length; i++)
        {
            ReparentMatchingChildren(nestedChildren[i], nameFragment, parent);
        }
    }

    private static Transform[] GetChildren(Transform parent)
    {
        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i);
        }

        return children;
    }

    private static bool ShouldSkipRoot(Transform current)
    {
        if (current.parent != null)
        {
            return false;
        }

        return current.name == "Main Camera"
            || current.name == "_Environment"
            || current.name == "_Props"
            || current.name == "_Interactables"
            || current.name == "_Characters"
            || current.name == "_Systems";
    }

    private static void EnsureGlobalVolume(Transform lighting)
    {
        GameObject existing = GameObject.Find("Global Volume");
        if (existing == null)
        {
            existing = new GameObject("Global Volume");
            existing.transform.SetParent(lighting, false);
        }
        else if (existing.transform.parent != lighting)
        {
            existing.transform.SetParent(lighting, true);
        }

        Volume volume = existing.GetComponent<Volume>();
        if (volume == null)
        {
            volume = existing.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 0f;
        volume.weight = 1f;

        if (volume.sharedProfile == null)
        {
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        }
    }

    private static void EnsureStarterCity(Transform buildings)
    {
        if (HasCityContent(buildings))
        {
            return;
        }

        GameObject townPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TownPrefabPath);
        if (townPrefab == null)
        {
            return;
        }

        GameObject cityInstance = (GameObject)PrefabUtility.InstantiatePrefab(townPrefab, buildings.gameObject.scene);
        cityInstance.name = "City";
        cityInstance.transform.SetParent(buildings, false);
        cityInstance.transform.localPosition = Vector3.zero;
        cityInstance.transform.localRotation = Quaternion.identity;
        cityInstance.transform.localScale = Vector3.one;
    }

    private static bool HasCityContent(Transform buildings)
    {
        if (buildings.childCount > 0)
        {
            return true;
        }

        GameObject town = GameObject.Find("Town");
        GameObject city = GameObject.Find("City");
        GameObject genesisCity = GameObject.Find("GenesisCity01");

        return town != null || city != null || genesisCity != null;
    }
}
