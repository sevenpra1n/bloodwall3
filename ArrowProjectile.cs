using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach to the arrow Image GameObject in BattleScene.
/// BattleManager calls Launch() to animate the arrow flying right.
/// </summary>
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private Image arrowImage;
    [SerializeField] private float travelDuration = 0.45f;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (arrowImage == null)
            arrowImage = GetComponent<Image>();

        rectTransform = arrowImage != null
            ? arrowImage.GetComponent<RectTransform>()
            : GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Animates the arrow from startLocalPos to endLocalPos, then hides it.
    /// </summary>
    public IEnumerator Launch(Vector3 startLocalPos, Vector3 endLocalPos)
    {
        if (rectTransform == null) yield break;

        rectTransform.localPosition = startLocalPos;
        gameObject.SetActive(true);

        // Make sure the arrow is visible
        if (arrowImage != null)
        {
            Color c = arrowImage.color;
            c.a = 1f;
            arrowImage.color = c;
        }

        float elapsed = 0f;
        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / travelDuration;
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            rectTransform.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, eased);
            yield return null;
        }

        rectTransform.localPosition = endLocalPos;

        // Fade out briefly on impact
        if (arrowImage != null)
        {
            float fadeDuration = 0.15f;
            float fadeElapsed = 0f;
            Color start = arrowImage.color;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                Color c = start;
                c.a = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                arrowImage.color = c;
                yield return null;
            }
        }

        gameObject.SetActive(false);
        rectTransform.localPosition = startLocalPos;

        if (arrowImage != null)
        {
            Color c = arrowImage.color;
            c.a = 1f;
            arrowImage.color = c;
        }
    }
}