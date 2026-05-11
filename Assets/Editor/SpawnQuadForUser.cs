using UnityEditor;
using UnityEngine;

public class SpawnQuadForUser {
    [MenuItem("Age of Guilds/Spawn Quad Auto")]
    public static void Spawn() {
        GameObject target = GameObject.Find("Waterplane");
        if (target == null) target = GameObject.Find("island02_water");
        if (target == null) target = GameObject.Find("Island02");
        if (target == null) target = GameObject.Find("island01_water");
        if (target == null) target = GameObject.Find("Island01");
        
        // Find ANY mesh renderer in the scene if specific names fail
        if (target == null) {
            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>();
            foreach (var r in renderers) {
                if(r.name.ToLower().Contains("water") || r.name.ToLower().Contains("island")) {
                    target = r.gameObject; break;
                }
            }
        }

        if (target == null) {
            Debug.LogError("Could not find any water or island object!");
            return;
        }

        Bounds bounds = target.GetComponentInChildren<Renderer>().bounds;
        GameObject mistQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mistQuad.name = "Layer1_SeaMist_" + target.name;
        mistQuad.transform.position = new Vector3(bounds.center.x, 0.6f, bounds.center.z);
        mistQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
        mistQuad.transform.localScale = new Vector3(bounds.size.x, bounds.size.z, 1);
        
        Material mistMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Layer1_SeaMistMat.mat");
        if (mistMat != null) mistQuad.GetComponent<Renderer>().sharedMaterial = mistMat;
        
        Debug.Log("SPAWNED! Set position to " + mistQuad.transform.position + " scale " + mistQuad.transform.localScale);
    }
}
