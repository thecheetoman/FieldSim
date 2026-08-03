using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMaterialController : MonoBehaviour
{
    [SerializeField] private Material ActiveMaterial;
    [SerializeField] private Material UnactiveMaterial;
    [SerializeField] private List<MeshRenderer> targetMeshes = new List<MeshRenderer>();

    private bool isActive = false;

    // function to toggle between materials, used when shift changes
    public void ToggleActive()
    {
        isActive = !isActive;
        Material materialToApply = isActive ? ActiveMaterial : UnactiveMaterial;

        foreach (MeshRenderer mesh in targetMeshes)
        {
            if (mesh != null)
            {
                // try to replace all material slots on the mesh(pls work bro)
                Material[] mats = new Material[mesh.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = materialToApply;
                }
                mesh.materials = mats;
            }
        }
    }
}