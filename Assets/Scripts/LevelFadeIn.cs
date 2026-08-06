using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LevelFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Start fully visible
        canvasGroup.alpha = 1f;
    }

    private void OnEnable()
    {
        // Start fade-out whenever the object is activated
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}