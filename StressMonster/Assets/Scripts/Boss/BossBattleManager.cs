using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StressMonster.Core;
using StressMonster.Emotion;
using StressMonster.Monster;

namespace StressMonster.Boss
{
    public enum BattleState
    {
        Idle,
        Intro,
        Fighting,
        Victory,
        Defeat
    }

    public class BossBattleManager : MonoBehaviour
    {
        [Header("Boss References")]
        [SerializeField] private List<BossController> bossPrefabs = new List<BossController>();
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private MonsterController monster;

        [Header("Battle Settings")]
        [SerializeField] private float introDuration = 1.5f;
        [SerializeField] private float introSlideSpeed = 5f;
        [SerializeField] private float energyCostPerSecond = 1f;
        [SerializeField] private float energyRestoreOnFeed = 5f;

        [Header("Victory Settings")]
        [SerializeField] private GameObject rewardPopupPrefab;
        [SerializeField] private float victoryDelay = 2f;

        [Header("Defeat Settings")]
        [SerializeField] private GameObject retryPopupPrefab;
        [SerializeField] private float defeatDelay = 1.5f;

        private BattleState currentBattleState = BattleState.Idle;
        private BossController currentBoss;
        private int currentBossIndex = -1;
        private float battleEnergy;

        public BattleState CurrentState => currentBattleState;
        public BossController CurrentBoss => currentBoss;

        public event Action<BossController> OnBattleStart;
        public event Action<bool> OnBattleEnd;
        public event Action<float> OnBossDamaged;

        private void OnEnable()
        {
            if (monster != null)
                monster.OnMonsterFed += OnMonsterFed;
        }

        private void OnDisable()
        {
            if (monster != null)
                monster.OnMonsterFed -= OnMonsterFed;
        }

        private void Update()
        {
            if (currentBattleState == BattleState.Fighting)
            {
                battleEnergy -= energyCostPerSecond * Time.deltaTime;
                if (battleEnergy <= 0f)
                {
                    battleEnergy = 0f;
                    EndBattle(false);
                }
            }
        }

        public void StartBattle(int bossIndex)
        {
            if (currentBattleState != BattleState.Idle) return;
            if (bossIndex < 0 || bossIndex >= bossPrefabs.Count) return;

            currentBossIndex = bossIndex;
            currentBattleState = BattleState.Intro;

            GameManager.Instance.ChangeState(GameState.BossBattle);

            if (monster != null)
                monster.EnterBattleMode();

            SpawnBoss(bossIndex);
            StartCoroutine(BossIntroSequence());
        }

        private void SpawnBoss(int bossIndex)
        {
            BossController prefab = bossPrefabs[bossIndex];
            Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
            spawnPos.y += 8f;

            currentBoss = Instantiate(prefab, spawnPos, Quaternion.identity);
            currentBoss.OnBossDamaged += HandleBossDamaged;
            currentBoss.OnBossDied += HandleBossDied;
        }

        private IEnumerator BossIntroSequence()
        {
            if (currentBoss == null) yield break;

            Vector3 targetPos = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
            Vector3 startPos = currentBoss.transform.position;

            float elapsed = 0f;
            while (elapsed < introDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / introDuration;
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                currentBoss.transform.position = Vector3.Lerp(startPos, targetPos, eased);
                yield return null;
            }

            currentBoss.transform.position = targetPos;

            currentBattleState = BattleState.Fighting;
            OnBattleStart?.Invoke(currentBoss);
        }

        private void HandleBossDamaged(float healthPercent)
        {
            OnBossDamaged?.Invoke(healthPercent);
        }

        private void HandleBossDied(BossController boss)
        {
            if (currentBattleState != BattleState.Fighting) return;
            StartCoroutine(VictorySequence());
        }

        private IEnumerator VictorySequence()
        {
            currentBattleState = BattleState.Victory;

            yield return new WaitForSeconds(victoryDelay);

            if (rewardPopupPrefab != null)
            {
                GameObject popup = Instantiate(rewardPopupPrefab, Vector3.zero, Quaternion.identity);
                var rewardDisplay = popup.GetComponent<RewardDisplay>();
                if (rewardDisplay != null)
                    rewardDisplay.Show(GetVictoryRewards());
            }

            yield return new WaitForSeconds(1f);

            CleanupBoss();
            ReturnToFeeding();
            OnBattleEnd?.Invoke(true);
        }

        private IEnumerator DefeatSequence()
        {
            currentBattleState = BattleState.Defeat;

            if (monster != null)
            {
                var monsterAnimator = monster.GetComponent<Animator>();
                if (monsterAnimator != null)
                    monsterAnimator.SetTrigger("Collapse");
            }

            yield return new WaitForSeconds(defeatDelay);

            if (retryPopupPrefab != null)
            {
                GameObject popup = Instantiate(retryPopupPrefab, Vector3.zero, Quaternion.identity);
                var retryHandler = popup.GetComponent<RetryHandler>();
                if (retryHandler != null)
                {
                    retryHandler.Setup(currentBossIndex, this);
                }
            }
        }

        public void EndBattle(bool victory)
        {
            if (currentBattleState == BattleState.Idle) return;

            if (victory)
            {
                StartCoroutine(VictorySequence());
            }
            else
            {
                StartCoroutine(DefeatSequence());
            }
        }

        public void RetryBattle()
        {
            CleanupBoss();
            currentBattleState = BattleState.Idle;
            StartBattle(currentBossIndex);
        }

        private void OnMonsterFed(EmotionType emotion)
        {
            if (currentBattleState != BattleState.Fighting) return;
            if (currentBoss == null || !currentBoss.IsAlive) return;

            currentBoss.TakeDamage(10f, emotion);
            battleEnergy += energyRestoreOnFeed;
        }

        private void CleanupBoss()
        {
            if (currentBoss != null)
            {
                currentBoss.OnBossDamaged -= HandleBossDamaged;
                currentBoss.OnBossDied -= HandleBossDied;
                Destroy(currentBoss.gameObject);
                currentBoss = null;
            }
        }

        private void ReturnToFeeding()
        {
            currentBattleState = BattleState.Idle;
            currentBossIndex = -1;

            if (monster != null)
                monster.ExitBattleMode();
        }

        private RewardData GetVictoryRewards()
        {
            return new RewardData
            {
                evolutionMaterial = 1,
                bossEssence = currentBoss != null ? currentBoss.GetBossName() : "",
            };
        }

        public BossController GetCurrentBoss() => currentBoss;
        public float GetBattleEnergy() => battleEnergy;
        public void SetBattleEnergy(float energy) => battleEnergy = energy;
    }

    [Serializable]
    public class RewardData
    {
        public int evolutionMaterial;
        public string bossEssence;
    }
}
