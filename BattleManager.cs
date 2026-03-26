using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Image enemyVisualImage;

    [SerializeField] private Button attackButton;
    [SerializeField] private Image attackButtonImage;
    [SerializeField] private Image[] ultimateBarTextures = new Image[6];

    [SerializeField] private Button[] skillButtons = new Button[4];
    [SerializeField] private Image skillImage;

    [SerializeField] private Sprite attackSpriteNormal;
    [SerializeField] private Sprite skillSpriteNormal;
    [SerializeField] private Sprite skillSpriteHover;
    [SerializeField] private Sprite skillSpritePressed;
    [SerializeField] private Sprite skillSpriteCooldown;
    [SerializeField] private Sprite skillSpriteNoMana;
    [SerializeField] private Sprite lockedSkillSprite;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Text battleLog;
    [SerializeField] private Text battleStartText;

    [SerializeField] private Image playerCritImage;
    [SerializeField] private Image playerMissImage;
    [SerializeField] private Image enemyCritImage;
    [SerializeField] private Image enemyMissImage;

    [SerializeField] private Image[] playerHPBars = new Image[9];
    [SerializeField] private Image[] enemyHPBars = new Image[9];
    [SerializeField] private Image[] bossHPBars = new Image[10];

    [SerializeField] private Text enemyHPText;

    [SerializeField] private AnimationController playerAnimController;
    [SerializeField] private AnimationController enemyAnimController;

    [SerializeField] private Image fadePanel;
    [SerializeField] private PotionSystem potionSystem;

    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private AudioClip playerAttackSound;
    [SerializeField] private AudioClip playerAttackEndSound;
    [SerializeField] private AudioClip enemyAttackSound;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip critSound;
    [SerializeField] private AudioClip missSound;
    [SerializeField] private AudioClip ultimateUseSound;

    [SerializeField] private Text playerPowerText;
    [SerializeField] private Text enemyPowerText;

    [SerializeField] private Text coinsRewardText;
    [SerializeField] private Text expRewardText;
    [SerializeField] private Button continueWinButton;

    [SerializeField] private Text expLoseText;
    [SerializeField] private Button continueLoseButton;

    [SerializeField] private Image screenFlashRed;
    [SerializeField] private RectTransform cameraShakeTarget;

    [SerializeField] private StageData[] stagesData = new StageData[5];

    // Arrow projectile for Archer skill (assign the ArrowProjectile component in Inspector)
    [SerializeField] private ArrowProjectile arrowProjectile;
    [SerializeField] private RectTransform arrowStartPoint;  // position near the player
    [SerializeField] private RectTransform arrowEndPoint;    // position near the enemy

    private bool isPlayerTurn = true;
    private bool battleActive = true;
    private bool isAnimating = false;
    private AudioSource audioSource;
    private AudioSource musicSource;

    private bool skillIsHovered = false;
    private bool skillIsPressed = false;

    private int playerPower;
    private int enemyPower;
    private int currentStage = 0;

    private int ultimateCharge = 0;
    private const int ultimateMaxCharge = 5;
    private bool isBossStage = false;

    [SerializeField] private AchievementSystem achievementSystem;

    private void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (screenFlashRed == null)
            Debug.LogError("screenFlashRed не назначен!");

        currentStage = PlayerPrefs.GetInt("CurrentStage", 0);
        playerPower = PlayerPrefs.GetInt("PlayerPower", 10);

        Debug.Log("Загружена мощь игрока: " + playerPower + ", этаж: " + currentStage);

        if (currentStage >= 0 && currentStage < stagesData.Length)
        {
            SetupEnemy(stagesData[currentStage]);
        }
        else
        {
            Debug.LogError("Неверный этаж: " + currentStage);
            enemyPower = 20;
        }

        if (playerPowerText != null)
            playerPowerText.text = playerPower.ToString();

        if (enemyPowerText != null)
            enemyPowerText.text = enemyPower.ToString();

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMenu);

        if (continueWinButton != null)
            continueWinButton.onClick.AddListener(ContinueAfterWin);

        if (continueLoseButton != null)
            continueLoseButton.onClick.AddListener(ContinueAfterLose);

        Invoke("SetupSkillButtons", 0.1f);
        attackButton.onClick.AddListener(PlayerAttackOrUltimate);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = battleMusic;
        musicSource.loop = true;
        musicSource.volume = 0;

        foreach (Image texture in ultimateBarTextures)
        {
            if (texture != null)
                texture.enabled = false;
        }

        UpdateUltimateBar();
        if (achievementSystem == null)
            achievementSystem = FindObjectOfType<AchievementSystem>();

        // ===== ЗАГРУЗИ ПЕРСОНАЖА И СКИЛЛ =====
        if (CharacterManager.Instance != null && playerAnimController != null)
        {
            CharacterData charData = CharacterManager.Instance.GetCurrentCharacter();
            if (charData != null)
            {
                // IDLE АНИМАЦИЯ
                if (charData.idleSprites != null && charData.idleSprites.Length > 0)
                {
                    playerAnimController.SetIdleSprites(charData.idleSprites);
                }

                // ATTACK АНИМАЦИЯ
                if (charData.attackSprites != null && charData.attackSprites.Length > 0)
                {
                    playerAnimController.SetAttackSprites(charData.attackSprites);
                }
            }
        }

        // ===== ЗАГРУЗИ СКИЛЛ СПРАЙТЫ =====
        if (CharacterManager.Instance != null && skillImage != null)
        {
            CharacterData charData = CharacterManager.Instance.GetCurrentCharacter();
            if (charData != null)
            {
                // ОСНОВНАЯ ИКОНКА СКИЛЛА
                if (charData.skillIcon != null)
                    skillImage.sprite = charData.skillIcon;

                // ЗАГРУЗИ СПРАЙТЫ СКИЛЛА ДЛЯ КНОПКИ
                Button skillButton = skillImage.GetComponent<Button>();
                if (skillButton != null)
                {
                    SpriteState spriteState = new SpriteState();

                    if (charData.skillSpritesHover != null && charData.skillSpritesHover.Length > 0)
                        spriteState.highlightedSprite = charData.skillSpritesHover[0];

                    if (charData.skillSpritesPressed != null && charData.skillSpritesPressed.Length > 0)
                        spriteState.pressedSprite = charData.skillSpritesPressed[0];

                    if (charData.skillSpritesCooldown != null && charData.skillSpritesCooldown.Length > 0)
                        spriteState.disabledSprite = charData.skillSpritesCooldown[0];

                    skillButton.spriteState = spriteState;
                }
            }
        }

        StartCoroutine(StartBattle());
    }

    private void SetupEnemy(StageData stageData)
    {
        if (enemy == null) return;

        enemyPower = (int)stageData.enemyData.power;
        enemy.SetPower(stageData.enemyData.power);

        // ← РАЗДЕЛИ НА 100!
        enemy.SetEnemyParameters(
            stageData.enemyData.power,
            stageData.enemyData.missChance / 100f,  // ← ВОТ ЗДЕСЬ!
            stageData.enemyData.critChance / 100f   // ← И ЗДЕСЬ!
        );

        isBossStage = stageData.isBossStage;

        // ← ПЕРЕДАЁМ СПРАЙТЫ В AnimationController
        if (enemyAnimController != null && stageData.enemyData.idleSprites != null && stageData.enemyData.idleSprites.Length > 0)
        {
            enemyAnimController.SetIdleSprites(stageData.enemyData.idleSprites);
        }

        if (enemyAnimController != null && stageData.enemyData.attackSprites != null && stageData.enemyData.attackSprites.Length > 0)
        {
            enemyAnimController.SetAttackSprites(stageData.enemyData.attackSprites);
        }

        // ← ПОКАЗЫВАЕМ/СКРЫВАЕМ HP БАРЫ
        foreach (Image bar in enemyHPBars)
            if (bar != null) bar.enabled = false;

        foreach (Image bar in bossHPBars)
            if (bar != null) bar.enabled = false;

        if (enemyHPText != null)
        {
            enemyHPText.enabled = !isBossStage;
        }

        if (isBossStage)
        {
            foreach (Image bar in bossHPBars)
                if (bar != null) bar.enabled = true;
            Debug.Log("🔴 BOSS STAGE! Показываю 10-секционный HP бар");
        }
        else
        {
            foreach (Image bar in enemyHPBars)
                if (bar != null) bar.enabled = true;
            Debug.Log("👹 NORMAL STAGE! Показываю обычный HP бар");
        }

        Debug.Log("Враг установлен: " + stageData.enemyData.enemyName + " (Boss: " + isBossStage + ")");
    }

    private void SetupSkillButtons()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;
            skillButtons[i].onClick.AddListener(() => PlayerUseSkill(index));
        }

        if (skillImage != null)
        {
            skillImage.sprite = lockedSkillSprite;
            skillImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }

        for (int i = 1; i < skillButtons.Length; i++)
        {
            Image buttonImage = skillButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = lockedSkillSprite;
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
        }

        EventTrigger skillTrigger = skillButtons[0].gameObject.GetComponent<EventTrigger>();
        if (skillTrigger == null)
            skillTrigger = skillButtons[0].gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnSkillEnter());
        skillTrigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => OnSkillExit());
        skillTrigger.triggers.Add(entryExit);

        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => OnSkillDown());
        skillTrigger.triggers.Add(entryDown);

        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => OnSkillUp());
        skillTrigger.triggers.Add(entryUp);

        UpdateSkillUI();
    }

    private void OnSkillEnter()
    {
        if (isAnimating) return;
        skillIsHovered = true;
        UpdateSkillImage();
    }

    private void OnSkillExit()
    {
        if (isAnimating) return;
        skillIsHovered = false;
        skillIsPressed = false;
        UpdateSkillImage();
    }

    private void OnSkillDown()
    {
        if (isAnimating) return;
        skillIsPressed = true;
        UpdateSkillImage();
    }

    private void OnSkillUp()
    {
        if (isAnimating) return;
        skillIsPressed = false;
        UpdateSkillImage();
    }

    private void UpdateSkillImage()
    {
        if (skillImage == null) return;

        if (player.IsSkillLocked(0))
        {
            skillImage.sprite = lockedSkillSprite;
            skillImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }
        else if (!player.CanUseSkill(0))
        {
            skillImage.sprite = skillSpriteNoMana;
            skillImage.color = new Color(1f, 0.5f, 0.5f, 1f);
        }
        else
        {
            if (skillIsPressed)
            {
                skillImage.sprite = skillSpritePressed;
                skillImage.color = Color.white;
            }
            else if (skillIsHovered)
            {
                skillImage.sprite = skillSpriteHover;
                skillImage.color = Color.white;
            }
            else
            {
                skillImage.sprite = skillSpriteNormal;
                skillImage.color = Color.white;
            }
        }
    }

    private void UpdateUltimateBar()
    {
        foreach (Image texture in ultimateBarTextures)
        {
            if (texture != null)
                texture.enabled = false;
        }

        if (ultimateCharge > 0 && ultimateCharge < ultimateBarTextures.Length && ultimateBarTextures[ultimateCharge] != null)
            ultimateBarTextures[ultimateCharge].enabled = true;
    }

    public void UpdateSkillUI()
    {
        UpdateSkillImage();
        skillButtons[0].interactable = !player.IsSkillLocked(0) && player.CanUseSkill(0);

        for (int i = 1; i < skillButtons.Length; i++)
        {
            Image buttonImage = skillButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = lockedSkillSprite;
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            skillButtons[i].interactable = false;
        }
    }

    private void PlayerAttackOrUltimate()
    {
        if (!isPlayerTurn || !battleActive || isAnimating) return;

        if (ultimateCharge >= ultimateMaxCharge)
            PlayerUseUltimate();
        else
            PlayerAttack();
    }

    private void PlayerAttack()
    {
        DisablePlayerButtons();
        StartCoroutine(PlayerAttackCoroutine());
    }

    private IEnumerator PlayerAttackCoroutine()
    {
        isAnimating = true;

        PlayAttackSound(playerAttackSound);
        yield return StartCoroutine(playerAnimController.PlayAttackAnimation());

        if (player.CheckMiss())
        {
            PlaySound(missSound);
            yield return StartCoroutine(ShowEffect(playerMissImage));
        }
        else
        {
            bool isCrit = player.CheckCrit();
            if (isCrit)
            {
                PlaySound(critSound);
                yield return StartCoroutine(ShowEffect(playerCritImage));
                yield return StartCoroutine(enemyAnimController.CritFlash());
                yield return new WaitForSeconds(0.2f);
                PlayAttackSound(playerAttackEndSound);
            }
            else
            {
                yield return StartCoroutine(enemyAnimController.DamageFlash());
                PlayAttackSound(playerAttackEndSound);
            }

            player.BasicAttack(enemy);
            if (achievementSystem != null)
                achievementSystem.AddDamage(player.GetLastDamageDealt());
            StartCoroutine(HPBarShakeAll(enemyHPBars));
            StartCoroutine(BossHPBarShakeAll(bossHPBars));
        }

        AddUltimateCharge();
        isAnimating = false;
        EndPlayerTurn();
    }

    private void PlayerUseUltimate()
    {
        DisablePlayerButtons();
        StartCoroutine(PlayerUltimateCoroutine());
    }

    private IEnumerator PlayerUltimateCoroutine()
    {
        isAnimating = true;

        PlaySound(ultimateUseSound);
        yield return StartCoroutine(playerAnimController.PlayUltimateAnimation());
        yield return StartCoroutine(ShowUltimateEffect());

        float damage = player.GetPower() * 0.45f;
        if (player.WasLastAttackCrit())
            damage *= 2;

        enemy.TakeDamage(damage);
        if (achievementSystem != null)
            achievementSystem.AddDamage(damage);
        StartCoroutine(HPBarShakeAll(enemyHPBars));
        StartCoroutine(BossHPBarShakeAll(bossHPBars));

        ultimateCharge = 0;
        UpdateUltimateBar();

        isAnimating = false;
        EndPlayerTurn();
    }

    private IEnumerator ShowUltimateEffect()
    {
        Image effectImage = playerCritImage;
        if (effectImage == null) yield break;

        effectImage.gameObject.SetActive(true);
        RectTransform rect = effectImage.GetComponent<RectTransform>();
        Vector3 originalPos = rect.localPosition;

        Color orangeColor = new Color(1f, 0.6f, 0f, 1f);
        effectImage.color = orangeColor;

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            rect.localPosition = originalPos + Vector3.up * progress * 100;
            float scale = 2f - (progress * 1.5f);
            rect.localScale = new Vector3(scale, scale, scale);

            Color color = orangeColor;
            color.a = 1 - progress;
            effectImage.color = color;

            yield return null;
        }

        effectImage.gameObject.SetActive(false);
        rect.localPosition = originalPos;
        rect.localScale = Vector3.one;
    }

    private void AddUltimateCharge()
    {
        if (ultimateCharge < ultimateMaxCharge)
        {
            ultimateCharge++;
            UpdateUltimateBar();
        }
    }

    private void PlayerUseSkill(int skillIndex)
    {
        if (!isPlayerTurn || !battleActive || isAnimating) return;
        if (!player.CanUseSkill(skillIndex)) return;

        DisablePlayerButtons();
        StartCoroutine(PlayerSkillCoroutine(skillIndex));
    }

    private IEnumerator PlayerSkillCoroutine(int skillIndex)
    {
        isAnimating = true;

        PlayAttackSound(playerAttackSound);
        yield return StartCoroutine(playerAnimController.PlayAttackAnimation());

        // For the Archer's arrow skill, play the arrow flight animation before damage
        bool isArrow = player.IsArrowSkill(skillIndex);
        if (isArrow && arrowProjectile != null && arrowStartPoint != null && arrowEndPoint != null)
        {
            yield return StartCoroutine(arrowProjectile.Launch(
                arrowStartPoint.localPosition,
                arrowEndPoint.localPosition));
        }

        bool isMiss = player.CheckMiss();
        bool isCrit = player.CheckCrit();

        if (isMiss)
        {
            PlaySound(missSound);
            yield return StartCoroutine(ShowEffect(playerMissImage));
        }
        else
        {
            if (isCrit)
            {
                PlaySound(critSound);
                yield return StartCoroutine(ShowEffect(playerCritImage));
                yield return StartCoroutine(enemyAnimController.CritFlash());
                yield return new WaitForSeconds(0.2f);
                PlayAttackSound(playerAttackEndSound);
            }
            else
            {
                yield return StartCoroutine(enemyAnimController.DamageFlash());
                PlayAttackSound(playerAttackEndSound);
            }

            player.UseSkill(skillIndex, enemy);
            if (achievementSystem != null)
                achievementSystem.AddDamage(player.GetLastDamageDealt());
            StartCoroutine(HPBarShakeAll(enemyHPBars));
            StartCoroutine(BossHPBarShakeAll(bossHPBars));
        }

        AddUltimateCharge();
        isAnimating = false;
        EndPlayerTurn();
    }

    private void EndPlayerTurn()
    {
        if (!enemy.IsAlive())
        {
            ShowWin();
            return;
        }

        isPlayerTurn = false;
        Invoke("EnemyTurn", 1f);
    }

    private void EnemyTurn()
    {
        if (!battleActive || isAnimating) return;
        StartCoroutine(EnemyAttackCoroutine());
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        isAnimating = true;

        yield return StartCoroutine(enemyAnimController.PlayAttackAnimation());
        PlayAttackSound(playerAttackSound);

        if (enemy.CheckMiss())
        {
            PlaySound(missSound);
            yield return StartCoroutine(ShowEffect(enemyMissImage));
            if (achievementSystem != null)
                achievementSystem.AddMiss();
        }
        else
        {
            bool isCrit = enemy.CheckCrit();
            if (isCrit)
            {
                PlaySound(critSound);
                yield return StartCoroutine(ShowEffect(enemyCritImage));
                yield return StartCoroutine(playerAnimController.CritFlash());
                yield return new WaitForSeconds(0.2f);
                PlayAttackSound(enemyAttackSound);
            }
            else
            {
                yield return StartCoroutine(playerAnimController.DamageFlash());
                PlayAttackSound(enemyAttackSound);
            }

            enemy.BasicAttack(player);
            StartCoroutine(HPBarShakeAll(playerHPBars));
        }

        if (!player.IsAlive())
        {
            ShowLose();
            isAnimating = false;
            yield break;
        }

        isAnimating = false;
        isPlayerTurn = true;
        EnablePlayerButtons();
    }

    private void PlayAttackSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, 0.7f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, 0.7f);
    }

    private void DisablePlayerButtons()
    {
        attackButton.interactable = false;
        for (int i = 0; i < skillButtons.Length; i++)
            skillButtons[i].interactable = false;

        if (attackButtonImage != null)
            attackButtonImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        for (int i = 0; i < skillButtons.Length; i++)
        {
            Image img = skillButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }

        skillImage.sprite = skillSpriteCooldown;
        skillImage.color = new Color(1f, 1f, 1f, 0.6f);

        potionSystem.DisableButtons();
    }

    private void EnablePlayerButtons()
    {
        attackButton.interactable = true;
        if (attackButtonImage != null)
            attackButtonImage.color = Color.white;

        UpdateSkillUI();
        potionSystem.EnableButtons();
    }

    private IEnumerator HPBarShakeAll(Image[] hpBars)
    {
        if (hpBars == null || hpBars.Length == 0) yield break;

        Vector3[] originalPositions = new Vector3[hpBars.Length];
        Color[] originalColors = new Color[hpBars.Length];
        RectTransform[] rects = new RectTransform[hpBars.Length];

        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] != null)
            {
                rects[i] = hpBars[i].GetComponent<RectTransform>();
                if (rects[i] != null)
                {
                    originalPositions[i] = rects[i].localPosition;
                    originalColors[i] = hpBars[i].color;
                }
            }
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            for (int i = 0; i < hpBars.Length; i++)
            {
                if (hpBars[i] != null && rects[i] != null)
                {
                    float shakeAmount = (1 - progress) * 10;
                    rects[i].localPosition = originalPositions[i] + new Vector3(
                        Random.Range(-shakeAmount, shakeAmount),
                        Random.Range(-shakeAmount, shakeAmount),
                        0
                    );

                    Color redColor = new Color(1, 0.3f, 0.3f, 1);
                    hpBars[i].color = Color.Lerp(redColor, originalColors[i], progress);
                }
            }

            yield return null;
        }

        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] != null && rects[i] != null)
            {
                rects[i].localPosition = originalPositions[i];
                hpBars[i].color = originalColors[i];
            }
        }
    }

    private IEnumerator BossHPBarShakeAll(Image[] hpBars)
    {
        if (hpBars == null || hpBars.Length == 0) yield break;

        Vector3[] originalPositions = new Vector3[hpBars.Length];
        Color[] originalColors = new Color[hpBars.Length];
        RectTransform[] rects = new RectTransform[hpBars.Length];

        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] != null)
            {
                rects[i] = hpBars[i].GetComponent<RectTransform>();
                if (rects[i] != null)
                {
                    originalPositions[i] = rects[i].localPosition;
                    originalColors[i] = hpBars[i].color;
                }
            }
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            for (int i = 0; i < hpBars.Length; i++)
            {
                if (hpBars[i] != null && rects[i] != null)
                {
                    float shakeAmount = (1 - progress) * 10;
                    rects[i].localPosition = originalPositions[i] + new Vector3(
                        Random.Range(-shakeAmount, shakeAmount),
                        Random.Range(-shakeAmount, shakeAmount),
                        0
                    );

                    Color redColor = new Color(1, 0.3f, 0.3f, 1);
                    hpBars[i].color = Color.Lerp(redColor, originalColors[i], progress);
                }
            }

            yield return null;
        }

        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] != null && rects[i] != null)
            {
                rects[i].localPosition = originalPositions[i];
                hpBars[i].color = originalColors[i];
            }
        }
    }

    private IEnumerator ShowEffect(Image effectImage)
    {
        if (effectImage == null) yield break;

        effectImage.gameObject.SetActive(true);
        RectTransform rect = effectImage.GetComponent<RectTransform>();
        Vector3 originalPos = rect.localPosition;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            rect.localPosition = originalPos + Vector3.up * progress * 80;
            float scale = 1.5f - (progress * 0.5f);
            rect.localScale = new Vector3(scale, scale, scale);
            rect.rotation = Quaternion.Euler(0, 0, progress * 20);

            Color color = effectImage.color;
            color.a = 1 - progress;
            effectImage.color = color;

            yield return null;
        }

        effectImage.gameObject.SetActive(false);
        rect.localPosition = originalPos;
        rect.localScale = Vector3.one;
        rect.rotation = Quaternion.identity;

        Color resetColor = effectImage.color;
        resetColor.a = 1;
        effectImage.color = resetColor;
    }

    private IEnumerator FadeOutText(Text textElement)
    {
        float duration = 1f;
        float elapsed = 0f;
        Color originalColor = textElement.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            Color color = originalColor;
            color.a = 1 - progress;
            textElement.color = color;

            yield return null;
        }

        Color finalColor = originalColor;
        finalColor.a = 0;
        textElement.color = finalColor;
    }

    private IEnumerator FadeOut(float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color startColor = fadePanel.color;
        startColor.a = 0;
        fadePanel.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            Color newColor = fadePanel.color;
            newColor.a = Mathf.Lerp(0, 1, progress);
            fadePanel.color = newColor;
            yield return null;
        }

        Color finalColor = fadePanel.color;
        finalColor.a = 1;
        fadePanel.color = finalColor;
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color startColor = fadePanel.color;
        startColor.a = 1;
        fadePanel.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            Color newColor = fadePanel.color;
            newColor.a = Mathf.Lerp(1, 0, progress);
            fadePanel.color = newColor;
            yield return null;
        }

        Color finalColor = fadePanel.color;
        finalColor.a = 0;
        fadePanel.color = finalColor;
        fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeInMusic(float duration)
    {
        float elapsed = 0f;
        float targetVolume = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            musicSource.volume = Mathf.Lerp(0, targetVolume, progress);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator StartBattle()
    {
        fadePanel.gameObject.SetActive(true);
        Color fadeColor = fadePanel.color;
        fadeColor.a = 1;
        fadePanel.color = fadeColor;

        battleStartText.gameObject.SetActive(true);

        musicSource.Play();
        StartCoroutine(FadeInMusic(2f));

        yield return StartCoroutine(FadeIn(1.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeOutText(battleStartText));
        battleStartText.gameObject.SetActive(false);

        if (Random.value < 0.5f)
        {
            isPlayerTurn = true;
            EnablePlayerButtons();
        }
        else
        {
            isPlayerTurn = false;
            Invoke("EnemyTurn", 1f);
        }
    }

    private void ShowWin()
    {
        battleActive = false;

        int coinsReward = stagesData[currentStage].coinsReward;
        int expReward = stagesData[currentStage].expReward;

        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        PlayerPrefs.SetInt("PlayerCoins", currentCoins + coinsReward);

        // ← ДОБАВЛЯЕМ ФЛАГ И НАГРАДУ ДЛЯ АНИМАЦИИ XP
        PlayerPrefs.SetInt("JustFinishedBattle", 1);
        PlayerPrefs.SetInt("LastBattleExpReward", expReward);

        if (currentStage < 4)
        {
            PlayerPrefs.SetInt("CurrentStage", currentStage + 1);
        }
        else
        {
            PlayerPrefs.SetInt("CurrentStage", 0);
        }

        PlayerPrefs.Save();

        if (isBossStage)
        {
            StartCoroutine(BossFallAnimation(coinsReward, expReward));
        }
        else
        {
            StartCoroutine(ShowWinPanel(coinsReward, expReward));
        }
    }

    private IEnumerator BossFallAnimation(int coinsReward, int expReward)
    {
        RectTransform bossRect = enemyVisualImage.GetComponent<RectTransform>();
        if (bossRect == null) yield break;

        Vector3 originalPos = bossRect.localPosition;
        float fallDuration = 1.5f;
        float elapsed = 0f;
        float fallDistance = 500f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fallDuration;

            float fallY = -Mathf.Pow(progress, 2) * fallDistance;
            bossRect.localPosition = originalPos + new Vector3(0, fallY, 0);

            bossRect.localRotation = Quaternion.Euler(0, 0, progress * 360 * 3);

            Color bossColor = enemyVisualImage.color;
            bossColor.a = 1 - (progress * 0.7f);
            enemyVisualImage.color = bossColor;

            yield return null;
        }

        yield return StartCoroutine(ShowUltimateEffect());

        if (playerAttackSound != null)
            PlaySound(playerAttackSound);

        enemyVisualImage.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(ShowWinPanel(coinsReward * 2, expReward * 2));
    }

    private IEnumerator ShowWinPanel(int coinsReward, int expReward)
    {
        winPanel.SetActive(true);
        RectTransform winPanelRect = winPanel.GetComponent<RectTransform>();

        winPanelRect.localScale = new Vector3(0.5f, 0.5f, 1f);
        CanvasGroup winCanvasGroup = winPanel.GetComponent<CanvasGroup>();
        if (winCanvasGroup == null)
            winCanvasGroup = winPanel.AddComponent<CanvasGroup>();
        winCanvasGroup.alpha = 0;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            winPanelRect.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1f), Vector3.one, progress);
            winCanvasGroup.alpha = Mathf.Lerp(0, 1, progress);

            yield return null;
        }

        winPanelRect.localScale = Vector3.one;
        winCanvasGroup.alpha = 1;

        // ← ПОКАЗЫВАЕМ НАГРАДЫ С АНИМАЦИЕЙ
        coinsRewardText.text = "0";
        expRewardText.text = "0";

        float countDuration = 1f;
        float countElapsed = 0f;

        while (countElapsed < countDuration)
        {
            countElapsed += Time.deltaTime;
            float countProgress = countElapsed / countDuration;

            int currentCoins = Mathf.RoundToInt(coinsReward * countProgress);
            int currentExp = Mathf.RoundToInt(expReward * countProgress);

            coinsRewardText.text = currentCoins.ToString();
            expRewardText.text = currentExp.ToString();

            yield return null;
        }

        coinsRewardText.text = coinsReward.ToString();
        expRewardText.text = expReward.ToString();
    }

    private void ShowLose()
    {
        battleActive = false;

        int expReward = Random.Range(10, 21);

        // ← ИСПОЛЬЗУЕМ ShowXpGainAnimation ВМЕСТО AddExp
        LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem != null)
        {
            // Добавляем флаг для animation в EquipmentScene
            PlayerPrefs.SetInt("JustFinishedBattle", 1);
            PlayerPrefs.SetInt("LastBattleExpReward", expReward);
            PlayerPrefs.Save();

            // Сразу добавляем опыт
            levelSystem.AddExp(expReward);  // ← Используем существующий метод AddExp()
        }

        Debug.Log("💔 ПОРАЖЕНИЕ! Получено опыта: " + expReward);

        StartCoroutine(ShowLosePanel(expReward));
    }

    private IEnumerator ShowLosePanel(int expReward)
    {
        yield return StartCoroutine(ScreenShake(0.5f, 10f));
        yield return StartCoroutine(ScreenFlash(0.5f));

        losePanel.SetActive(true);
        RectTransform losePanelRect = losePanel.GetComponent<RectTransform>();

        losePanelRect.localScale = new Vector3(0.5f, 0.5f, 1f);
        CanvasGroup loseCanvasGroup = losePanel.GetComponent<CanvasGroup>();
        if (loseCanvasGroup == null)
            loseCanvasGroup = losePanel.AddComponent<CanvasGroup>();
        loseCanvasGroup.alpha = 0;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            losePanelRect.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1f), Vector3.one, progress);
            loseCanvasGroup.alpha = Mathf.Lerp(0, 1, progress);

            yield return null;
        }

        losePanelRect.localScale = Vector3.one;
        loseCanvasGroup.alpha = 1;

        // ← АНИМАЦИЯ ОПЫТА В LOSE PANEL
        expLoseText.text = "0";
        float countDuration = 1f;
        float countElapsed = 0f;

        while (countElapsed < countDuration)
        {
            countElapsed += Time.deltaTime;
            float countProgress = countElapsed / countDuration;

            int currentExp = Mathf.RoundToInt(expReward * countProgress);
            expLoseText.text = currentExp.ToString();

            yield return null;
        }

        expLoseText.text = expReward.ToString();
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        RectTransform targetRect = cameraShakeTarget != null ? cameraShakeTarget : fadePanel.GetComponent<RectTransform>();
        Vector3 originalPos = targetRect.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float currentMagnitude = magnitude * (1 - progress);
            float randomX = Random.Range(-currentMagnitude, currentMagnitude);
            float randomY = Random.Range(-currentMagnitude, currentMagnitude);

            targetRect.localPosition = originalPos + new Vector3(randomX, randomY, 0);

            yield return null;
        }

        targetRect.localPosition = originalPos;
    }

    private IEnumerator ScreenFlash(float duration)
    {
        if (screenFlashRed == null) yield break;

        screenFlashRed.gameObject.SetActive(true);

        float flashDuration = 0.1f;
        float elapsed = 0f;
        int flashCount = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (elapsed % flashDuration < Time.deltaTime)
            {
                flashCount++;
                bool showFlash = flashCount % 2 == 1;
                screenFlashRed.gameObject.SetActive(showFlash);
            }

            yield return null;
        }

        screenFlashRed.gameObject.SetActive(false);
    }

    private void ContinueAfterWin()
    {
        if (achievementSystem != null)
            achievementSystem.SaveData();
        // ← ВСЕГДА ИДЁМ В СНАРЯЖЕНИЕ, НЕ В БИТВУ
        SceneManager.LoadScene("EquipmentScene");
    }

    private void ContinueAfterLose()
    {
        if (achievementSystem != null)
            achievementSystem.SaveData();
        SceneManager.LoadScene("EquipmentScene");
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public int GetPlayerPower()
    {
        return playerPower;
    }

    public int GetEnemyPower()
    {
        return enemyPower;
    }
}