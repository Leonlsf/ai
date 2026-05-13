using System;
using System.Collections.Generic;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Monster
{
    public enum EvolutionSlot
    {
        Head,
        Body,
        Arms,
        Tail
    }

    [Serializable]
    public class EvolutionFeature
    {
        public EvolutionSlot slot;
        public EmotionType dominantEmotion;
        public string featureName;
        public Sprite sprite;
    }

    public class MonsterEvolution : MonoBehaviour
    {
        public List<EvolutionFeature> evolutionFeatures = new List<EvolutionFeature>();
        public MonsterAnimation monsterAnimation;

        private EvolutionStage _currentStage = EvolutionStage.Baby;
        private Dictionary<(EvolutionSlot, EmotionType), EvolutionFeature> _featureLookup;

        public event Action<EvolutionStage, List<EvolutionFeature>> OnEvolved;

        private void Awake()
        {
            if (monsterAnimation == null)
            {
                monsterAnimation = GetComponent<MonsterAnimation>();
            }

            BuildFeatureLookup();
        }

        private void BuildFeatureLookup()
        {
            _featureLookup = new Dictionary<(EvolutionSlot, EmotionType), EvolutionFeature>();

            foreach (var feature in evolutionFeatures)
            {
                var key = (feature.slot, feature.dominantEmotion);
                if (!_featureLookup.ContainsKey(key))
                {
                    _featureLookup[key] = feature;
                }
            }
        }

        public bool CheckEvolution(MonsterStats stats)
        {
            if (stats == null) return false;

            EvolutionStage newStage = stats.GetCurrentStage();
            return newStage != _currentStage;
        }

        public EvolutionFeature GetEvolutionFeature(EvolutionSlot slot, EmotionType emotion)
        {
            if (_featureLookup == null)
            {
                BuildFeatureLookup();
            }

            if (_featureLookup.TryGetValue((slot, emotion), out EvolutionFeature feature))
            {
                return feature;
            }

            return null;
        }

        public void ApplyEvolution(MonsterStats stats)
        {
            if (stats == null) return;

            EvolutionStage newStage = stats.GetCurrentStage();

            if (newStage == _currentStage) return;

            EmotionType dominant = stats.GetDominantEmotion();
            List<EvolutionFeature> activeFeatures = new List<EvolutionFeature>();

            EvolutionSlot[] slots = (EvolutionSlot[])Enum.GetValues(typeof(EvolutionSlot));
            foreach (EvolutionSlot slot in slots)
            {
                EvolutionFeature feature = GetEvolutionFeature(slot, dominant);
                if (feature != null)
                {
                    activeFeatures.Add(feature);
                }
            }

            EvolutionStage previousStage = _currentStage;
            _currentStage = newStage;

            if (monsterAnimation != null)
            {
                monsterAnimation.PlayEvolutionAnimation();
            }

            OnEvolved?.Invoke(newStage, activeFeatures);
        }

        public EvolutionStage GetCurrentStage()
        {
            return _currentStage;
        }

        public List<EvolutionFeature> GetCurrentFeatures(MonsterStats stats)
        {
            if (stats == null) return new List<EvolutionFeature>();

            EmotionType dominant = stats.GetDominantEmotion();
            List<EvolutionFeature> features = new List<EvolutionFeature>();

            EvolutionSlot[] slots = (EvolutionSlot[])Enum.GetValues(typeof(EvolutionSlot));
            foreach (EvolutionSlot slot in slots)
            {
                EvolutionFeature feature = GetEvolutionFeature(slot, dominant);
                if (feature != null)
                {
                    features.Add(feature);
                }
            }

            return features;
        }

        public static List<EvolutionFeature> CreateDefaultFeatureSet()
        {
            List<EvolutionFeature> features = new List<EvolutionFeature>
            {
                new EvolutionFeature { slot = EvolutionSlot.Head, dominantEmotion = EmotionType.Anger, featureName = "FlameHorns" },
                new EvolutionFeature { slot = EvolutionSlot.Head, dominantEmotion = EmotionType.Anxiety, featureName = "ThunderEars" },
                new EvolutionFeature { slot = EvolutionSlot.Head, dominantEmotion = EmotionType.Sadness, featureName = "RainCloudHat" },
                new EvolutionFeature { slot = EvolutionSlot.Head, dominantEmotion = EmotionType.Irritation, featureName = "ExplosionHair" },

                new EvolutionFeature { slot = EvolutionSlot.Body, dominantEmotion = EmotionType.Anger, featureName = "LavaPattern" },
                new EvolutionFeature { slot = EvolutionSlot.Body, dominantEmotion = EmotionType.Anxiety, featureName = "ElectricSkin" },
                new EvolutionFeature { slot = EvolutionSlot.Body, dominantEmotion = EmotionType.Sadness, featureName = "CrystalBody" },
                new EvolutionFeature { slot = EvolutionSlot.Body, dominantEmotion = EmotionType.Irritation, featureName = "ArmorShell" },

                new EvolutionFeature { slot = EvolutionSlot.Arms, dominantEmotion = EmotionType.Anger, featureName = "FireFist" },
                new EvolutionFeature { slot = EvolutionSlot.Arms, dominantEmotion = EmotionType.Anxiety, featureName = "ThunderClaw" },
                new EvolutionFeature { slot = EvolutionSlot.Arms, dominantEmotion = EmotionType.Sadness, featureName = "HealingHands" },
                new EvolutionFeature { slot = EvolutionSlot.Arms, dominantEmotion = EmotionType.Irritation, featureName = "BlastArm" },

                new EvolutionFeature { slot = EvolutionSlot.Tail, dominantEmotion = EmotionType.Anger, featureName = "FlameTail" },
                new EvolutionFeature { slot = EvolutionSlot.Tail, dominantEmotion = EmotionType.Anxiety, featureName = "LightningTail" },
                new EvolutionFeature { slot = EvolutionSlot.Tail, dominantEmotion = EmotionType.Sadness, featureName = "RainbowTail" },
                new EvolutionFeature { slot = EvolutionSlot.Tail, dominantEmotion = EmotionType.Irritation, featureName = "MissileTail" }
            };

            return features;
        }
    }
}
