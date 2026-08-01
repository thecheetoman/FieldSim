using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // manage robot enable and robot disable
    public static event Action<bool> OnRobotStateChanged;
    //hold robot enabled or not
    public bool IsRobotEnabled { get; private set; } = true;
    // audio thingy
    [Header("Audio system")]
    public AudioSource fieldSFX;
    [SerializeField] private List<AudioClip> soundEffects = new();


    private void Start()
    {
        SetRobotEnabled(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("Robot state changed: " + !IsRobotEnabled);
            SetRobotEnabled(!IsRobotEnabled);
        }
    }
    public void SetRobotEnabled(bool enable)
    {
        IsRobotEnabled = enable;
        OnRobotStateChanged?.Invoke(IsRobotEnabled);
        Debug.Log(IsRobotEnabled ? ">>> ROBOT ENABLED <<<" : ">>> ROBOT DISABLED <<<");
    }
    public void EnableRobot() => SetRobotEnabled(true);
    public void DisableRobot() => SetRobotEnabled(false);
}
