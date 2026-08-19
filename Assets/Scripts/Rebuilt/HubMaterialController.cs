using System.Collections.Generic;
using UnityEngine;

public class HubMaterialController : MonoBehaviour
{
    [Header("Material References")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    [Header("Target Meshes")]
    [SerializeField] private List<MeshRenderer> targetMeshes = new List<MeshRenderer>();

    private bool currentState = false;

    public void SetActive(bool active)
    {
        currentState = active;
        Material materialToApply = currentState ? activeMaterial : inactiveMaterial;

        if (materialToApply == null)
        {
            Debug.LogWarning($"[HubMaterialController] Missing material reference on {gameObject.name}!", this);
            return;
        }

        foreach (MeshRenderer mesh in targetMeshes)
        {
            if (mesh == null) continue;

            // Replace all material slots cleanly
            Material[] mats = new Material[mesh.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = materialToApply;
            }
            mesh.materials = mats;
        }
    }

    public void ToggleActive()
    {
        SetActive(!currentState);
    }
}