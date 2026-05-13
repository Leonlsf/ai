using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Boss
{
    public class BossAnxiety : BossController
    {
        [Header("Attack Prefabs")]
        [SerializeField] private GameObject webProjectilePrefab;
        [SerializeField] private GameObject whatIfParticlePrefab;
        [SerializeField] private GameObject spiralPrefab;

        [Header("Web Trap Settings")]
        [SerializeField] private float webStunDuration = 1.5f;
        [SerializeField] private int tapsToBreakFree = 5;
        [SerializeField] private float webProjectileSpeed = 8f;

        [Header("What-If Spray Settings")]
        [SerializeField] private int whatIfParticleCount = 15;
        [SerializeField] private float whatIfDuration = 2f;

        [Header("Anxiety Spiral Settings")]
        [SerializeField] private float spiralExpandSpeed = 2f;
        [SerializeField] private float spiralMaxRadius = 5f;
        [SerializeField] private float spiralEnergyDrainRate = 3f;
        [SerializeField] private float spiralDuration = 4f;

        [Header("Death Settings")]
        [SerializeField] private GameObject deathParticlePrefab;
        [SerializeField] private GameObject[] webStrandObjects;
        [SerializeField] private float deathDuration = 2f;

        [Header("Hit Settings")]
        [SerializeField] private float recoilDistance = 0.2f;
        [SerializeField] private float recoilDuration = 0.15f;

        private Transform monsterTransform;
        private bool monsterIsStunned;
        private int currentTapCount;
        private GameObject activeWebTrap;
        private Coroutine stunCoroutine;
        private int lastAttackIndex = -1;

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
                    StartCoroutine(WebTrapAttack());
                    break;
                case 1:
                    StartCoroutine(WhatIfSprayAttack());
                    break;
                case 2:
                    StartCoroutine(AnxietySpiralAttack());
                    break;
            }
        }

        private IEnumerator WebTrapAttack()
        {
            InvokeOnBossAttack("Web Trap");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(0.3f);

            if (webProjectilePrefab == null || monsterTransform == null) yield break;

            GameObject web = Instantiate(webProjectilePrefab, transform.position, Quaternion.identity);
            activeWebTrap = web;

            while (web != null && monsterTransform != null && Vector3.Distance(web.transform.position, monsterTransform.position) > 0.3f)
            {
                web.transform.position = Vector3.MoveTowards(web.transform.position, monsterTransform.position, webProjectileSpeed * Time.deltaTime);
                yield return null;
            }

            if (web != null && monsterTransform != null && Vector3.Distance(web.transform.position, monsterTransform.position) <= 0.5f)
            {
                Destroy(web);
                activeWebTrap = null;
                stunCoroutine = StartCoroutine(StunMonster());
            }
            else if (web != null)
            {
                Destroy(web);
                activeWebTrap = null;
            }
        }

        private IEnumerator StunMonster()
        {
            monsterIsStunned = true;
            currentTapCount = 0;

            var monsterCtrl = FindObjectOfType<StressMonster.Monster.MonsterController>();
            if (monsterCtrl != null)
                monsterCtrl.SetStunned(true);

            float elapsed = 0f;
            while (elapsed < webStunDuration && currentTapCount < tapsToBreakFree)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            monsterIsStunned = false;
            if (monsterCtrl != null)
                monsterCtrl.SetStunned(false);
        }

        public void OnTapToBreakFree()
        {
            if (!monsterIsStunned) return;
            currentTapCount++;
        }

        private IEnumerator WhatIfSprayAttack()
        {
            InvokeOnBossAttack("What-If Spray");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(0.2f);

            if (whatIfParticlePrefab == null) yield break;

            GameObject[] particles = new GameObject[whatIfParticleCount];
            for (int i = 0; i < whatIfParticleCount; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-8f, 8f),
                    Random.Range(-4f, 4f),
                    0f
                );
                particles[i] = Instantiate(whatIfParticlePrefab, randomPos, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            }

            yield return new WaitForSeconds(whatIfDuration);

            foreach (var p in particles)
            {
                if (p != null)
                    Destroy(p);
            }
        }

        private IEnumerator AnxietySpiralAttack()
        {
            InvokeOnBossAttack("Anxiety Spiral");

            if (animator != null)
                animator.SetTrigger(AttackTrigger);

            if (spiralPrefab == null) yield break;

            GameObject spiral = Instantiate(spiralPrefab, transform.position, Quaternion.identity);
            float elapsed = 0f;

            while (elapsed < spiralDuration && spiral != null)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Min(elapsed * spiralExpandSpeed, spiralMaxRadius);
                spiral.transform.localScale = new Vector3(scale, scale, 1f);

                if (monsterTransform != null)
                {
                    float dist = Vector3.Distance(spiral.transform.position, monsterTransform.position);
                    if (dist < scale)
                    {
                        var monsterCtrl = FindObjectOfType<StressMonster.Monster.MonsterController>();
                        if (monsterCtrl != null)
                            monsterCtrl.TakeEnergyDamage(spiralEnergyDrainRate * Time.deltaTime);
                    }
                }

                spiral.transform.Rotate(0f, 0f, 360f * Time.deltaTime);
                yield return null;
            }

            if (spiral != null)
                Destroy(spiral);
        }

        protected override void OnDeath()
        {
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            if (animator != null)
                animator.SetTrigger(DeathTrigger);

            if (webStrandObjects != null)
            {
                for (int i = 0; i < webStrandObjects.Length; i++)
                {
                    if (webStrandObjects[i] != null)
                    {
                        StartCoroutine(BreakWebStrand(webStrandObjects[i]));
                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (deathParticlePrefab != null)
            {
                GameObject particles = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
                Destroy(particles, 2f);
            }

            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / 1f);
                    spriteRenderer.color = c;
                }
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, elapsed / 1f);
                yield return null;
            }

            Destroy(gameObject);
        }

        private IEnumerator BreakWebStrand(GameObject webStrand)
        {
            if (webStrand == null) yield break;

            Vector3 originalPos = webStrand.transform.position;
            float elapsed = 0f;
            float shakeDuration = 0.3f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                webStrand.transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
                yield return null;
            }

            webStrand.transform.position = originalPos;

            if (deathParticlePrefab != null)
                Instantiate(deathParticlePrefab, webStrand.transform.position, Quaternion.identity);

            Destroy(webStrand);
        }

        protected override void OnHit()
        {
            StartCoroutine(RecoilRoutine());
            VibrateWebs();
        }

        private IEnumerator RecoilRoutine()
        {
            Vector3 originalPos = transform.position;
            Vector3 recoilPos = originalPos + Vector3.up * recoilDistance;

            float elapsed = 0f;
            while (elapsed < recoilDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / recoilDuration;
                transform.position = Vector3.Lerp(originalPos, recoilPos, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < recoilDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / recoilDuration;
                transform.position = Vector3.Lerp(recoilPos, originalPos, t);
                yield return null;
            }

            transform.position = originalPos;
        }

        private void VibrateWebs()
        {
            if (webStrandObjects == null) return;

            foreach (var web in webStrandObjects)
            {
                if (web != null)
                    StartCoroutine(VibrateWeb(web));
            }
        }

        private IEnumerator VibrateWeb(GameObject web)
        {
            if (web == null) yield break;

            Vector3 originalPos = web.transform.position;
            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                web.transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.08f;
                yield return null;
            }

            web.transform.position = originalPos;
        }

        public bool IsMonsterStunned => monsterIsStunned;
        public int CurrentTapCount => currentTapCount;
        public int TapsRequired => tapsToBreakFree;
    }
}
