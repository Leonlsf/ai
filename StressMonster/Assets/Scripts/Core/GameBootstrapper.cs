using UnityEngine;
using StressMonster.Core;
using StressMonster.Monster;
using StressMonster.Feeding;
using StressMonster.Emotion;
using StressMonster.Boss;
using StressMonster.UI;

namespace StressMonster.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Monster")]
        [SerializeField] private MonsterController monsterController;
        [SerializeField] private MonsterEvolution monsterEvolution;

        [Header("Feeding")]
        [SerializeField] private FoodGenerator foodGenerator;
        [SerializeField] private DragHandler dragHandler;
        [SerializeField] private ComboSystem comboSystem;

        [Header("Emotion")]
        [SerializeField] private EmotionConverter emotionConverter;
        [SerializeField] private EmotionEffectPlayer emotionEffectPlayer;
        [SerializeField] private EmotionKeywordDB emotionKeywordDB;

        [Header("Boss")]
        [SerializeField] private BossBattleManager bossBattleManager;

        [Header("UI")]
        [SerializeField] private InputPanel inputPanel;
        [SerializeField] private EmotionSelector emotionSelector;
        [SerializeField] private ComboDisplay comboDisplay;
        [SerializeField] private EvolutionPanel evolutionPanel;
        [SerializeField] private BossHealthBar bossHealthBar;

        [Header("Settings")]
        [SerializeField] private float energyPerFeed = 10f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float bossEnergyThreshold = 100f;

        private void Awake()
        {
            InitializeManagers();
            InitializeEmotionSystem();
            InitializeFeedingSystem();
            InitializeBossSystem();
            InitializeUISystem();
            WireEvents();
        }

        private void InitializeManagers()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            if (inputManager == null)
                inputManager = FindObjectOfType<InputManager>();
            if (audioManager == null)
                audioManager = FindObjectOfType<AudioManager>();
        }

        private void InitializeEmotionSystem()
        {
            if (emotionKeywordDB == null)
                emotionKeywordDB = EmotionKeywordDB.CreateDefault();

            if (emotionConverter != null)
                emotionConverter.SetKeywordDB(emotionKeywordDB);

            if (foodGenerator != null)
                foodGenerator.SetEmotionConverter(emotionConverter);
        }

        private void InitializeFeedingSystem()
        {
            if (dragHandler != null && monsterController != null)
                dragHandler.SetMouthTarget(monsterController.MouthPosition);

            if (comboSystem == null)
                comboSystem = FindObjectOfType<ComboSystem>();
        }

        private void InitializeBossSystem()
        {
            if (bossBattleManager != null && monsterController != null)
                bossBattleManager.SetMonster(monsterController);
        }

        private void InitializeUISystem()
        {
            if (inputPanel != null && foodGenerator != null)
                inputPanel.SetFoodGenerator(foodGenerator);

            if (emotionSelector != null)
                emotionSelector.SetKeywordDB(emotionKeywordDB);
        }

        private void WireEvents()
        {
            if (monsterController != null)
            {
                monsterController.OnMonsterFed += HandleMonsterFed;
            }

            if (comboSystem != null)
            {
                comboSystem.OnComboChanged += HandleComboChanged;
                comboSystem.OnComboMilestone += HandleComboMilestone;
            }

            if (bossBattleManager != null)
            {
                bossBattleManager.OnBattleStart += HandleBattleStart;
                bossBattleManager.OnBattleEnd += HandleBattleEnd;
                bossBattleManager.OnBossDamaged += HandleBossDamaged;
            }

            if (gameManager != null)
            {
                gameManager.OnStateChanged += HandleGameStateChanged;
            }
        }

        private void HandleMonsterFed(EmotionType emotion)
        {
            if (comboSystem != null)
                comboSystem.OnFeed();

            if (emotionEffectPlayer != null)
            {
                emotionEffectPlayer.PlayEffect(emotion, monsterController.transform.position);
                emotionEffectPlayer.PlayScreenEffect(emotion);
                emotionEffectPlayer.PlayEmotionSound(emotion);
            }

            if (gameManager != null)
            {
                gameManager.FeedMonster(energyPerFeed);

                if (gameManager.CurrentEnergy >= bossEnergyThreshold && gameManager.CurrentState == GameState.Feeding)
                {
                    gameManager.ChangeState(GameState.BossBattle);
                }
            }

            if (monsterController != null && monsterEvolution != null)
            {
                monsterEvolution.CheckEvolution(monsterController.Stats);
            }
        }

        private void HandleComboChanged(int combo)
        {
            if (comboDisplay != null)
                comboDisplay.UpdateCombo(combo, comboSystem.ComboMultiplier, comboSystem.TimerPercent);
        }

        private void HandleComboMilestone(int combo)
        {
            Debug.Log($"Combo Milestone: {combo}!");
        }

        private void HandleBattleStart(BossController boss)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.SetBoss(boss.BossName, boss.MaxHealth, boss.Weakness);
                bossHealthBar.Show();
            }

            if (inputPanel != null)
                inputPanel.Hide();

            if (evolutionPanel != null)
                evolutionPanel.Hide();
        }

        private void HandleBattleEnd(bool victory)
        {
            if (bossHealthBar != null)
                bossHealthBar.Hide();

            if (victory)
            {
                if (gameManager != null)
                    gameManager.CompleteBossBattle(true);

                if (evolutionPanel != null && monsterController != null)
                {
                    evolutionPanel.Show(monsterController.Stats);
                }
            }
            else
            {
                if (gameManager != null)
                    gameManager.CompleteBossBattle(false);
            }

            if (inputPanel != null)
                inputPanel.Show();
        }

        private void HandleBossDamaged(float healthPercent)
        {
            if (bossHealthBar != null)
                bossHealthBar.UpdateHealth(healthPercent * bossBattleManager.GetCurrentBoss().MaxHealth,
                    bossBattleManager.GetCurrentBoss().MaxHealth);
        }

        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    if (inputPanel != null) inputPanel.Hide();
                    if (evolutionPanel != null) evolutionPanel.Hide();
                    break;
                case GameState.Feeding:
                    if (inputPanel != null) inputPanel.Show();
                    if (monsterController != null) monsterController.ExitBattleMode();
                    break;
                case GameState.BossBattle:
                    if (inputPanel != null) inputPanel.Show();
                    if (monsterController != null) monsterController.EnterBattleMode();
                    if (bossBattleManager != null) bossBattleManager.StartBattle(0);
                    break;
                case GameState.Evolution:
                    if (evolutionPanel != null && monsterController != null)
                        evolutionPanel.Show(monsterController.Stats);
                    break;
                case GameState.Paused:
                    break;
            }
        }

        private void OnDestroy()
        {
            if (monsterController != null)
                monsterController.OnMonsterFed -= HandleMonsterFed;

            if (comboSystem != null)
            {
                comboSystem.OnComboChanged -= HandleComboChanged;
                comboSystem.OnComboMilestone -= HandleComboMilestone;
            }

            if (bossBattleManager != null)
            {
                bossBattleManager.OnBattleStart -= HandleBattleStart;
                bossBattleManager.OnBattleEnd -= HandleBattleEnd;
                bossBattleManager.OnBossDamaged -= HandleBossDamaged;
            }

            if (gameManager != null)
                gameManager.OnStateChanged -= HandleGameStateChanged;
        }
    }
}
