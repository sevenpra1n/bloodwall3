using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach to the arrow Image GameObject in BattleScene.
/// BattleManager calls Launch() to animate the arrow flying from start to end position.
/// The arrow is hidden before launch, visible during flight with rotation, and fades out on impact.
/// </summary>
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private Image arrowImage;
    [SerializeField] private float travelDuration = 0.45f;
    [SerializeField] private float fadeDuration = 0.15f;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (arrowImage == null)
            arrowImage = GetComponent<Image>();

        rectTransform = arrowImage != null
            ? arrowImage.GetComponent<RectTransform>()
            : GetComponent<RectTransform>();

        // Hide arrow before launch
        if (arrowImage != null)
        {
            Color c = arrowImage.color;
            c.a = 0f;
            arrowImage.color = c;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Animates the arrow from startLocalPos to endLocalPos, then hides it.
    /// Arrow starts invisible, becomes fully visible on launch, rotates from 0° to -15°
    /// during flight, then fades out on impact.
    /// </summary>
    public IEnumerator Launch(Vector3 startLocalPos, Vector3 endLocalPos)
    {
        if (rectTransform == null) yield break;

        rectTransform.localPosition = startLocalPos;

        gameObject.SetActive(true);

        // Show arrow at start of flight (alpha = 1)
        if (arrowImage != null)
        {
            Color c = arrowImage.color;
            c.a = 1f;
            arrowImage.color = c;
        }

        // Reset rotation to point right (Z = 0°)
        rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / travelDuration;
            float eased = Mathf.SmoothStep(0f, 1f, progress);

            // Move arrow along the path
            rectTransform.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, eased);

            // Rotate arrow: start at 0°, tilt down to -15° by end of flight
            float rotation = Mathf.Lerp(0f, -15f, eased);
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotation);

            yield return null;
        }

        rectTransform.localPosition = endLocalPos;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, -15f);

        // Fade out on impact
        if (arrowImage != null)
        {
            float fadeElapsed = 0f;
            Color startColor = arrowImage.color;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                arrowImage.color = c;
                yield return null;
            }
        }

        gameObject.SetActive(false);
        rectTransform.localPosition = startLocalPos;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}