using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaches to an icon's parent RectTransform and draws a thin animated
/// shimmer border whose colour is determined by the item's Rarity.
/// Call Initialize(rarity) once after the item is set up.
/// </summary>
public class RarityGlowEffect : MonoBehaviour
{
    // How many pixels wide the border ring is
    private const float BorderSize = 4f;
    // How fast the shimmer cycles (seconds per full cycle)
    private const float ShimmerSpeed = 1.8f;

    private static readonly Color[] ColorA = new Color[]
    {
        new Color(1.00f, 1.00f, 1.00f),   // Common  – white
        new Color(1.00f, 0.55f, 0.00f),   // Rare    – orange
        new Color(0.70f, 0.10f, 1.00f),   // Epic    – purple
        new Color(1.00f, 0.84f, 0.00f),   // Legendary – gold
        new Color(1.00f, 0.15f, 0.15f),   // Mythical  – red
    };

    private static readonly Color[] ColorB = new Color[]
    {
        new Color(0.55f, 0.55f, 0.55f),   // Common  – grey
        new Color(0.60f, 0.25f, 0.00f),   // Rare    – dark orange
        new Color(0.30f, 0.00f, 0.55f),   // Epic    – dark purple
        new Color(0.55f, 0.42f, 0.00f),   // Legendary – dark gold
        new Color(0.55f, 0.00f, 0.00f),   // Mythical  – dark red
    };

    private Image borderImage;
    private int rarityIndex;
    private float phase;
    private GameObject borderGO;

    public void Initialize(Rarity rarity)
    {
        // Clean up any previously created objects before re-initializing
        Cleanup();

        rarityIndex = (int)rarity;
        if (rarityIndex < 0 || rarityIndex >= ColorA.Length) rarityIndex = 0;

        // Create the border as a child of the icon so it is always attached to it,
        // regardless of where the icon sits in the hierarchy (e.g. grid root).
        // A nested Canvas with overrideSorting = true and a sortingOrder one below the
        // nearest parent Canvas renders its content BEHIND the parent Canvas content,
        // giving us the visual "glow behind the icon" without polluting the grid layout.
        borderGO = new GameObject("RarityBorder");
        borderGO.transform.SetParent(transform, false);

        Canvas borderCanvas = borderGO.AddComponent<Canvas>();
        borderCanvas.overrideSorting = true;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        borderCanvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 0) - 1;

        borderImage = borderGO.AddComponent<Image>();
        borderImage.color = ColorA[rarityIndex];
        borderImage.raycastTarget = false;

        // Stretch to fill the icon and expand outward by BorderSize on every side.
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-BorderSize, -BorderSize);
        borderRect.offsetMax = new Vector2(BorderSize, BorderSize);

        phase = (GetInstanceID() % 100) * 0.0628f;
    }

    private void Cleanup()
    {
        if (borderGO != null)
        {
            Destroy(borderGO);
            borderGO = null;
        }
        borderImage = null;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Update()
    {
        if (borderImage == null) return;

        phase += Time.deltaTime * (Mathf.PI * 2f / ShimmerSpeed);
        float t = (Mathf.Sin(phase) + 1f) * 0.5f;   // 0..1

        borderImage.color = Color.Lerp(ColorA[rarityIndex], ColorB[rarityIndex], t);
    }
}
