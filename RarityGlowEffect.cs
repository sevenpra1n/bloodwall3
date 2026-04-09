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

    public void Initialize(Rarity rarity)
    {
        rarityIndex = (int)rarity;
        if (rarityIndex < 0 || rarityIndex >= ColorA.Length) rarityIndex = 0;

        // Create a child Image that sits behind the icon and acts as the border ring
        GameObject borderGO = new GameObject("RarityBorder");
        borderGO.transform.SetParent(transform, false);
        borderGO.transform.SetAsFirstSibling();

        borderImage = borderGO.AddComponent<Image>();
        borderImage.color = ColorA[rarityIndex];
        borderImage.raycastTarget = false;

        RectTransform parentRect = GetComponent<RectTransform>();
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();

        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-BorderSize, -BorderSize);
        borderRect.offsetMax = new Vector2(BorderSize, BorderSize);

        // Mask: create an inner transparent cutout so only the edge is visible
        // We achieve this with a second solid Image that covers the centre and
        // matches the icon area exactly, drawn on top of borderImage.
        GameObject maskGO = new GameObject("RarityBorderMask");
        maskGO.transform.SetParent(transform, false);
        maskGO.transform.SetSiblingIndex(1);

        Image maskImage = maskGO.AddComponent<Image>();
        // Match the background colour of the slot (transparent) so the centre looks clear.
        maskImage.color = new Color(0f, 0f, 0f, 0f);
        maskImage.raycastTarget = false;

        RectTransform maskRect = maskGO.GetComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;

        phase = (GetInstanceID() % 100) * 0.0628f;
    }

    private void Update()
    {
        if (borderImage == null) return;

        phase += Time.deltaTime * (Mathf.PI * 2f / ShimmerSpeed);
        float t = (Mathf.Sin(phase) + 1f) * 0.5f;   // 0..1

        borderImage.color = Color.Lerp(ColorA[rarityIndex], ColorB[rarityIndex], t);
    }
}
