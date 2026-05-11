using UnityEngine;

[ExecuteAlways]
public class GlobalCloudManager : MonoBehaviour
{
    [Tooltip("Assign the Puff Clouds material here so its shadow drops dynamically onto the terrain in real-time.")]
    public Material puffCloudMaterial;

    void Update()
    {
        if (puffCloudMaterial != null)
        {
            // Sync the exact mathematical scale, coverage, and wind speed from the Cloud into the mathematical Shadow
            Shader.SetGlobalFloat("_GlobalCloudScale", puffCloudMaterial.GetFloat("_Scale"));
            Shader.SetGlobalFloat("_GlobalCloudCoverage", puffCloudMaterial.GetFloat("_Coverage"));
            Shader.SetGlobalFloat("_GlobalCloudSoftness", puffCloudMaterial.GetFloat("_Softness"));
            Shader.SetGlobalFloat("_GlobalCloudDeform", puffCloudMaterial.GetFloat("_Deform"));
            Shader.SetGlobalVector("_GlobalCloudWind", puffCloudMaterial.GetVector("_WindSpeed"));
            Shader.SetGlobalColor("_GlobalCloudShadowTint", puffCloudMaterial.GetColor("_TerrainShadowTint"));
        }
    }
}
