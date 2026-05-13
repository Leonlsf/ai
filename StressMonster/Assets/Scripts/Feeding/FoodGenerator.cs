using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Feeding
{
    public class FoodGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject foodPrefab;
        [SerializeField] private Transform spawnArea;
        [SerializeField] private EmotionConverter emotionConverter;

        [Header("Sprite Mapping")]
        [SerializeField] private Sprite angerSprite;
        [SerializeField] private Sprite anxietySprite;
        [SerializeField] private Sprite sadnessSprite;
        [SerializeField] private Sprite irritationSprite;

        [Header("Auto Generate")]
        [SerializeField] private bool autoGenerate = false;
        [SerializeField] private float autoGenerateInterval = 3f;

        [Header("Spawn Area")]
        [SerializeField] private float spawnWidth = 6f;
        [SerializeField] private float spawnHeight = 3f;

        private readonly string[] _angerWords = { "加班", "KPI", "裁员", "996", "不公平" };
        private readonly string[] _anxietyWords = { "考试", "面试", "deadline", "房贷", "未来" };
        private readonly string[] _sadnessWords = { "分手", "失去", "孤独", "被误解", "离别" };
        private readonly string[] _irritationWords = { "堵车", "排队", "失眠", "噪音", "磨蹭" };

        private List<string> _allWords;
        private float _autoGenerateTimer;
        private Coroutine _batchCoroutine;

        public event Action<FoodItem> OnFoodGenerated;

        private void Awake()
        {
            BuildWordList();

            if (emotionConverter == null)
                InitializeDefaultConverter();
        }

        private void Update()
        {
            if (!autoGenerate) return;

            _autoGenerateTimer += Time.deltaTime;
            if (_autoGenerateTimer >= autoGenerateInterval)
            {
                _autoGenerateTimer = 0f;
                GenerateRandomFood();
            }
        }

        private void BuildWordList()
        {
            _allWords = new List<string>();
            _allWords.AddRange(_angerWords);
            _allWords.AddRange(_anxietyWords);
            _allWords.AddRange(_sadnessWords);
            _allWords.AddRange(_irritationWords);
        }

        private void InitializeDefaultConverter()
        {
            emotionConverter = ScriptableObject.CreateInstance<EmotionConverter>();
            emotionConverter.AddWordEntry(EmotionType.Anger, _angerWords);
            emotionConverter.AddWordEntry(EmotionType.Anxiety, _anxietyWords);
            emotionConverter.AddWordEntry(EmotionType.Sadness, _sadnessWords);
            emotionConverter.AddWordEntry(EmotionType.Irritation, _irritationWords);
        }

        public FoodItem GenerateFood(string stressWord)
        {
            if (foodPrefab == null)
            {
                Debug.LogError("FoodGenerator: foodPrefab is not assigned.");
                return null;
            }

            EmotionType emotion = emotionConverter != null
                ? emotionConverter.ConvertWord(stressWord)
                : EmotionType.Anger;

            Sprite sprite = GetSpriteForEmotion(emotion);
            Vector3 position = GetRandomSpawnPosition();

            GameObject foodObj = Instantiate(foodPrefab, position, Quaternion.identity, spawnArea);
            FoodItem foodItem = foodObj.GetComponent<FoodItem>();

            if (foodItem == null)
                foodItem = foodObj.AddComponent<FoodItem>();

            foodItem.Initialize(stressWord, emotion, sprite);
            OnFoodGenerated?.Invoke(foodItem);

            return foodItem;
        }

        public void GenerateMultipleFood(string[] words)
        {
            if (_batchCoroutine != null)
                StopCoroutine(_batchCoroutine);

            _batchCoroutine = StartCoroutine(GenerateMultipleRoutine(words));
        }

        public FoodItem GenerateRandomFood()
        {
            if (_allWords == null || _allWords.Count == 0)
                BuildWordList();

            string randomWord = _allWords[UnityEngine.Random.Range(0, _allWords.Count)];
            return GenerateFood(randomWord);
        }

        public void SetAutoGenerate(bool enabled, float interval = 3f)
        {
            autoGenerate = enabled;
            autoGenerateInterval = interval;
            _autoGenerateTimer = 0f;
        }

        public void SetEmotionConverter(EmotionConverter converter)
        {
            emotionConverter = converter;
        }

        private Sprite GetSpriteForEmotion(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Anger:
                    return angerSprite;
                case EmotionType.Anxiety:
                    return anxietySprite;
                case EmotionType.Sadness:
                    return sadnessSprite;
                case EmotionType.Irritation:
                    return irritationSprite;
                default:
                    return angerSprite;
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 center = spawnArea != null ? spawnArea.position : Vector3.zero;
            float x = center.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f);
            float y = center.y + UnityEngine.Random.Range(-spawnHeight * 0.5f, spawnHeight * 0.5f);
            return new Vector3(x, y, center.z);
        }

        private IEnumerator GenerateMultipleRoutine(string[] words)
        {
            foreach (string word in words)
            {
                GenerateFood(word);
                yield return new WaitForSeconds(0.1f);
            }

            _batchCoroutine = null;
        }
    }
}
