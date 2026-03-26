using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float loadingDuration = 4f;

    public IEnumerator TransitionToScene(string sceneName)
    {
        // Затухание звука
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            yield return StartCoroutine(audioManager.FadeOutAudio(fadeDuration));
        }

        // Плавное затемнение
        yield return StartCoroutine(FadeIn(fadeDuration));

        // Loading экран
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(loadingDuration);

        // Загружаем сцену
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadePanel == null)
            yield break;

        float elapsed = 0f;
        Color startColor = fadePanel.color;
        startColor.a = 0;
        Color endColor = fadePanel.color;
        endColor.a = 1;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            fadePanel.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        fadePanel.color = endColor;
    }

    public IEnumerator FadeOut(float duration)
    {
        if (fadePanel == null)
            yield break;

        float elapsed = 0f;
        Color startColor = fadePanel.color;
        startColor.a = 1;
        Color endColor = fadePanel.color;
        endColor.a = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            fadePanel.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        fadePanel.color = endColor;
    }
}