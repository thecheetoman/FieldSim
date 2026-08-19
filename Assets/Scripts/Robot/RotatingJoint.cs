using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingJoint : MonoBehaviour
{
    [Header("Hinge Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float retractedAngle = 0f;
    [SerializeField] private float extendedAngle = 90f;
    [SerializeField] private float rotationSpeed = 5f;
    [Header("Control Settings")]
    [SerializeField] private KeyCode intakeKey = KeyCode.LeftShift;

    private float currentAngle;

    [SerializeField] private bool rEnabled = false;
    private bool isDeployed = false;

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
        rEnabled = !rEnabled;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentAngle = retractedAngle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && rEnabled)
        {
            isDeployed = true;
        }
        float targetAngle = isDeployed ? extendedAngle : retractedAngle;

        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime * 100f);
        transform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);

    }
}
