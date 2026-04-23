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

        // Create a sibling Image that sits BEHIND the icon and acts as the border ring.
        // It must be a sibling (not a child) because in Unity UI children always render
        // on top of their parent, which would put the border over the icon.
        Transform iconParent = transform.parent != null ? transform.parent : transform;
        borderGO = new GameObject("RarityBorder");
        borderGO.transform.SetParent(iconParent, false);
        // Place the border just before this icon in the sibling list so it renders behind it.
        borderGO.transform.SetSiblingIndex(transform.GetSiblingIndex());

        borderImage = borderGO.AddComponent<Image>();
        borderImage.color = ColorA[rarityIndex];
        borderImage.raycastTarget = false;

        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        // Mirror the icon's anchors so the border tracks the icon's position.
        RectTransform iconRect = GetComponent<RectTransform>();
        if (iconRect != null)
        {
            borderRect.anchorMin = iconRect.anchorMin;
            borderRect.anchorMax = iconRect.anchorMax;
            borderRect.anchoredPosition = iconRect.anchoredPosition;
            borderRect.sizeDelta = iconRect.sizeDelta;
        }
        else
        {
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
        }
        borderRect.offsetMin -= new Vector2(BorderSize, BorderSize);
        borderRect.offsetMax += new Vector2(BorderSize, BorderSize);

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
