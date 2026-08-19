using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driverstation : MonoBehaviour
{
    [SerializeField] private GameObject Enabled;
    [SerializeField] private GameObject Disabled;
    private void OnEnable()
    {
        GameManager.OnRobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnRobotStateChanged -= HandleRobotStateChanged;
    }
    private void HandleRobotStateChanged(bool enabledState)
    {
        if (enabledState)
        {
            Enabled.SetActive(false);
            Disabled.SetActive(true);
        }
        else
        {
            Enabled.SetActive(true);
            Disabled.SetActive(false);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
