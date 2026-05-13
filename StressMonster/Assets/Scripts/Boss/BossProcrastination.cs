using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Boss
{
    public class BossProcrastination : BossController
    {
        [Header("Growth Settings")]
        [SerializeField] private float growthRate = 0.005f;
        [SerializeField] private float maxScaleMultiplier = 3f;
        private Vector3 originalScale;
        private float currentScaleMultiplier = 1f;

        [Header("Expand Attack Settings")]
        [SerializeField] private float expandScaleAmount = 0.3f;
        [SerializeField] private float expandDuration = 1f;

        [Header("Tomorrow Shield Settings")]
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private float shieldVisualScale = 1.2f;
        private GameObject activeShield;
        private bool hasShield;

        [Header("Delay Wave Settings")]
        [SerializeField] private GameObject delayWavePrefab;
        [SerializeField] private float delayWaveSpeed = 5f;
        [SerializeField] private float slowDuration = 3f;

        [Header("Death Settings")]
        [SerializeField] private GameObject slimePiecePrefab;
        [SerializeField] private int slimePieceCount = 12;
        [SerializeField] private float slimeScatterForce = 8f;
        [SerializeField] private float deathDuration = 1.5f;

        [Header("Hit Settings")]
        [SerializeField] private float wobbleIntensity = 15f;
        [SerializeField] private float wobbleDuration = 0.3f;

        private Transform monsterTransform;
        private int lastAttackIndex = -1;

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        public override void Initialize(string name, float health, float atkInterval, EmotionType weak)
        {
            base.Initialize(name, health, atkInterval, weak);
            originalScale = transform.localScale;
            currentScaleMultiplier = 1f;
            hasShield = false;
        }

        private void Start()
        {
            var monsterObj = GameObject.FindGameObjectWithTag("Monster");
            if (monsterObj != null)
                monsterTransform = monsterObj.transform;
        }

        protected override void Update()
        {
            base.Update();

            if (isAlive)
            {
                currentScaleMultiplier += growthRate * Time.deltaTime;
                currentScaleMultiplier = Mathf.Min(currentScaleMultiplier, maxScaleMultiplier);
                ApplyGrowthScale();
            }
        }

        private void ApplyGrowthScale()
        {
            transform.localScale = originalScale * currentScaleMultiplier;
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
                    StartCoroutine(ExpandAttack());
                    break;
                case 1:
                    StartCoroutine(TomorrowShieldAttack());
                    break;
                case 2:
                    StartCoroutine(DelayWaveAttack());
                    break;
            }
        }

        private IEnumerator ExpandAttack()
        {
            InvokeOnBossAttack("Expand");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            float targetMultiplier = currentScaleMultiplier + expandScaleAmount;
            targetMultiplier = Mathf.Min(targetMultiplier, maxScaleMultiplier);

            float startMultiplier = currentScaleMultiplier;
            float elapsed = 0f;

            while (elapsed < expandDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / expandDuration;
                float eased = t * t;
                currentScaleMultiplier = Mathf.Lerp(startMultiplier, targetMultiplier, eased);
                ApplyGrowthScale();
                yield return null;
            }

            currentScaleMultiplier = targetMultiplier;
            ApplyGrowthScale();
        }

        private IEnumerator TomorrowShieldAttack()
        {
            InvokeOnBossAttack("Tomorrow Shield");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            if (hasShield) yield break;

            hasShield = true;

            if (shieldPrefab != null)
            {
                activeShield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
                activeShield.transform.SetParent(transform);
                activeShield.transform.localScale = Vector3.one * shieldVisualScale;
                activeShield.transform.localPosition = Vector3.zero;
            }

            yield return null;
        }

        private IEnumerator DelayWaveAttack()
        {
            InvokeOnBossAttack("Delay Wave");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(0.3f);

            if (delayWavePrefab != null)
            {
                GameObject wave = Instantiate(delayWavePrefab, transform.position, Quaternion.identity);
                StartCoroutine(MoveWave(wave));
            }

            var monsterCtrl = FindObjectOfType<StressMonster.Monster.MonsterController>();
            if (monsterCtrl != null)
            {
                monsterCtrl.SetFoodGenerationSlowed(true);
                yield return new WaitForSeconds(slowDuration);
                monsterCtrl.SetFoodGenerationSlowed(false);
            }
        }

        private IEnumerator MoveWave(GameObject wave)
        {
            if (wave == null) yield break;

            Vector3 direction = Vector3.right;
            if (monsterTransform != null)
                direction = (monsterTransform.position - transform.position).normalized;

            float elapsed = 0f;
            float maxDuration = 3f;

            while (wave != null && elapsed < maxDuration)
            {
                elapsed += Time.deltaTime;
                wave.transform.position += direction * delayWaveSpeed * Time.deltaTime;
                yield return null;
            }

            if (wave != null)
                Destroy(wave);
        }

        public override void TakeDamage(float damage, EmotionType emotion)
        {
            if (hasShield)
            {
                BreakShield();
                return;
            }

            base.TakeDamage(damage, emotion);
        }

        public override void TakeDamage(float damage)
        {
            if (hasShield)
            {
                BreakShield();
                return;
            }

            base.TakeDamage(damage);
        }

        private void BreakShield()
        {
            hasShield = false;

            if (activeShield != null)
            {
                StartCoroutine(ShieldBreakEffect(activeShield));
            }
        }

        private IEnumerator ShieldBreakEffect(GameObject shield)
        {
            if (shield == null) yield break;

            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startScale = shield.transform.localScale;

            while (elapsed < duration && shield != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                shield.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            if (shield != null)
                Destroy(shield);

            activeShield = null;
        }

        protected override void OnDeath()
        {
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            if (animator != null)
                animator.SetTrigger(DeathTrigger);

            for (int i = 0; i < slimePieceCount; i++)
            {
                if (slimePiecePrefab != null)
                {
                    Vector3 offset = Random.insideUnitSphere * 0.5f;
                    offset.z = 0f;
                    GameObject piece = Instantiate(slimePiecePrefab, transform.position + offset, Quaternion.identity);
                    Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 force = Random.insideUnitCircle.normalized * slimeScatterForce;
                        rb.AddForce(force, ForceMode2D.Impulse);
                    }
                    Destroy(piece, 3f);
                }
                yield return new WaitForSeconds(0.03f);
            }

            float elapsed = 0f;
            while (elapsed < deathDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / deathDuration;
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    spriteRenderer.color = c;
                }
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        protected override void OnHit()
        {
            StartCoroutine(WobbleRoutine());
        }

        private IEnumerator WobbleRoutine()
        {
            float elapsed = 0f;
            Quaternion originalRot = transform.rotation;

            while (elapsed < wobbleDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / wobbleDuration;
                float intensity = wobbleIntensity * (1f - progress);
                float angle = Mathf.Sin(elapsed * 30f) * intensity;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            transform.rotation = originalRot;
        }

        public float CurrentGrowthMultiplier => currentScaleMultiplier;
        public bool HasActiveShield => hasShield;
    }
}
