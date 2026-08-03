using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private bool gameStarted = false;
    public bool isBlueAlliance = true;

    int scoreBlue = 0;
    int scoreRed = 0;

    [Header("UI elements")]
    public TextMeshProUGUI blueTMPObject;
    public TextMeshProUGUI redTMPObject;
    public GameObject blueShift;
    public GameObject redShift;

    [Header("Hub material switchers")]
    public HubMaterialController RedHub;
    public HubMaterialController BlueHub;

    private void Start()
    {
        SetRobotEnabled(false);
        blueShift.SetActive(false);
        redShift.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("Robot state changed: " + !IsRobotEnabled);
            SetRobotEnabled(!IsRobotEnabled);
            if (!gameStarted)
            {
                startGame();
                gameStarted = true;
            }
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

    public void startGame()
    {
        fieldSFX.clip = soundEffects[0];
        fieldSFX.Play();
        RedHub.ToggleActive();
        BlueHub.ToggleActive();
        blueShift.SetActive(true);
        redShift.SetActive(true);
    }
    
    // handle scoring
    public void scorePoint(bool isBlue)
    {
        if (isBlue && isBlueAlliance)
        {
            scoreBlue++;
            blueTMPObject.text = scoreBlue.ToString();
        }
        else
        {
            scoreRed += 6;
            redTMPObject.text = scoreRed.ToString();
        }
    }
}
