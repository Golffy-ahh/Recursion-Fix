using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MapGenerator;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    public WorldTierManager world;       // has CurrentIndex(0..3) & Current (data)
    public MapGenerator     generator;
    public EncounterManager encounter;

    [Header("Object")]
    [SerializeField] private GameObject regularEnemyGO; // drag your normal enemy GO
    [SerializeField] private GameObject bossEnemyGO;    // drag your boss enemy GO
    [SerializeField] private GameObject GameWinGO;
    [SerializeField] private GameObject GameOverGO;

    [Header("Choice UI")]
    public ChoiceCard cardLeft;
    public ChoiceCard cardCenter;
    public ChoiceCard cardRight;

    [Header("Labels")]
    public Text layerStageText;
    [SerializeField] private GameObject choicePanel;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel; // drag your Pause UI here
    private bool _paused = false;
    private bool _lastEncounterWasBoss = false;
    [Header("Audio / BGM")]
    [SerializeField] private MonoBehaviour audioServiceSource;
    [SerializeField] private AudioClip ChoiceStageBGM;
    [SerializeField] private AudioClip normalBattleBGM;
    [SerializeField] private AudioClip bossBattleBGM;
    [SerializeField] private AudioClip GameWinBGM;
    [SerializeField] private AudioClip GameOverBGM;
    private IAudioService audioService;


    // simple persistent state
    int _stageInLayer = 1; // 1..3 (Layer 4 is boss flow)

    void Start()
    {
        if (!world) Debug.LogError("GameManager: WorldTierManager not assigned.");
        if (!generator) Debug.LogError("GameManager: MapGenerator not assigned.");
        if (!encounter) Debug.LogError("GameManager: EncounterManager not assigned.");

        if (audioServiceSource != null)
            audioService = audioServiceSource as IAudioService;

        if (GameWinGO)   GameWinGO.SetActive(false);
        if (GameOverGO)  GameOverGO.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        ShowChoices();
    }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // don’t pause over win/lose screens
                if (GameWinGO && GameWinGO.activeSelf) return;
                if (GameOverGO && GameOverGO.activeSelf) return;

                TogglePause();
            }
        }

    void ShowChoices()
    {
        ClearAllCards();

        int tier1to4 = Mathf.Clamp(world.CurrentIndex + 1, 1, 4);
        List<MapNode> nodes = generator.Generate(tier1to4);

        var cards = new ChoiceCard[] { cardLeft, cardCenter, cardRight };

        int count = Mathf.Min(nodes.Count, cards.Length);
        for (int i = 0; i < count; i++)
        {
            int captured = i; // closure-safe
            if (cards[i] == null) continue;
            cards[i].Bind(nodes[i], () => OnNodeChosen(nodes[captured]));
        }

        UpdateLayerStageLabel();
    }

    void ClearAllCards()
    {
        if (cardLeft)   cardLeft.Clear();
        if (cardCenter) cardCenter.Clear();
        if (cardRight)  cardRight.Clear();
    }

    void UpdateLayerStageLabel()
    {
        int layer = world.CurrentIndex + 1;
        int stage = Mathf.Clamp(_stageInLayer, 1, 3);
        if (layer >= 4) layerStageText.text = "Layer 4 – Boss";
        else            layerStageText.text = $"Layer {layer} – {stage}";
    }

    // =============== Choice handling =================
    void OnNodeChosen(MapNode node)
    {
        switch (node.Type)
        {
            case NodeType.Encounter:
                if (choicePanel) choicePanel.SetActive(false);
                StartEncounter(false);
                break;

            case NodeType.Rest:
                encounter.PlayerGainHP(30);
                AdvanceStage(); // rest finishes immediately
                break;

            case NodeType.Event:
                // 50/50 heal or damage 20
                if (Random.value < 0.5f) encounter.PlayerGainHP(20);
                else                      encounter.FlashInfo("Ouch! -20 HP"); // damage via Encounter if you prefer
                // quick damage route:
                // encounter.ApplyDirectDamageToPlayer(20); <-- if you add such helper
                AdvanceStage();
                break;

            case NodeType.Boss:
                if (choicePanel) choicePanel.SetActive(false);
                StartEncounter(true);
                break;
        }
    }

    void StartEncounter(bool isBoss)
    {
        _lastEncounterWasBoss = isBoss;
        ClearAllCards();

        if (regularEnemyGO) regularEnemyGO.SetActive(!isBoss);
        if (bossEnemyGO) bossEnemyGO.SetActive(isBoss);

        var p = new Player("Hero", 100, 15, 0);
        var e = isBoss
            ? new Enemy("Gate-Binder", 80, 20)
            : new Enemy($"Grunt T{world.CurrentIndex + 1}", 20, 10);

        encounter.OnEncounterFinished -= OnEncounterFinished;
        encounter.OnEncounterFinished += OnEncounterFinished;

        if (audioService != null)
        {
            var bgm = isBoss ? bossBattleBGM : normalBattleBGM;
            audioService.PlayMusic(bgm);
        }

        encounter.StartEncounter(p, e, isBoss);
    }


    void OnEncounterFinished(bool win)
    {
        encounter.OnEncounterFinished -= OnEncounterFinished;

        if (audioService != null)
            audioService.StopMusic();

        if (!win)
        {
            // Hide enemies and choices
            if (regularEnemyGO) regularEnemyGO.SetActive(false);
            if (bossEnemyGO)    bossEnemyGO.SetActive(false);
            if (choicePanel)    choicePanel.SetActive(false);

            // Show Game Over
            if (GameOverGO) GameOverGO.SetActive(true);
            return;
        }



        if (_lastEncounterWasBoss)
        {
        if (regularEnemyGO) regularEnemyGO.SetActive(false);
        if (bossEnemyGO)    bossEnemyGO.SetActive(false);
        if (choicePanel)    choicePanel.SetActive(false);
        if (GameWinGO)      GameWinGO.SetActive(true);  // <— WIN SCREEN
        return;
        }
        
        if (choicePanel) choicePanel.SetActive(true);
        // Normal tier: count stage progress
        AdvanceStage();
    }

    void AdvanceStage()
    {
        // Stages 1..3 → after 3, advance layer
        if (world.CurrentIndex < 3)
        {
            _stageInLayer++;
            if (_stageInLayer > 3)
            {
                _stageInLayer = 1;
                world.SetCurrentIndex(world.CurrentIndex + 1);
            }
        }
        else
        {
            // If called from boss via other flows, just reset after win
            world.SetCurrentIndex(0);
            _stageInLayer = 1;
        }

        ShowChoices();
    }

    public void SkipLayer()
    {
        world.SetCurrentIndex(world.CurrentIndex + 1);
        UpdateLayerStageLabel();
        ShowChoices();
    }

        public void RetryRun()
    {
        Time.timeScale = 1f;

        // Reload current scene (your Combat/Gameplay scene)
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void TogglePause()
    {
        if (_paused) Resume();
        else         Pause();
    }

    public void Pause()
    {
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        _paused = true;
    }

    public void Resume()
    {
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        _paused = false;
    }
}