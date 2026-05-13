using System;
using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Monster
{
    public enum MonsterState
    {
        Hungry,
        Chewing,
        Satisfied,
        Burping,
        BattleMode,
        Hit
    }

    public class MonsterController : MonoBehaviour
    {
        public MonsterStats stats;
        public Animator animator;
        public SpriteRenderer spriteRenderer;
        public Transform mouthPosition;
        public MonsterAnimation monsterAnimation;

        public float chewDuration = 0.8f;
        public float satisfiedDuration = 0.5f;
        public float burpDuration = 1f;
        public float hitRecoveryDuration = 0.5f;

        private MonsterState _currentState = MonsterState.Hungry;
        private Coroutine _stateTimerCoroutine;
        private EmotionType _lastFedEmotion;

        public MonsterState CurrentState => _currentState;
        public Transform MouthPosition => mouthPosition;
        public MonsterStats Stats => stats;

        public event Action<EmotionType> OnMonsterFed;
        public event Action<MonsterState> OnStateChanged;

        private void Awake()
        {
            if (stats == null)
            {
                stats = new MonsterStats();
            }

            if (monsterAnimation == null)
            {
                monsterAnimation = GetComponent<MonsterAnimation>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            SetState(MonsterState.Hungry);
        }

        public void Feed(EmotionType emotion)
        {
            if (_currentState == MonsterState.BattleMode || _currentState == MonsterState.Hit)
                return;

            _lastFedEmotion = emotion;
            stats.AddFeed(emotion);
            OnMonsterFed?.Invoke(emotion);

            if (monsterAnimation != null)
            {
                monsterAnimation.BounceScale();
            }

            SetState(MonsterState.Chewing);
            StartStateTimer(chewDuration, () =>
            {
                SetState(MonsterState.Satisfied);
                StartStateTimer(satisfiedDuration, () =>
                {
                    SetState(MonsterState.Burping);
                    StartStateTimer(burpDuration, () =>
                    {
                        SetState(MonsterState.Hungry);
                    });
                });
            });
        }

        public void EnterBattleMode()
        {
            CancelStateTimer();
            SetState(MonsterState.BattleMode);
        }

        public void ExitBattleMode()
        {
            SetState(MonsterState.Hungry);
        }

        public void TakeHit()
        {
            if (_currentState != MonsterState.BattleMode)
                return;

            CancelStateTimer();
            stats.ConsumeEnergy(10f);
            SetState(MonsterState.Hit);

            if (monsterAnimation != null)
            {
                monsterAnimation.FlashColor(Color.red);
            }

            StartStateTimer(hitRecoveryDuration, () =>
            {
                SetState(MonsterState.BattleMode);
            });
        }

        public void TakeEnergyDamage(float damage)
        {
            stats.ConsumeEnergy(damage);
        }

        private bool _isStunned;

        public void SetStunned(bool stunned)
        {
            _isStunned = stunned;
        }

        public bool IsStunned => _isStunned;

        private bool _isFoodGenerationSlowed;

        public void SetFoodGenerationSlowed(bool slowed)
        {
            _isFoodGenerationSlowed = slowed;
        }

        public bool IsFoodGenerationSlowed => _isFoodGenerationSlowed;

        public void PlayEmotionReaction(EmotionType emotion)
        {
            if (monsterAnimation != null)
            {
                monsterAnimation.PlayEmotionReaction(emotion);
            }
        }

        private void SetState(MonsterState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;

            if (monsterAnimation != null)
            {
                monsterAnimation.PlayState(newState);
            }

            if (newState == MonsterState.Burping)
            {
                if (monsterAnimation != null)
                {
                    monsterAnimation.Shake();
                }
            }

            OnStateChanged?.Invoke(newState);
        }

        private void StartStateTimer(float duration, Action onComplete)
        {
            CancelStateTimer();
            _stateTimerCoroutine = StartCoroutine(StateTimerRoutine(duration, onComplete));
        }

        private void CancelStateTimer()
        {
            if (_stateTimerCoroutine != null)
            {
                StopCoroutine(_stateTimerCoroutine);
                _stateTimerCoroutine = null;
            }
        }

        private IEnumerator StateTimerRoutine(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration);
            _stateTimerCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
