using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StressMonster.Emotion
{
    public class EmotionEffectPlayer : MonoBehaviour
    {
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem angerParticles;
        [SerializeField] private ParticleSystem anxietyParticles;
        [SerializeField] private ParticleSystem sadnessParticles;
        [SerializeField] private ParticleSystem irritationParticles;

        [Header("Screen Effects")]
        [SerializeField] private Image screenOverlay;
        [SerializeField] private Camera mainCamera;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip angerClip;
        [SerializeField] private AudioClip anxietyClip;
        [SerializeField] private AudioClip sadnessClip;
        [SerializeField] private AudioClip irritationClip;

        private const int PoolSize = 3;

        private Dictionary<EmotionType, List<ParticleSystem>> _particlePool;
        private Dictionary<EmotionType, ParticleSystem> _particlePrefabs;
        private Dictionary<EmotionType, AudioClip> _audioClips;

        private Coroutine _currentScreenEffect;
        private Vector3 _originalCameraPos;

        private void Awake()
        {
            InitializeParticlePool();
            InitializeAudioMap();

            if (mainCamera != null)
                _originalCameraPos = mainCamera.transform.localPosition;

            if (screenOverlay != null)
            {
                screenOverlay.color = new Color(0f, 0f, 0f, 0f);
                screenOverlay.gameObject.SetActive(false);
            }
        }

        private void InitializeParticlePool()
        {
            _particlePrefabs = new Dictionary<EmotionType, ParticleSystem>
            {
                { EmotionType.Anger, angerParticles },
                { EmotionType.Anxiety, anxietyParticles },
                { EmotionType.Sadness, sadnessParticles },
                { EmotionType.Irritation, irritationParticles }
            };

            _particlePool = new Dictionary<EmotionType, List<ParticleSystem>>();

            foreach (var kvp in _particlePrefabs)
            {
                if (kvp.Value == null) continue;

                var pool = new List<ParticleSystem>();
                for (int i = 0; i < PoolSize; i++)
                {
                    var instance = Instantiate(kvp.Value, transform);
                    instance.gameObject.SetActive(false);
                    pool.Add(instance);
                }
                _particlePool[kvp.Key] = pool;
            }
        }

        private void InitializeAudioMap()
        {
            _audioClips = new Dictionary<EmotionType, AudioClip>
            {
                { EmotionType.Anger, angerClip },
                { EmotionType.Anxiety, anxietyClip },
                { EmotionType.Sadness, sadnessClip },
                { EmotionType.Irritation, irritationClip }
            };
        }

        public void PlayEffect(EmotionType type, Vector3 position)
        {
            if (!_particlePool.TryGetValue(type, out List<ParticleSystem> pool)) return;

            foreach (var ps in pool)
            {
                if (!ps.gameObject.activeSelf)
                {
                    ps.transform.position = position;
                    ps.gameObject.SetActive(true);
                    ps.Play();
                    StartCoroutine(ReturnToPoolWhenDone(ps));
                    return;
                }
            }

            if (_particlePrefabs.TryGetValue(type, out ParticleSystem prefab) && prefab != null)
            {
                var instance = Instantiate(prefab, position, Quaternion.identity, transform);
                instance.Play();
                pool.Add(instance);
                StartCoroutine(ReturnToPoolWhenDone(instance));
            }
        }

        private IEnumerator ReturnToPoolWhenDone(ParticleSystem ps)
        {
            yield return new WaitWhile(() => ps.isPlaying);
            ps.gameObject.SetActive(false);
        }

        public void PlayScreenEffect(EmotionType type)
        {
            if (_currentScreenEffect != null)
                StopCoroutine(_currentScreenEffect);

            _currentScreenEffect = StartCoroutine(PlayScreenEffectCoroutine(type));
        }

        private IEnumerator PlayScreenEffectCoroutine(EmotionType type)
        {
            switch (type)
            {
                case EmotionType.Anger:
                    yield return StartCoroutine(PlayRedEdgeGlow());
                    break;
                case EmotionType.Anxiety:
                    yield return StartCoroutine(PlayScreenShake(0.3f, 0.3f));
                    break;
                case EmotionType.Sadness:
                    yield return StartCoroutine(PlayBlueTintWithRainbow());
                    break;
                case EmotionType.Irritation:
                    yield return StartCoroutine(PlayIrritationScreenEffect());
                    break;
            }

            _currentScreenEffect = null;
        }

        private IEnumerator PlayRedEdgeGlow()
        {
            if (screenOverlay == null) yield break;

            screenOverlay.gameObject.SetActive(true);
            screenOverlay.color = new Color(1f, 0f, 0f, 0f);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Sin(t * Mathf.PI) * 0.6f;
                screenOverlay.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }

            screenOverlay.color = new Color(1f, 0f, 0f, 0f);
            screenOverlay.gameObject.SetActive(false);
        }

        private IEnumerator PlayScreenShake(float duration, float intensity)
        {
            if (mainCamera == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - (elapsed / duration);
                Vector2 offset = UnityEngine.Random.insideUnitCircle * intensity * decay;
                mainCamera.transform.localPosition = _originalCameraPos + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            mainCamera.transform.localPosition = _originalCameraPos;
        }

        private IEnumerator PlayBlueTintWithRainbow()
        {
            if (screenOverlay == null) yield break;

            screenOverlay.gameObject.SetActive(true);

            float tintDuration = 0.8f;
            float elapsed = 0f;

            while (elapsed < tintDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / tintDuration;
                float alpha = t < 0.3f ? (t / 0.3f) : (1f - (t - 0.3f) / 0.7f);
                screenOverlay.color = new Color(0.2f, 0.3f, 0.8f, alpha * 0.4f);
                yield return null;
            }

            screenOverlay.color = new Color(0f, 0f, 0f, 0f);
            screenOverlay.gameObject.SetActive(false);
        }

        private IEnumerator PlayIrritationScreenEffect()
        {
            yield return StartCoroutine(PlayScreenShake(0.5f, 0.5f));

            if (screenOverlay != null)
            {
                screenOverlay.gameObject.SetActive(true);
                screenOverlay.color = new Color(1f, 1f, 1f, 0.8f);

                float flashDuration = 0.2f;
                float elapsed = 0f;

                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = 0.8f * (1f - elapsed / flashDuration);
                    screenOverlay.color = new Color(1f, 1f, 1f, alpha);
                    yield return null;
                }

                screenOverlay.color = new Color(0f, 0f, 0f, 0f);
                screenOverlay.gameObject.SetActive(false);
            }
        }

        public void StopAllEffects()
        {
            StopAllCoroutines();
            _currentScreenEffect = null;

            if (_particlePool != null)
            {
                foreach (var pool in _particlePool.Values)
                {
                    foreach (var ps in pool)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        ps.gameObject.SetActive(false);
                    }
                }
            }

            if (screenOverlay != null)
            {
                screenOverlay.color = new Color(0f, 0f, 0f, 0f);
                screenOverlay.gameObject.SetActive(false);
            }

            if (mainCamera != null)
                mainCamera.transform.localPosition = _originalCameraPos;
        }

        public void PlayEmotionSound(EmotionType type)
        {
            if (_audioClips == null) return;

            if (_audioClips.TryGetValue(type, out AudioClip clip) && clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
