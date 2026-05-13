using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StressMonster.Emotion
{
    [CreateAssetMenu(fileName = "EmotionKeywordDB", menuName = "StressMonster/EmotionKeywordDB")]
    public class EmotionKeywordDB : ScriptableObject
    {
        [SerializeField] private List<EmotionKeywordEntry> entries = new List<EmotionKeywordEntry>();

        private Dictionary<EmotionType, List<string>> _keywordMap;
        private Dictionary<string, EmotionType> _wordToEmotion;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _keywordMap = new Dictionary<EmotionType, List<string>>();
            _wordToEmotion = new Dictionary<string, EmotionType>();

            foreach (var entry in entries)
            {
                if (!_keywordMap.ContainsKey(entry.emotionType))
                    _keywordMap[entry.emotionType] = new List<string>();

                foreach (string word in entry.keywords)
                {
                    if (!_keywordMap[entry.emotionType].Contains(word))
                        _keywordMap[entry.emotionType].Add(word);

                    if (!_wordToEmotion.ContainsKey(word))
                        _wordToEmotion[word] = entry.emotionType;
                }
            }
        }

        public EmotionType GetEmotionType(string keyword)
        {
            if (_wordToEmotion == null)
                BuildLookup();

            if (_wordToEmotion.TryGetValue(keyword, out EmotionType emotion))
                return emotion;

            return EmotionType.Irritation;
        }

        public string GetRandomKeyword(EmotionType type)
        {
            if (_keywordMap == null)
                BuildLookup();

            if (_keywordMap.TryGetValue(type, out List<string> keywords) && keywords.Count > 0)
                return keywords[UnityEngine.Random.Range(0, keywords.Count)];

            return string.Empty;
        }

        public List<string> GetAllKeywords()
        {
            if (_keywordMap == null)
                BuildLookup();

            var all = new List<string>();
            foreach (var kvp in _keywordMap)
                all.AddRange(kvp.Value);

            return all;
        }

        public void AddKeyword(EmotionType type, string keyword)
        {
            if (_keywordMap == null)
                BuildLookup();

            if (!_keywordMap.ContainsKey(type))
            {
                _keywordMap[type] = new List<string>();
                entries.Add(new EmotionKeywordEntry { emotionType = type, keywords = new List<string>() });
            }

            if (!_keywordMap[type].Contains(keyword))
            {
                _keywordMap[type].Add(keyword);

                foreach (var entry in entries)
                {
                    if (entry.emotionType == type && !entry.keywords.Contains(keyword))
                    {
                        entry.keywords.Add(keyword);
                        break;
                    }
                }
            }

            if (!_wordToEmotion.ContainsKey(keyword))
                _wordToEmotion[keyword] = type;
        }

        public void LoadFromJSON(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, path);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"EmotionKeywordDB: JSON file not found at {fullPath}");
                return;
            }

            string json = File.ReadAllText(fullPath);
            var data = JsonUtility.FromJson<EmotionKeywordDBData>(json);
            if (data == null || data.entries == null)
            {
                Debug.LogWarning("EmotionKeywordDB: Failed to parse JSON data");
                return;
            }

            entries.Clear();
            foreach (var entry in data.entries)
            {
                entries.Add(new EmotionKeywordEntry
                {
                    emotionType = entry.emotionType,
                    keywords = new List<string>(entry.keywords)
                });
            }

            BuildLookup();
        }

        public static EmotionKeywordDB CreateDefault()
        {
            var db = CreateInstance<EmotionKeywordDB>();
            db.entries = new List<EmotionKeywordEntry>
            {
                new EmotionKeywordEntry
                {
                    emotionType = EmotionType.Anger,
                    keywords = new List<string>
                    {
                        "加班", "KPI", "裁员", "996", "不公平",
                        "吵架", "被骂", "压榨", "剥削", "欺骗"
                    }
                },
                new EmotionKeywordEntry
                {
                    emotionType = EmotionType.Anxiety,
                    keywords = new List<string>
                    {
                        "考试", "面试", "deadline", "房贷", "未来",
                        "失业", "业绩", "竞争", "催促", "不确定"
                    }
                },
                new EmotionKeywordEntry
                {
                    emotionType = EmotionType.Sadness,
                    keywords = new List<string>
                    {
                        "分手", "失去", "孤独", "被误解", "离别",
                        "失败", "遗憾", "想念", "心碎", "无助"
                    }
                },
                new EmotionKeywordEntry
                {
                    emotionType = EmotionType.Irritation,
                    keywords = new List<string>
                    {
                        "堵车", "排队", "失眠", "噪音", "磨蹭",
                        "拖延", "烂代码", "改需求", "等待", "打扰"
                    }
                }
            };
            db.BuildLookup();
            return db;
        }
    }

    [Serializable]
    public class EmotionKeywordEntry
    {
        public EmotionType emotionType;
        public List<string> keywords;
    }

    [Serializable]
    public class EmotionKeywordDBData
    {
        public List<EmotionKeywordEntry> entries;
    }
}
