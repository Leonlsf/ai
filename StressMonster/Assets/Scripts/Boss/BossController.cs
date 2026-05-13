using System;
using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Boss
{
    public abstract class BossController : MonoBehaviour
    {
        [SerializeField] protected string bossName;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float attackInterval;
        [SerializeField] protected EmotionType weakness;
        [SerializeField] protected float weaknessMultiplier = 2f;
        [SerializeField] protected bool isAlive = true;

        [Header("Visuals")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected BossHealthBar healthBar;
        [SerializeField] protected Animator animator;

        protected float attackTimer;
        protected Color originalColor;
        protected static readonly int HitTrigger = Animator.StringToHash("Hit");
        protected static readonly int DeathTrigger = Animator.StringToHash("Death");
        protected static readonly int AttackTrigger = Animator.StringToHash("Attack");

        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsAlive => isAlive;
        public string BossName => bossName;
        public float MaxHealth => maxHealth;
        public EmotionType Weakness => weakness;

        public event Action<float> OnBossDamaged;
        public event Action<BossController> OnBossDied;
        public event Action<string> OnBossAttack;

        protected virtual void Awake()
        {
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        public virtual void Initialize(string name, float health, float atkInterval, EmotionType weak)
        {
            bossName = name;
            maxHealth = health;
            currentHealth = health;
            attackInterval = atkInterval;
            weakness = weak;
            isAlive = true;
            attackTimer = attackInterval;

            if (healthBar != null)
                healthBar.SetHealth(1f);
        }

        public virtual void TakeDamage(float damage, EmotionType emotion)
        {
            if (!isAlive) return;

            float finalDamage = emotion == weakness ? damage * weaknessMultiplier : damage;
            ApplyDamage(finalDamage);
        }

        public virtual void TakeDamage(float damage)
        {
            if (!isAlive) return;
            ApplyDamage(damage);
        }

        protected virtual void ApplyDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0f);

            float healthPercent = HealthPercentage;

            if (healthBar != null)
                healthBar.SetHealth(healthPercent);

            OnBossDamaged?.Invoke(healthPercent);
            OnHit();

            if (animator != null)
                animator.SetTrigger(HitTrigger);

            StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isAlive = false;
            OnDeath();

            if (animator != null)
                animator.SetTrigger(DeathTrigger);

            OnBossDied?.Invoke(this);
        }

        protected virtual void Update()
        {
            if (!isAlive) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackInterval;
            }
        }

        public abstract void Attack();
        protected abstract void OnDeath();
        protected abstract void OnHit();

        protected void InvokeOnBossAttack(string attackName)
        {
            OnBossAttack?.Invoke(attackName);
        }

        protected IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        public string GetBossName() => bossName;
        public EmotionType GetWeakness() => weakness;
    }
}
