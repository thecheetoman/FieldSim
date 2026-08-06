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

    public void NextRobot()
    {
        Robots[RIndex].SetActive(false);
        RIndex++;
        if (RIndex >= Robots.Count) {
            RIndex = 0;
        }
        if (RIndex >= Robots.Count)
        {
            RIndex = 0;
        }
        Robots[RIndex].SetActive(true);
        descText.text = RobotNames[RIndex];
    }
    public void PrevRobot() {
        Robots[RIndex].SetActive(false);
        RIndex--;
        if (RIndex >= Robots.Count) {
            RIndex = 0;
        }
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

        // Load the main game scene
        SceneManager.LoadScene("Field");
    }

    private IEnumerator FadeTransition(GameObject hide, GameObject show)
    {
        CanvasGroup hideGroup = hide.GetComponent<CanvasGroup>();
        CanvasGroup showGroup = show.GetComponent<CanvasGroup>();

        // Ensure show is active but invisible
        show.SetActive(true);
        showGroup.alpha = 0f;

        float elapsed = 0f;

        // Fade out current
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            hideGroup.alpha = 1f - t;
            showGroup.alpha = t;

            yield return null;
        }

        // Finalize
        hideGroup.alpha = 0f;
        showGroup.alpha = 1f;
        hide.SetActive(false);
    }
}
