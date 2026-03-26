using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KnightAnimator : MonoBehaviour
{
    [SerializeField] public Image characterImage;
    [SerializeField] private float frameDelay = 0.15f;

    private Sprite[] currentIdleSprites;
    private Coroutine idleCoroutine;

    private void Awake()  // ← ИЗМЕНИ НА AWAKE!
    {
        if (characterImage == null)
            characterImage = GetComponent<Image>();
    }

    public void SetIdleSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;

        currentIdleSprites = sprites;
        StopIdleAnimation();

        // ← СРАЗУ ПОКАЗАТЬ ПЕРВЫЙ КАДР!
        if (characterImage != null)
            characterImage.sprite = sprites[0];

        idleCoroutine = StartCoroutine(PlayIdleAnimation(sprites));
    }

    private IEnumerator PlayIdleAnimation(Sprite[] sprites)
    {
        int frame = 0;
        while (true)
        {
            if (characterImage != null && sprites != null && sprites.Length > 0)
                characterImage.sprite = sprites[frame % sprites.Length];

            yield return new WaitForSeconds(frameDelay);
            frame++;
        }
    }

    private void StopIdleAnimation()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopIdleAnimation();
    }
}