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

    // hold robot enabled or not
    public bool IsRobotEnabled { get; private set; } = false;

    // audio system
    [Header("Audio System")]
    public AudioSource fieldSFX;
    [Tooltip("0: Match Start | 1: Auto End / Disable | 2: Teleop Start / Enable | 3: Shift Change / Alert | 4: Match End")]
    [SerializeField] private List<AudioClip> soundEffects = new();

    [Header("Season Match Timing")]
    public bool isBlueAlliance = true;
    [SerializeField] private float autoDuration = 20f;
    [SerializeField] private float teleopTransitionDuration = 10f;
    [SerializeField] private float shiftDuration = 25f; // 4 shifts @ 25s = 100s total
    [SerializeField] private float endgameDuration = 30f;

    private bool gameStarted = false;
    private bool isMatchOver = false;

    // scores
    private int scoreBlue = 0;
    private int scoreRed = 0;
    private int autoScoreBlue = 0;
    private int autoScoreRed = 0;

    private bool isAutoPhase = false;

    // hub active state tracking
    private bool isBlueHubActive = true;
    private bool isRedHubActive = true;

    [Header("UI Elements")]
    public TextMeshProUGUI blueTMPObject;
    public TextMeshProUGUI redTMPObject;
    public GameObject blueShift;
    public GameObject redShift;
    public TextMeshProUGUI timer;

    [Header("Hub Material Switchers")]
    public HubMaterialController RedHub;
    public HubMaterialController BlueHub;

    private Coroutine matchRoutine;

    private void Start()
    {
        // start match with robot disabled and active hub arrow indicators hidden
        SetRobotEnabled(false);

        SetHubStates(false, false);

        if (blueShift != null) blueShift.SetActive(false);
        if (redShift != null) redShift.SetActive(false);

        UpdateScoreUI();

        float totalMatchLength = autoDuration + teleopTransitionDuration + (shiftDuration * 4) + endgameDuration;
        Debug.Log(totalMatchLength);
        UpdateTimerUI(totalMatchLength);
    }

    void Update()
    {
        // toggle match start or force e-stop using other shift so you don hit the same shift key as intake
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (!gameStarted && !isMatchOver)
            {
                startGame();
            }
            else
            {
                // toggle enable state manually if match is running
                SetRobotEnabled(!IsRobotEnabled);
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
        if (gameStarted) return;

        gameStarted = true;
        isMatchOver = false;

        // start the full match sequence coroutine
        matchRoutine = StartCoroutine(MatchSequence());
    }

    private IEnumerator MatchSequence()
    {
        //  PHASE 1: AUTONOMOUS 
        Debug.Log(" STARTING AUTONOMOUS (20s) ");
        isAutoPhase = true;
        PlaySound(0); // match start sfx

        SetHubStates(true, true);
        SetRobotEnabled(true);

        yield return RunTimerSegment(autoDuration, 140f);

        //  PHASE 2: AUTO DISABLE  
        Debug.Log(" END OF AUTO: DISABLING ROBOT ");
        isAutoPhase = false;
        DisableRobot();
        SetHubStates(false, false);
        PlaySound(1); // auto end sfx

        yield return new WaitForSeconds(2.0f);

        //  DETERMINE AUTO WINNER 
        bool blueWonAuto = autoScoreBlue > autoScoreRed;

        // If tied, default winner behavior 
        if (autoScoreBlue == autoScoreRed)
        {
            blueWonAuto = isBlueAlliance;
        }

        //  PHASE 3: TELEOP TRANSITION SHIFT (10s) 
        SetHubStates(true, true);
        Debug.Log(" STARTING TELEOP: TRANSITION SHIFT 1/6 (10s) ");
        PlaySound(2); // Teleop start sfx
        EnableRobot();

        yield return RunTimerSegment(teleopTransitionDuration, 130f);

        //  SHIFT 1 / ShiftName 2/6 (25s) 
        Debug.Log(" SHIFT 1 (2/6) ");
        PlaySound(3); // shift sfx
        SetHubStates(!blueWonAuto, blueWonAuto); // Auto winner is INACTIVE
        yield return RunTimerSegment(shiftDuration, 105f);

        //  SHIFT 2 / ShiftName 3/6 (25s) 
        Debug.Log(" SHIFT 2 (3/6) ");
        PlaySound(3); // shift sfx
        SetHubStates(blueWonAuto, !blueWonAuto); // Auto winner is ACTIVE
        yield return RunTimerSegment(shiftDuration, 80f);

        //  SHIFT 3 / ShiftName 4/6 (25s) 
        Debug.Log(" SHIFT 3 (4/6) ");
        PlaySound(3); // shift sfx
        SetHubStates(!blueWonAuto, blueWonAuto); // Auto winner is INACTIVE
        yield return RunTimerSegment(shiftDuration, 55f);

        //  SHIFT 4 / ShiftName 5/6 (25s) 
        Debug.Log(" SHIFT 4 (5/6) ");
        PlaySound(3); // shift sfx
        SetHubStates(blueWonAuto, !blueWonAuto); // Auto winner is ACTIVE
        yield return RunTimerSegment(shiftDuration, 30f);

        //  ENDGAME / ShiftName 6/6 (30s) 
        Debug.Log(" ENDGAME (6/6) ");
        PlaySound(4); // shift sfx
        SetHubStates(true, true); // both hubs active in endgame
        yield return RunTimerSegment(endgameDuration, 0f);

        //  PHASE 4: MATCH END 
        EndMatch();
    }

    private IEnumerator RunTimerSegment(float segmentDuration, float baseRemainingTime)
    {
        float timer = segmentDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI(timer + baseRemainingTime);
            yield return null;
        }
    }

    private void SetHubStates(bool blueActive, bool redActive)
    {
        isBlueHubActive = blueActive;
        isRedHubActive = redActive;

        // Direct, state-driven material updates (no toggling room for error!)
        if (BlueHub != null) BlueHub.SetActive(isBlueHubActive);
        if (RedHub != null) RedHub.SetActive(isRedHubActive);

        // Update UI indicator objects
        if (blueShift != null) blueShift.SetActive(isBlueHubActive);
        if (redShift != null) redShift.SetActive(isRedHubActive);
    }   

    private void EndMatch()
    {
        Debug.Log(" MATCH FINISHED ");
        DisableRobot();
        SetHubStates(false, false);
        isMatchOver = true;
        gameStarted = false;

        PlaySound(5); // match end sfx

        UpdateTimerUI(0f);
    }

    // Handle scoring
    public void scorePoint(bool isBlue)
    {
        if (!gameStarted || isMatchOver) return;

        if (isBlue)
        {
            if (!isBlueHubActive) return; // Ignore score if Hub is inactive!

            scoreBlue += 1;
            if (isAutoPhase)
            {
                autoScoreBlue += 1;
            }
        }
        else
        {
            if (!isRedHubActive) return; // Ignore score if Hub is inactive!

            scoreRed += 6;
            if (isAutoPhase)
            {
                autoScoreRed += 6;
            }
        }

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (blueTMPObject != null) blueTMPObject.text = scoreBlue.ToString();
        if (redTMPObject != null) redTMPObject.text = scoreRed.ToString();
    }

    private void UpdateTimerUI(float timeDisplay)
    {
        if (timer == null) return;

        int minutes = Mathf.FloorToInt(timeDisplay / 60F);
        int seconds = Mathf.FloorToInt(timeDisplay % 60F);
        timer.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    private void PlaySound(int index)
    {
        if (fieldSFX != null && soundEffects != null && index < soundEffects.Count && soundEffects[index] != null)
        {
            fieldSFX.PlayOneShot(soundEffects[index]);
        }
    }
}