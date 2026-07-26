using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Util;

/// <summary>
/// Generates a part object can be used multiple times on one object to create complex models
/// </summary>
[ExecuteAlways]
public class GeneratePart : MonoBehaviour
{
    private string partName;
    [HideInInspector] public bool ObjectSpawned;
    
    [HideInInspector] public string PartName;

    [HideInInspector] public GameObject Part;
    [HideInInspector] public Vector3 LoadedPartLocation;
    [HideInInspector] public Quaternion LoadedPartRotation;
    [HideInInspector] public Vector3 LoadedPartScale;
    
    private GameObject currentPart;
    private GameObject _loadedPart;

    private string _lastPartName;
    private Vector3 _lastLocation;
    private Quaternion _lastRotation;
    private Vector3 _lastScale;
    private GameObject _lastPartPrefab;
    

    // Start is called before the first frame update
    void OnEnable()
    {
        Startup();
    }

    protected GameObject getLoadedPart()
    {
        return _loadedPart;
    }
    
    void OnDisable()
    {
        CancelInvoke(nameof(run));
    }

    private void OnDestroy()
    {
       DestroyImmediate(_loadedPart);
    }

    protected void run()
    {
        if (PartName == _lastPartName
            && Part == _lastPartPrefab
            && LoadedPartLocation == _lastLocation
            && LoadedPartRotation == _lastRotation
            && LoadedPartScale == _lastScale)
        {
            return;
        }

        _lastPartName = PartName;
        _lastPartPrefab = Part;
        _lastLocation = LoadedPartLocation;
        _lastRotation = LoadedPartRotation;
        _lastScale = LoadedPartScale;

        if (PartName != null && Part != null)
        {
            partName = PartName;
            ObjectSpawned = _loadedPart;
            if (_loadedPart)
            {
                if (_loadedPart.name != PartName || currentPart != Part)
                {
                    DestroyImmediate(_loadedPart);
                }
            }

            if (!_loadedPart && Part)
            {
                currentPart = Part;
                _loadedPart = Instantiate(Part, LoadedPartLocation, LoadedPartRotation, transform);
                _loadedPart.name = PartName;
            }

            _loadedPart.transform.localPosition = LoadedPartLocation;
            _loadedPart.transform.localRotation = LoadedPartRotation;

            var scaleAdjustedScale = new Vector3(LoadedPartScale.x / transform.localScale.x,
                LoadedPartScale.y / transform.localScale.y, LoadedPartScale.z / transform.localScale.z);
            _loadedPart.transform.localScale = scaleAdjustedScale;
        }

        ObjectSpawned = _loadedPart;
    }

    protected void Startup()
    {
        if (partName != null)
        {
            PartName = partName;
        }

        _loadedPart = Utils.FindChild(PartName, gameObject);
        currentPart = Part;
        
        InvokeRepeating(nameof(run), 0f, 0.2f); //does the same thing as fixed update but doesnt require it be selected in editor
    }
}
