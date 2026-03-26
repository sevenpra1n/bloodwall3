using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] attackSprites;
    [SerializeField] private float frameDelay = 0.1f;
    [SerializeField] private float rushDistance = 100f;
    [SerializeField] private float rushSpeed = 0.3f;
    [SerializeField] private bool isEnemy = false;

    private Coroutine idleCoroutine;
    private bool isAttacking = false;
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    private void Start()
    {
        rectTransform = characterImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.localPosition;
        }

        if (characterImage != null && idleSprites.Length > 0)
        {
            characterImage.sprite = idleSprites[0];
        }

        idleCoroutine = StartCoroutine(PlayIdleAnimation());
    }

    private IEnumerator PlayIdleAnimation()
    {
        while (!isAttacking)
        {
            for (int i = 0; i < idleSprites.Length; i++)
            {
                if (isAttacking) break;

                if (characterImage != null && idleSprites[i] != null)
                    characterImage.sprite = idleSprites[i];
                yield return new WaitForSeconds(frameDelay);
            }
        }
    }

    public IEnumerator PlayAttackAnimation()
    {
        if (characterImage == null || attackSprites == null || attackSprites.Length == 0)
            yield break;

        isAttacking = true;

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }

        float direction = isEnemy ? -rushDistance : rushDistance;
        yield return StartCoroutine(MoveCharacter(direction, rushSpeed));

        for (int i = 0; i < attackSprites.Length; i++)
        {
            if (attackSprites[i] != null)
                characterImage.sprite = attackSprites[i];
            yield return new WaitForSeconds(frameDelay);
        }

        yield return StartCoroutine(MoveCharacter(-direction, rushSpeed));

        isAttacking = false;

        if (idleSprites.Length > 0 && idleSprites[0] != null)
            characterImage.sprite = idleSprites[0];

        idleCoroutine = StartCoroutine(PlayIdleAnimation());
    }

    public IEnumerator PlayUltimateAnimation()
    {
        if (characterImage == null || attackSprites == null || attackSprites.Length == 0)
            yield break;

        isAttacking = true;

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }

        float direction = isEnemy ? -rushDistance : rushDistance;
        yield return StartCoroutine(MoveCharacterFast(direction, 0.15f));

        for (int i = 0; i < attackSprites.Length; i++)
        {
            if (attackSprites[i] != null)
                characterImage.sprite = attackSprites[i];
            yield return new WaitForSeconds(frameDelay * 0.7f);
        }

        yield return StartCoroutine(MoveCharacterFast(-direction, 0.15f));

        isAttacking = false;

        if (idleSprites.Length > 0 && idleSprites[0] != null)
            characterImage.sprite = idleSprites[0];

        idleCoroutine = StartCoroutine(PlayIdleAnimation());
    }

    private IEnumerator MoveCharacter(float distance, float duration)
    {
        if (rectTransform == null)
            yield break;

        float elapsed = 0f;
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(distance, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        rectTransform.localPosition = endPos;
    }

    private IEnumerator MoveCharacterFast(float distance, float duration)
    {
        if (rectTransform == null)
            yield break;

        float elapsed = 0f;
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(distance, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float easeProgress = progress * progress;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, easeProgress);
            yield return null;
        }

        rectTransform.localPosition = endPos;
    }

    public void ResetColor()
    {
        if (characterImage != null)
        {
            Color resetColor = characterImage.color;
            resetColor.a = 1;
            characterImage.color = resetColor;
        }
    }

    public IEnumerator DamageFlash()
    {
        if (characterImage == null)
            yield break;

        Color originalColor = characterImage.color;
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float flash = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 4));

            characterImage.color = Color.Lerp(originalColor, new Color(1, 0, 0, 1), flash);
            yield return null;
        }

        characterImage.color = originalColor;
    }

    public IEnumerator CritFlash()
    {
        if (characterImage == null)
            yield break;

        Color originalColor = characterImage.color;
        Vector3 originalScale = characterImage.transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float flash = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 3));
            characterImage.color = Color.Lerp(originalColor, new Color(1, 1, 0, 1), flash);

            float pulseScale = 1 + (Mathf.Sin(progress * Mathf.PI * 3) * 0.2f);
            characterImage.transform.localScale = originalScale * pulseScale;

            yield return null;
        }

        characterImage.color = originalColor;
        characterImage.transform.localScale = originalScale;
    }

    // ← ДОБАВЛЕНЫ НОВЫЕ МЕТОДЫ
    public void SetIdleSprites(Sprite[] newIdleSprites)
    {
        if (newIdleSprites != null && newIdleSprites.Length > 0)
        {
            idleSprites = newIdleSprites;
            if (characterImage != null)
            {
                characterImage.sprite = idleSprites[0];
                Debug.Log("✅ Idle спрайты обновлены: " + newIdleSprites.Length + " шт");
            }
        }
    }

    public void SetAttackSprites(Sprite[] newAttackSprites)
    {
        if (newAttackSprites != null && newAttackSprites.Length > 0)
        {
            attackSprites = newAttackSprites;
            Debug.Log("✅ Attack спрайты обновлены: " + newAttackSprites.Length + " шт");
        }
    }
}