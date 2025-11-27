using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EncounterManager : MonoBehaviour
{
    [Header("World & QTE")]
    [SerializeField] private WorldTierManager world;
    [SerializeField] private CircularQTE defenseQTE;   // enemy → player
    [SerializeField] private ArrowQTEImages arrowQTE;  // player skills QTE

    [Header("HUD")]
    [SerializeField] private Text infoText;

    [Header("HP Bars")]
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private Text   playerHPAmount;
    [SerializeField] private Slider enemyHPSlider;
    [SerializeField] private Text   enemyHPAmount;

    [Header("AP")]
    [SerializeField] private Text apText;
    [SerializeField] private int apMax = 6;

    [Header("Action UI")]
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button guardButton;
    [SerializeField] private Button skillButton;

    [Header("Skill Panel")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Transform  skillButtonRoot; // assign: Skill_Panel/Panel (parent that contains the skill buttons)
    [SerializeField] private Button backBtn;

    [Header("Outcome Popups (Enemy RNG when Player attacks)")]
    [SerializeField] private Text enemyHitText;
    [SerializeField] private Text enemyGuardText;
    [SerializeField] private Text enemyDodgeText;
    [SerializeField] private float outcomeFlashSeconds = 0.6f;

    [Header("Animation")]
    [SerializeField] private MonoBehaviour playerAnimatorSource;
    [SerializeField] private MonoBehaviour enemyAnimatorSource;
    [SerializeField] private MonoBehaviour bossAnimatorSource;
    public ArrowQTEImages ArrowQTE => arrowQTE;

    private enum UIMode { Actions, Skills }
    private UIMode _ui = UIMode.Actions;

    private readonly TurnManager _turns = new();
    private Player _player;
    private Enemy  _enemy;

    private bool _startedTurnAP;
    private bool _playerGuard;
    private readonly System.Random _rng = new();
    private bool IsPlayerTurn => _turns.Current == Turn.Player;
    public event Action<bool> OnEncounterFinished;

    private ICharacterAnimator _playerAnim;
    private ICharacterAnimator _enemyAnim;
    // ====== Public entry: call this from your stage/dungeon flow ======
    public void StartEncounter(Player p, Enemy e, bool isBoss = false)
    {
        _player = p;
        _enemy = e;

        _playerAnim = playerAnimatorSource as ICharacterAnimator;

        if (isBoss && bossAnimatorSource != null)
            _enemyAnim = bossAnimatorSource as ICharacterAnimator;
        else
            _enemyAnim = enemyAnimatorSource as ICharacterAnimator;

        _playerAnim?.PlayIdle();
        _enemyAnim?.PlayIdle();

        _turns.Reset(Turn.Player);
        _startedTurnAP = false;
        _playerGuard = false;
        _ui = UIMode.Actions;

        if (playerHPSlider)
        {
            playerHPSlider.minValue = 0;
            playerHPSlider.maxValue = _player.MaxHP;
            playerHPSlider.value = _player.HP;
        }
        if (enemyHPSlider)
        {
            enemyHPSlider.minValue = 0;
            enemyHPSlider.maxValue = _enemy.MaxHP;
            enemyHPSlider.value = _enemy.HP;
        }

        if (world != null)
        {
            var tier = world.Current;
            ScaleEnemyForTier(tier.enemyStatMult);
            if (defenseQTE) defenseQTE.SetDuration(tier.qteTimeToFail);
            if (arrowQTE) arrowQTE.SetTotalTime(tier.ArrowQTETimer);
        }

        WireStaticButtons();
        BuildSkillButtonsFromPlayer();
        UpdateHUD();

        HideAllEnemyOutcomeTexts();
        ApplyUIMode();
        SetInfo("Player Turn");

        StopAllCoroutines();
        StartCoroutine(MainLoop());
    }


    private void ScaleEnemyForTier(float mult)
    {
        mult = Mathf.Max(0.1f, mult);
        int scaledMax = Mathf.CeilToInt(_enemy.MaxHP * mult);
        int scaledAtk = Mathf.CeilToInt(_enemy.ATK   * mult);
        _enemy = new Enemy(_enemy.Name, scaledMax, scaledAtk);
        if (enemyHPSlider) enemyHPSlider.maxValue = _enemy.MaxHP;
        UpdateHUD();
    }

    private void WireStaticButtons()
    {
        if (attackButton) { attackButton.onClick.RemoveAllListeners(); attackButton.onClick.AddListener(OnAttack); }
        if (guardButton)  { guardButton .onClick.RemoveAllListeners(); guardButton .onClick.AddListener(OnGuard);  }
        if (skillButton)  { skillButton .onClick.RemoveAllListeners(); skillButton .onClick.AddListener(OnOpenSkills); }
        if (backBtn)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackFromSkills);
        }
    }

    private void BuildSkillButtonsFromPlayer()
    {
        if (!skillButtonRoot)
        {
            Debug.LogWarning("[Encounter] Skill Button Root is NULL — assign Skill_Panel/Panel.");
            return;
        }

        // Clear all child buttons first
        var buttons = skillButtonRoot.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            b.onClick.RemoveAllListeners();
            b.gameObject.SetActive(false);
        }

        if (_player?.Skills == null || _player.Skills.Count == 0)
        {
            Debug.LogWarning("[Encounter] Player has no skills or Skills list is null.");
            return;
        }

        for (int i = 0; i < _player.Skills.Count && i < buttons.Length; i++)
        {
            var btn   = buttons[i];
            var skill = _player.Skills[i];

            btn.gameObject.SetActive(true);
            var label = btn.GetComponentInChildren<Text>();
            if (label) label.text = $"{skill.Label} ({skill.Cost} AP)";
            btn.onClick.AddListener(() => StartCoroutine(CastSkill(skill)));
        }
    }

    private IEnumerator MainLoop()
    {
        while (_player.HP > 0 && _enemy.HP > 0)
        {
            if (IsPlayerTurn)
            {
                if (!_startedTurnAP)
                {
                    _player.AP = Mathf.Min(apMax, _player.AP + 1);
                    _startedTurnAP = true;
                    UpdateHUD();
                }

                ApplyUIMode(); // show Action or Skills depending on _ui
                yield return null;
                continue;
            }

            // Enemy turn
            _ui = UIMode.Actions;
            ApplyUIMode();

            _enemyAnim?.PlayAttack();

            if (world && defenseQTE) defenseQTE.SetDuration(world.Current.qteTimeToFail);

            if (_playerGuard)
            {
                _playerGuard = false;
                int dmg = Mathf.CeilToInt(_enemy.ATK * 0.5f);
                _player.TakeDamage(dmg);
                UpdateHUD();
                SetInfo($"Enemy attacks — Guarded! You take {dmg}.");
            }
            else
            {
                SetInfo("Enemy attacks! Time the defense…");
                QTEResult res = QTEResult.Miss;
                if (defenseQTE) yield return defenseQTE.RunDefense(r => res = r);

                float dr = res == QTEResult.Perfect ? 1f : res == QTEResult.Good ? 0.5f : 0f;

                if (dr == 1f)
                {
                    _playerAnim?.PlayDodge();
                }
                else if (dr == 0.5f)
                {
                    _playerAnim?.PlayGuard();
                }


                int dmg = Mathf.CeilToInt(_enemy.ATK * (1f - dr));
                _player.TakeDamage(dmg);
                UpdateHUD();
                SetInfo($"QTE: {res}. You take {dmg}.");

            }

            if (CheckEnd()) break;
            _turns.Next();
            _startedTurnAP = false;
            SetInfo("Player Turn");
        }

        _ui = UIMode.Actions;
        ApplyUIMode();
        SetInfo(_player.HP > 0 ? "Victory!" : "Defeat…");
        OnEncounterFinished?.Invoke(_player.HP > 0);

    }

    // ================== Player actions ==================
    private void OnAttack() => StartCoroutine(CoAttack());

    private IEnumerator CoAttack()
    {
        _ui = UIMode.Actions; 
        ApplyUIMode();
        _playerAnim?.PlayAttack();

        // Enemy RNG outcome (dodge / guard / none)
        float dr = 0f;
        var roll = _rng.NextDouble();
        string outcome = "Hit";
        if (roll < 0.10)
        {
            dr = 1f;        // full avoid
            outcome = "Dodge";
            _enemyAnim?.PlayDodge();
        }
        else if (roll < 0.35)
        {
            dr = 0.5f;      // half damage
            outcome = "Guard";
            _enemyAnim?.PlayGuard();
        }

        // Flash popup
        yield return StartCoroutine(FlashEnemyOutcome(outcome));

        int dmg = Mathf.CeilToInt(_player.ATK * (1f - dr));
        _enemy.TakeDamage(dmg);
        if (_enemy.HP > 0)
        UpdateHUD();
        SetInfo(outcome == "Dodge"
            ? "Enemy dodged!"
            : outcome == "Guard" ? $"Enemy guarded. You deal {dmg}."
                                  : $"You deal {dmg}.");

        if (!CheckEnd())
        {
            _turns.Next();
            _startedTurnAP = false;
        }
    }

    private void OnGuard()
    {
        _ui = UIMode.Actions; 
        ApplyUIMode();
        _playerGuard = true;

        _playerAnim?.PlayGuard();

        SetInfo("You brace yourself (Guard).");
        _turns.Next();
        _startedTurnAP = false;
    }
    public void PlayPlayerSkillAnimation(string triggerName = null)
    {
        _playerAnim?.PlaySkill(triggerName);
    }

    private void OnOpenSkills()
    {
        BuildSkillButtonsFromPlayer();
        RefreshSkillInteractable();
        _ui = UIMode.Skills;
        ApplyUIMode();
        SetInfo("Choose a skill…");
    }

    private void OnBackFromSkills()
    {
        _ui = UIMode.Actions;
        ApplyUIMode();
        SetInfo("Player Turn");
    }

    private IEnumerator CastSkill(ISkill skill)
    {
        bool finished = false;

        // Delegate AP check + ArrowQTE execution to the Skill itself
        yield return skill.Perform(this, _player, _enemy, ok =>
        {
            UpdateHUD();
            if (ok)
            {
                _turns.Next();
                _startedTurnAP = false;
                _ui = UIMode.Actions;
                ApplyUIMode();
            }
            else
            {
                _turns.Next();                   // failed QTE skips the turn by design
                _ui = UIMode.Skills;             // panel state will be reset by ApplyUIMode on next loop
                ApplyUIMode();
                SetInfo("Skill failed or not enough AP.");
                RefreshSkillInteractable();
            }
            finished = true;
        });

        while (!finished) yield return null;
    }

    // ================== UI helpers ==================
    private void ApplyUIMode()
    {
        // Skills UI wants to be on only when: (a) we're in Skills mode AND (b) it's the player's turn
        bool showSkills  = (_ui == UIMode.Skills) && IsPlayerTurn;
        bool showActions = !showSkills && IsPlayerTurn;

        if (actionPanel) actionPanel.SetActive(showActions);
        if (attackButton) attackButton.interactable = showActions;
        if (guardButton)  guardButton.interactable  = showActions;
        if (skillButton)  skillButton.interactable  = showActions;

        if (skillPanel) skillPanel.SetActive(showSkills);
        if (backBtn)    backBtn.interactable = showSkills;

        if (showSkills) RefreshSkillInteractable();
    }

    private void RefreshSkillInteractable()
    {
        if (!skillButtonRoot || _player?.Skills == null) return;
        var buttons = skillButtonRoot.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < _player.Skills.Count && i < buttons.Length; i++)
            buttons[i].interactable = _player.AP >= _player.Skills[i].Cost;
    }

    private bool CheckEnd() => _enemy.HP <= 0 || _player.HP <= 0;

    public void PlayerGainHP(int amount)
{
    if (_player == null) return;                  // <- added
    _player.HP = Mathf.Clamp(_player.HP + amount, 0, _player.MaxHP);
    UpdateHUD();
    }


    private void UpdateHUD()
    {
        if (_player != null)
        {
            if (playerHPSlider) playerHPSlider.value = _player.HP;
            if (playerHPAmount) playerHPAmount.text  = $"{_player.HP}/{_player.MaxHP}";
            if (apText)         apText.text          = $"AP : {_player.AP} / {apMax}";
        }
        if (_enemy != null)
        {
            if (enemyHPSlider) enemyHPSlider.value = _enemy.HP;
            if (enemyHPAmount) enemyHPAmount.text  = $"{_enemy.HP}/{_enemy.MaxHP}";
        }
    }

    public void FlashInfo(string s) => SetInfo(s);
    private void SetInfo(string s) { if (infoText) infoText.text = s; }

    // ================== Enemy outcome popups ==================
    private void HideAllEnemyOutcomeTexts()
    {
        if (enemyHitText)   enemyHitText.gameObject.SetActive(false);
        if (enemyGuardText) enemyGuardText.gameObject.SetActive(false);
        if (enemyDodgeText) enemyDodgeText.gameObject.SetActive(false);
    }

    private IEnumerator FlashEnemyOutcome(string outcome)
    {
        HideAllEnemyOutcomeTexts();

        Text target = null;
        switch (outcome)
        {
            case "Dodge": target = enemyDodgeText; break;
            case "Guard": target = enemyGuardText; break;
            default:      target = enemyHitText;   break;
        }

        if (target)
        {
            target.gameObject.SetActive(true);
            yield return new WaitForSeconds(outcomeFlashSeconds);
            target.gameObject.SetActive(false);
        }
    }
}