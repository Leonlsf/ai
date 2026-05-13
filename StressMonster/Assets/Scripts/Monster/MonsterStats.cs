using System;
using StressMonster.Emotion;

namespace StressMonster.Monster
{
    [Serializable]
    public enum EvolutionStage
    {
        Baby,
        Growing,
        Mature,
        Ultimate
    }

    [Serializable]
    public class MonsterStats
    {
        public string monsterName;
        public int totalFeeds;
        public int angerFeeds;
        public int anxietyFeeds;
        public int sadnessFeeds;
        public int irritationFeeds;
        public float energy;
        public float maxEnergy = 100f;
        public int level;
        public float happiness;

        public MonsterStats()
        {
            monsterName = "Stress Monster";
            totalFeeds = 0;
            angerFeeds = 0;
            anxietyFeeds = 0;
            sadnessFeeds = 0;
            irritationFeeds = 0;
            energy = maxEnergy;
            level = 1;
            happiness = 50f;
        }

        public EvolutionStage GetCurrentStage()
        {
            if (totalFeeds >= 500) return EvolutionStage.Ultimate;
            if (totalFeeds >= 200) return EvolutionStage.Mature;
            if (totalFeeds >= 50) return EvolutionStage.Growing;
            return EvolutionStage.Baby;
        }

        public EmotionType GetDominantEmotion()
        {
            int max = Math.Max(Math.Max(angerFeeds, anxietyFeeds), Math.Max(sadnessFeeds, irritationFeeds));

            if (max == 0) return EmotionType.Anger;

            if (angerFeeds == max) return EmotionType.Anger;
            if (anxietyFeeds == max) return EmotionType.Anxiety;
            if (sadnessFeeds == max) return EmotionType.Sadness;
            return EmotionType.Irritation;
        }

        public void AddFeed(EmotionType emotion)
        {
            totalFeeds++;

            switch (emotion)
            {
                case EmotionType.Anger:
                    angerFeeds++;
                    break;
                case EmotionType.Anxiety:
                    anxietyFeeds++;
                    break;
                case EmotionType.Sadness:
                    sadnessFeeds++;
                    break;
                case EmotionType.Irritation:
                    irritationFeeds++;
                    break;
            }

            happiness = Math.Min(100f, happiness + 5f);

            int newLevel = totalFeeds / 10 + 1;
            if (newLevel > level)
            {
                level = newLevel;
            }
        }

        public void AddEnergy(float amount)
        {
            energy = Math.Min(maxEnergy, energy + amount);
        }

        public void ConsumeEnergy(float amount)
        {
            energy = Math.Max(0f, energy - amount);
            if (energy <= 0f)
            {
                happiness = Math.Max(0f, happiness - 2f);
            }
        }

        public void ResetStats()
        {
            monsterName = "Stress Monster";
            totalFeeds = 0;
            angerFeeds = 0;
            anxietyFeeds = 0;
            sadnessFeeds = 0;
            irritationFeeds = 0;
            energy = maxEnergy;
            level = 1;
            happiness = 50f;
        }
    }
}
