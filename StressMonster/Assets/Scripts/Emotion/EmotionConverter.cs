using System.Collections.Generic;
using UnityEngine;

namespace StressMonster.Emotion
{
    [CreateAssetMenu(fileName = "EmotionConverter", menuName = "StressMonster/EmotionConverter")]
    public class EmotionConverter : ScriptableObject
    {
        [SerializeField] private EmotionKeywordDB keywordDB;
        [SerializeField] private List<EmotionWordEntry> wordEntries = new List<EmotionWordEntry>();

        private Dictionary<string, EmotionType> _wordToEmotion;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _wordToEmotion = new Dictionary<string, EmotionType>();

            if (keywordDB != null)
            {
                List<string> allKeywords = keywordDB.GetAllKeywords();
                foreach (string word in allKeywords)
                {
                    if (!_wordToEmotion.ContainsKey(word))
                        _wordToEmotion[word] = keywordDB.GetEmotionType(word);
                }
            }

            foreach (var entry in wordEntries)
            {
                foreach (string word in entry.words)
                {
                    if (!_wordToEmotion.ContainsKey(word))
                    {
                        _wordToEmotion[word] = entry.emotionType;
                    }
                }
            }
        }

        public EmotionType ConvertWord(string word)
        {
            if (_wordToEmotion == null)
                BuildLookup();

            if (_wordToEmotion.TryGetValue(word, out EmotionType emotion))
                return emotion;

            if (keywordDB != null)
                return keywordDB.GetEmotionType(word);

            return EmotionType.Irritation;
        }

        public void AddWordEntry(EmotionType emotion, string[] words)
        {
            var entry = new EmotionWordEntry { emotionType = emotion, words = new List<string>(words) };
            wordEntries.Add(entry);
            BuildLookup();
        }

        public void SetKeywordDB(EmotionKeywordDB db)
        {
            keywordDB = db;
            BuildLookup();
        }
    }

    [System.Serializable]
    public class EmotionWordEntry
    {
        public EmotionType emotionType;
        public List<string> words;
    }
}
