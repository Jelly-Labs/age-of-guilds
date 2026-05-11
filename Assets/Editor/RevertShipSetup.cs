using UnityEditor;
using UnityEngine;

public class RevertShipSetup : EditorWindow
{
    [MenuItem("Tools/Revert to Shader Quad")]
    public static void RunSetup()
    {
        string[] shipGuids = AssetDatabase.FindAssets("Ship t:GameObject");
        string prefabPath = "";
        foreach(var guid in shipGuids) {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.Contains(".prefab") && p.Contains("Ship.prefab")) { prefabPath = p; break; }
        }

        if (string.IsNullOrEmpty(prefabPath)) return;

        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

        Transform tr = prefab.transform.Find("TrailWake");
        if (tr != null) DestroyImmediate(tr.gameObject);
        
        Transform pw = prefab.transform.Find("BowSplash");
        if (pw != null) DestroyImmediate(pw.gameObject);

        Transform wake = prefab.transform.Find("WakeQuad");
        if (wake == null)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "WakeQuad";
            DestroyImmediate(quad.GetComponent<MeshCollider>());
            quad.transform.SetParent(prefab.transform);
            wake = quad.transform;
        }

        wake.transform.localPosition = new Vector3(0, 0.05f, 0.1f);
        wake.transform.localRotation = Quaternion.Euler(90, 0, 0);
        wake.transform.localScale = new Vector3(1.5f, 3f, 1f); 
        
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShipWakeMat.mat");
        if (mat != null) wake.GetComponent<MeshRenderer>().sharedMaterial = mat;

        ShipVisuals visuals = prefab.GetComponent<ShipVisuals>();
        if (visuals != null) {
            visuals.wakeRenderer = wake.GetComponent<MeshRenderer>();
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Vector3 oldPos = new Vector3(0, 0f, 0);

        foreach(var obj in allObjects) {
            if (obj.name == "Ship01" || obj.name == "Ship") {
                oldPos = obj.transform.position;
                DestroyImmediate(obj);
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        instance.name = "Ship01";
        instance.transform.position = oldPos;
        
        GameObject shipsFolder = GameObject.Find("_Ships");
        if (shipsFolder != null) instance.transform.SetParent(shipsFolder.transform);
        
        Debug.Log("Reversion successful!");
    }
}
