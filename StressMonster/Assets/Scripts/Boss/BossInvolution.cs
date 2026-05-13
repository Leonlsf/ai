using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Boss
{
    public class BossInvolution : BossController
    {
        [Header("Attack Prefabs")]
        [SerializeField] private GameObject deadlineBombPrefab;
        [SerializeField] private GameObject kpiTextPrefab;
        [SerializeField] private GameObject overtimeRushEffectPrefab;

        [Header("Attack Settings")]
        [SerializeField] private float deadlineBombDamage = 10f;
        [SerializeField] private float kpiDamage = 5f;
        [SerializeField] private int kpiCount = 3;
        [SerializeField] private float overtimeRushDuration = 5f;
        [SerializeField] private float overtimeRushSpeedMultiplier = 1.5f;
        [SerializeField] private float overtimeRushScaleMultiplier = 1.3f;

        [Header("Death Settings")]
        [SerializeField] private GameObject deathParticlePrefab;
        [SerializeField] private GameObject runawayMonsterPrefab;
        [SerializeField] private float deathDuration = 1.5f;

        [Header("Hit Settings")]
        [SerializeField] private float knockbackForce = 0.3f;
        [SerializeField] private float knockbackDuration = 0.2f;

        private Transform monsterTransform;
        private float originalAttackInterval;
        private Vector3 originalScale;
        private bool isOvertimeRushing;
        private int lastAttackIndex = -1;

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        public override void Initialize(string name, float health, float atkInterval, EmotionType weak)
        {
            base.Initialize(name, health, atkInterval, weak);
            originalAttackInterval = atkInterval;
            originalScale = transform.localScale;
        }

        private void Start()
        {
            var monsterObj = GameObject.FindGameObjectWithTag("Monster");
            if (monsterObj != null)
                monsterTransform = monsterObj.transform;
        }

        public override void Attack()
        {
            int attackIndex;
            do
            {
                attackIndex = Random.Range(0, 3);
            } while (attackIndex == lastAttackIndex && Random.value > 0.3f);

            lastAttackIndex = attackIndex;

            switch (attackIndex)
            {
                case 0:
                    StartCoroutine(DeadlineBombAttack());
                    break;
                case 1:
                    StartCoroutine(KPIPressureAttack());
                    break;
                case 2:
                    StartCoroutine(OvertimeRushAttack());
                    break;
            }
        }

        private IEnumerator DeadlineBombAttack()
        {
            InvokeOnBossAttack("Deadline Bomb");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(0.3f);

            if (deadlineBombPrefab == null || monsterTransform == null) yield break;

            Vector3 targetPos = monsterTransform.position;
            GameObject bomb = Instantiate(deadlineBombPrefab, transform.position, Quaternion.identity);
            StartCoroutine(MoveProjectile(bomb, targetPos, deadlineBombDamage, 6f));
        }

        private IEnumerator KPIPressureAttack()
        {
            InvokeOnBossAttack("KPI Pressure");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(0.2f);

            if (kpiTextPrefab == null || monsterTransform == null) yield break;

            for (int i = 0; i < kpiCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0.5f, 2f), 0f);
                Vector3 spawnPos = transform.position + offset;
                GameObject kpi = Instantiate(kpiTextPrefab, spawnPos, Quaternion.identity);
                StartCoroutine(MoveProjectile(kpi, monsterTransform.position, kpiDamage, 4f));
                yield return new WaitForSeconds(0.2f);
            }
        }

        private IEnumerator OvertimeRushAttack()
        {
            InvokeOnBossAttack("Overtime Rush");
            isOvertimeRushing = true;

            if (overtimeRushEffectPrefab != null)
                Instantiate(overtimeRushEffectPrefab, transform.position, Quaternion.identity);

            float targetScaleX = originalScale.x * overtimeRushScaleMultiplier;
            float targetScaleY = originalScale.y * overtimeRushScaleMultiplier;
            float targetScaleZ = originalScale.z * overtimeRushScaleMultiplier;

            float elapsed = 0f;
            float growDuration = 0.5f;
            while (elapsed < growDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / growDuration;
                transform.localScale = Vector3.Lerp(originalScale, new Vector3(targetScaleX, targetScaleY, targetScaleZ), t);
                yield return null;
            }

            attackInterval = originalAttackInterval / overtimeRushSpeedMultiplier;

            yield return new WaitForSeconds(overtimeRushDuration);

            attackInterval = originalAttackInterval;
            isOvertimeRushing = false;

            elapsed = 0f;
            while (elapsed < growDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / growDuration;
                transform.localScale = Vector3.Lerp(new Vector3(targetScaleX, targetScaleY, targetScaleZ), originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private IEnumerator MoveProjectile(GameObject projectile, Vector3 target, float damage, float speed)
        {
            if (projectile == null) yield break;

            while (projectile != null && Vector3.Distance(projectile.transform.position, target) > 0.2f)
            {
                projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            if (projectile != null)
            {
                var monsterCtrl = FindObjectOfType<StressMonster.Monster.MonsterController>();
                if (monsterCtrl != null)
                {
                    monsterCtrl.TakeEnergyDamage(damage);
                }
                Destroy(projectile);
            }
        }

        protected override void OnDeath()
        {
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            if (animator != null)
                animator.SetTrigger(DeathTrigger);

            if (deathParticlePrefab != null)
                Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;

            while (elapsed < deathDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / deathDuration;
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                transform.localScale = Vector3.Lerp(startScale, endScale, eased);
                yield return null;
            }

            if (runawayMonsterPrefab != null)
            {
                GameObject runaway = Instantiate(runawayMonsterPrefab, transform.position, Quaternion.identity);
                StartCoroutine(RunawayAnimation(runaway));
            }

            Destroy(gameObject);
        }

        private IEnumerator RunawayAnimation(GameObject runaway)
        {
            Vector3 runDirection = new Vector3(Random.Range(-1f, 1f) > 0 ? 1f : -1f, 0f, 0f);
            float runDuration = 2f;
            float elapsed = 0f;
            float runSpeed = 5f;

            while (elapsed < runDuration && runaway != null)
            {
                elapsed += Time.deltaTime;
                runaway.transform.position += runDirection * runSpeed * Time.deltaTime;
                yield return null;
            }

            if (runaway != null)
                Destroy(runaway);
        }

        protected override void OnHit()
        {
            StartCoroutine(HitKnockback());
            StartCoroutine(RedFlashRoutine());
        }

        private IEnumerator HitKnockback()
        {
            Vector3 originalPos = transform.position;
            Vector3 knockbackPos = originalPos + Vector3.right * knockbackForce * (Random.value > 0.5f ? 1f : -1f);

            float elapsed = 0f;
            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / knockbackDuration;
                transform.position = Vector3.Lerp(originalPos, knockbackPos, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / knockbackDuration;
                transform.position = Vector3.Lerp(knockbackPos, originalPos, t);
                yield return null;
            }

            transform.position = originalPos;
        }

        private IEnumerator RedFlashRoutine()
        {
            if (spriteRenderer == null) yield break;

            Color prev = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, prev.a);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
