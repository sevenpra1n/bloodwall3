using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource menuAudioSource;

    private void Start()
    {
        if (menuAudioSource != null)
        {
            if (!menuAudioSource.isPlaying)
            {
                menuAudioSource.Play();
            }
        }
    }

    public IEnumerator FadeOutAudio(float duration)
    {
        if (menuAudioSource == null)
            yield break;

        float startVolume = menuAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            menuAudioSource.volume = Mathf.Lerp(startVolume, 0, progress);
            yield return null;
        }

        menuAudioSource.volume = 0;
        menuAudioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        if (menuAudioSource != null)
        {
            menuAudioSource.volume = Mathf.Clamp01(volume);
        }
    }
}