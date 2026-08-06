using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject InitialMenu;
    [SerializeField] private GameObject Credits;
    [SerializeField] private GameObject Settings;
    [SerializeField] private GameObject chooseMenu;
    [SerializeField] private GameObject blackFade;
    [SerializeField] private GameObject controlsScreen;
    [SerializeField] private TextMeshProUGUI descText;
    public List<GameObject> Robots = new List<GameObject>();
    public List<string> RobotNames = new List<string>();
    private int RIndex = 0;

    [SerializeField] private float fadeDuration = 0.1f;

    public void ShowSettings()
    {
        StartCoroutine(FadeTransition(InitialMenu, Settings));
    }

    public void HideSettings()
    {
        StartCoroutine(FadeTransition(Settings, InitialMenu));
    }

    public void StartGame()
    {
        StartCoroutine(FadeTransition(InitialMenu, chooseMenu));
    }

    public void gameBackMenu()
    {
        StartCoroutine(FadeTransition(chooseMenu, InitialMenu));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void showCredits()
    {
        StartCoroutine(FadeTransition(InitialMenu, Credits));
    }

    public void backCredits()
    {
        StartCoroutine(FadeTransition(Credits, InitialMenu));
    }
    public void showControls()
    {
        StartCoroutine(FadeTransition(InitialMenu, controlsScreen));
    }

    public void hideControls()
    {
        StartCoroutine(FadeTransition(controlsScreen, InitialMenu));
    }

    public void NextRobot()
    {
        Robots[RIndex].SetActive(false);
        RIndex++;
        if (RIndex >= Robots.Count)
        {
            RIndex = 0;
        }
        Robots[RIndex].SetActive(true);
        descText.text = RobotNames[RIndex];
    }

    public void PrevRobot()
    {
        Robots[RIndex].SetActive(false);
        RIndex--;
        if (RIndex < 0)
        {
            RIndex = Robots.Count - 1;
        }
        Robots[RIndex].SetActive(true);
        descText.text = RobotNames[RIndex];
    }

    public void PlayGame()
    {
        // Save choice so the Field scene can read it
        PlayerPrefs.SetInt("SelectedRobotIndex", RIndex);
        PlayerPrefs.Save();

        // Start the fade to black before loading the scene
        StartCoroutine(FadeToBlackAndLoadScene("Field"));
    }

    private IEnumerator FadeTransition(GameObject hide, GameObject show)
    {
        CanvasGroup hideGroup = hide.GetComponent<CanvasGroup>();
        CanvasGroup showGroup = show.GetComponent<CanvasGroup>();

        // Ensure show is active but invisible
        show.SetActive(true);
        showGroup.alpha = 0f;

        float elapsed = 0f;

        // Fade out current and fade in target
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (hideGroup != null) hideGroup.alpha = 1f - t;
            if (showGroup != null) showGroup.alpha = t;

            yield return null;
        }

        // Finalize
        if (hideGroup != null) hideGroup.alpha = 0f;
        if (showGroup != null) showGroup.alpha = 1f;
        hide.SetActive(false);
    }

    private IEnumerator FadeToBlackAndLoadScene(string sceneName)
    {
        if (blackFade != null)
        {
            // Ensure the black fade object has a CanvasGroup component
            CanvasGroup fadeGroup = blackFade.GetComponent<CanvasGroup>();
            if (fadeGroup == null)
            {
                fadeGroup = blackFade.AddComponent<CanvasGroup>();
            }

            blackFade.SetActive(true);
            fadeGroup.alpha = 0f;

            float elapsed = 0f;

            // Fade the black screen in over fadeDuration
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeGroup.alpha = elapsed / fadeDuration;
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        // Load the new scene after the screen is completely black
        SceneManager.LoadScene(sceneName);
    }
}