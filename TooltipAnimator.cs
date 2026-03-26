using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TooltipAnimator : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Text tooltipName;
    private Text tooltipDescription;
    private Text tooltipPower;
    private RectTransform rectTransform;

    private Color rarityColor;
    private float animationDuration = 0.3f;
    private Coroutine currentAnimation;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();

        Text[] allTexts = GetComponentsInChildren<Text>();
        foreach (Text t in allTexts)
        {
            if (t.name == "TooltipName" || t.name.Contains("Name"))
                tooltipName = t;
            else if (t.name == "TooltipDescription" || t.name.Contains("Description"))
                tooltipDescription = t;
            else if (t.name == "TooltipPower" || t.name.Contains("Power"))
                tooltipPower = t;
        }

        canvasGroup.alpha = 0;
        rectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);

        if (tooltipDescription != null)
            tooltipDescription.color = Color.white;
        if (tooltipPower != null)
            tooltipPower.color = Color.white;
    }

    public void ShowTooltip(Rarity rarity)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        rarityColor = GetRarityColor(rarity);

        currentAnimation = StartCoroutine(ShowAnimation());
    }

    public void HideTooltip()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(HideAnimation());
    }

    private IEnumerator ShowAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
        Vector3 endScale = new Vector3(1f, 1f, 1f);

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.localScale = endScale;

        StartCoroutine(PulseNameColor());
    }

    private IEnumerator HideAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = new Vector3(0.8f, 0.8f, 1f);

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rectTransform.localScale = endScale;
    }

    private IEnumerator PulseNameColor()
    {
        while (canvasGroup.alpha > 0.9f)
        {
            float time = 0f;
            float pulseDuration = 1.5f;

            while (time < pulseDuration)
            {
                time += Time.deltaTime;

                float pulse = (Mathf.Sin(time / pulseDuration * Mathf.PI) + 1f) / 2f;
                Color pulseColor = Color.Lerp(Color.white, rarityColor, pulse);

                if (tooltipName != null)
                    tooltipName.color = pulseColor;

                yield return null;
            }
        }
    }

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Rare:
                return new Color(1f, 0.6f, 0f);
            case Rarity.Epic:
                return new Color(0.8f, 0.2f, 1f);
            case Rarity.Legendary:
                return new Color(1f, 0.84f, 0f);
            case Rarity.Mythical:
                return new Color(1f, 0.2f, 0.2f);
            default:
                return Color.white;
        }
    }
}