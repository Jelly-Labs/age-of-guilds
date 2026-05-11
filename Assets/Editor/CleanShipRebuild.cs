using UnityEditor;
using UnityEngine;

public class CleanShipRebuild : EditorWindow
{
    [MenuItem("Tools/Clean Rebuild Ship Prefab")]
    public static void RunSetup()
    {
        string modelPath = "Assets/Models/GenesisShip01.fbx";
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (fbxAsset == null) {
            Debug.LogError("Missing GenesisShip01.fbx");
            return;
        }

        Material shipMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GenesisShipMat.mat");
        Material wakeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShipWakeMat.mat");

        GameObject shipRoot = new GameObject("Ship");

        GameObject visualModel = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
        visualModel.transform.SetParent(shipRoot.transform);
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.name = "ShipModel";

        // Store its innate FBX euler angles dynamically
        Vector3 innateRot = visualModel.transform.localEulerAngles;

        Renderer[] renderers = visualModel.GetComponentsInChildren<Renderer>();
        foreach(var r in renderers) {
            if (shipMat != null) r.sharedMaterial = shipMat;
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "WakeQuad";
        DestroyImmediate(quad.GetComponent<MeshCollider>());
        quad.transform.SetParent(shipRoot.transform);
        quad.transform.localPosition = new Vector3(0, 0.05f, 0.1f);
        quad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(1.5f, 3f, 1f); 
        if (wakeMat != null) quad.GetComponent<MeshRenderer>().sharedMaterial = wakeMat;

        ShipVisuals visuals = shipRoot.AddComponent<ShipVisuals>();
        visuals.visualModel = visualModel.transform;
        visuals.wakeRenderer = quad.GetComponent<MeshRenderer>();
        visuals.baseEulerOffset = innateRot; // Strictly pass the clean, un-tampered innate offset

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        
        string prefabPath = "Assets/Prefabs/Ship.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(shipRoot, prefabPath);
        DestroyImmediate(shipRoot);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Vector3 oldPos = new Vector3(0, 1.0f, 0);

        foreach(var obj in allObjects) {
            if (obj.name == "Ship01" || obj.name == "Ship") {
                oldPos = obj.transform.position;
                DestroyImmediate(obj);
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab);
        instance.name = "Ship01";
        instance.transform.position = oldPos;
        
        GameObject shipsFolder = GameObject.Find("_Ships");
        if (shipsFolder != null) instance.transform.SetParent(shipsFolder.transform);
        
        Debug.Log("Hard Rebuild of Ship.prefab completely successful.");
    }
}
