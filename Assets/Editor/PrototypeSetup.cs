using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PrototypeSetup : EditorWindow
{
    [MenuItem("Tools/Setup Prototype Scene")]
    public static void RunSetup()
    {
        // 1. Create Folders
        GameObject env = new GameObject("_Environment");
        GameObject lighting = new GameObject("Lighting");
        lighting.transform.SetParent(env.transform);
        GameObject islands = new GameObject("_Islands");
        islands.transform.SetParent(env.transform);

        GameObject entities = new GameObject("_Entities");
        GameObject towns = new GameObject("_Towns");
        towns.transform.SetParent(entities.transform);
        GameObject ships = new GameObject("_Ships");
        ships.transform.SetParent(entities.transform);

        GameObject systems = new GameObject("_Systems");

        // 2. Reparent existing
        var water = GameObject.Find("WaterPlane");
        if (water != null) water.transform.SetParent(env.transform);
        
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null) dirLight.transform.SetParent(lighting.transform);
        var vol = GameObject.Find("Global Volume");
        if (vol != null) vol.transform.SetParent(lighting.transform);

        for (int i=1; i<=4; i++) {
            var isl = GameObject.Find("Island_Sphere_" + i);
            if (isl != null) isl.transform.SetParent(islands.transform);
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // 3. Create Town Prefab
        GameObject townObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        townObj.name = "Town";
        townObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Material townMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        townMat.color = new Color(0.8f, 0.2f, 0.2f); // Red
        AssetDatabase.CreateAsset(townMat, "Assets/Prefabs/TownMat.mat");
        townObj.GetComponent<Renderer>().sharedMaterial = townMat;
        GameObject townPrefab = PrefabUtility.SaveAsPrefabAsset(townObj, "Assets/Prefabs/Town.prefab");
        DestroyImmediate(townObj);

        // 4. Create Ship Prefab
        GameObject shipObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        shipObj.name = "Ship";
        shipObj.transform.localScale = new Vector3(0.3f, 0.3f, 1.0f);
        Material shipMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        shipMat.color = new Color(0.2f, 0.3f, 0.8f); // Blue
        AssetDatabase.CreateAsset(shipMat, "Assets/Prefabs/ShipMat.mat");
        shipObj.GetComponent<Renderer>().sharedMaterial = shipMat;
        
        // Add a "bow" point to ship so we clearly see direction
        GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bow.transform.SetParent(shipObj.transform);
        bow.transform.localPosition = new Vector3(0, 0, 0.5f);
        bow.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        bow.GetComponent<Renderer>().sharedMaterial = shipMat;

        GameObject shipPrefab = PrefabUtility.SaveAsPrefabAsset(shipObj, "Assets/Prefabs/Ship.prefab");
        DestroyImmediate(shipObj);

        // 5. Spawn Towns
        var p1 = new Vector3(-4, 0.4f, -3);
        var t1 = (GameObject)PrefabUtility.InstantiatePrefab(townPrefab);
        t1.transform.position = p1; t1.transform.SetParent(towns.transform);
        
        var p2 = new Vector3(3, 0.5f, 4);
        var t2 = (GameObject)PrefabUtility.InstantiatePrefab(townPrefab);
        t2.transform.position = p2; t2.transform.SetParent(towns.transform);
        
        var p3 = new Vector3(1, 0.35f, -5);
        var t3 = (GameObject)PrefabUtility.InstantiatePrefab(townPrefab);
        t3.transform.position = p3; t3.transform.SetParent(towns.transform);

        // 6. Spawn Ship
        var s1 = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab);
        s1.transform.position = new Vector3(0, 0, 0); s1.transform.SetParent(ships.transform);

        // 7. Add Camera Rig
        GameObject camRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GlobeCameraRig.prefab");
        if (camRigPrefab != null) {
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(camRigPrefab);
            rig.transform.SetParent(systems.transform);
            var mc = GameObject.Find("Main Camera");
            if(mc != null) DestroyImmediate(mc);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
