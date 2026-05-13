using UnityEngine;
using System;

public enum GameState
{
    MainMenu,
    Feeding,
    BossBattle,
    Evolution,
    Paused
}

[Serializable]
public class GameData
{
    public int FeedingCount;
    public float Energy;
    public int EvolutionState;
    public string MonsterName;
    public float MonsterHappiness;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState, GameState> OnStateChanged;
    public event Action OnGamePause;
    public event Action OnGameResume;

    [Header("Managers")]
    public InputManager InputManager;
    public AudioManager AudioManager;

    [Header("Energy System")]
    public float MaxEnergy = 100f;
    public float EnergyPerFeed = 10f;
    public float CurrentEnergy { get; private set; }

    [Header("Game Data")]
    public GameData Data { get; private set; }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    private const string SaveKey = "StressMonster_SaveData";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentState = GameState.MainMenu;
        PreviousState = GameState.MainMenu;
        CurrentEnergy = 0f;
        Data = new GameData
        {
            FeedingCount = 0,
            Energy = 0f,
            EvolutionState = 0,
            MonsterName = "Stress Monster",
            MonsterHappiness = 50f
        };

        LoadGame();
    }

    private void Start()
    {
        if (InputManager == null)
            InputManager = FindObjectOfType<InputManager>();
        if (AudioManager == null)
            AudioManager = FindObjectOfType<AudioManager>();
    }

    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState)
            return;

        if (CurrentState == GameState.Paused && newState != GameState.Paused)
        {
            ResumeGame();
            return;
        }

        PreviousState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(PreviousState, CurrentState);

        if (CurrentState == GameState.BossBattle)
        {
            CurrentEnergy = 0f;
            Data.Energy = 0f;
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Paused)
            return;

        PreviousState = CurrentState;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
        OnStateChanged?.Invoke(PreviousState, CurrentState);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;

        CurrentState = PreviousState;
        PreviousState = GameState.Paused;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
        OnStateChanged?.Invoke(GameState.Paused, CurrentState);
    }

    public void FeedMonster()
    {
        if (CurrentState != GameState.Feeding)
            return;

        CurrentEnergy = Mathf.Min(CurrentEnergy + EnergyPerFeed, MaxEnergy);
        Data.FeedingCount++;
        Data.Energy = CurrentEnergy;
        Data.MonsterHappiness = Mathf.Min(Data.MonsterHappiness + 2f, 100f);

        if (CurrentEnergy >= MaxEnergy)
        {
            ChangeState(GameState.BossBattle);
        }
    }

    public void FeedMonster(float amount)
    {
        if (CurrentState != GameState.Feeding)
            return;

        CurrentEnergy = Mathf.Min(CurrentEnergy + amount, MaxEnergy);
        Data.FeedingCount++;
        Data.Energy = CurrentEnergy;
        Data.MonsterHappiness = Mathf.Min(Data.MonsterHappiness + 2f, 100f);

        if (CurrentEnergy >= MaxEnergy)
        {
            ChangeState(GameState.BossBattle);
        }
    }

    public void CompleteBossBattle(bool victory)
    {
        if (CurrentState != GameState.BossBattle)
            return;

        if (victory)
        {
            Data.EvolutionState++;
            Data.MonsterHappiness = Mathf.Min(Data.MonsterHappiness + 20f, 100f);
            ChangeState(GameState.Evolution);
        }
        else
        {
            CurrentEnergy = 0f;
            Data.Energy = 0f;
            ChangeState(GameState.Feeding);
        }
    }

    public void CompleteEvolution()
    {
        if (CurrentState != GameState.Evolution)
            return;

        CurrentEnergy = 0f;
        Data.Energy = 0f;
        ChangeState(GameState.Feeding);
    }

    public void SaveGame()
    {
        Data.Energy = CurrentEnergy;
        var json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        var json = PlayerPrefs.GetString(SaveKey);
        try
        {
            Data = JsonUtility.FromJson<GameData>(json);
            CurrentEnergy = Data.Energy;
        }
        catch (Exception)
        {
            Data = new GameData
            {
                FeedingCount = 0,
                Energy = 0f,
                EvolutionState = 0,
                MonsterName = "Stress Monster",
                MonsterHappiness = 50f
            };
            CurrentEnergy = 0f;
        }
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Data = new GameData
        {
            FeedingCount = 0,
            Energy = 0f,
            EvolutionState = 0,
            MonsterName = "Stress Monster",
            MonsterHappiness = 50f
        };
        CurrentEnergy = 0f;
    }

    public float GetEnergyNormalized()
    {
        return MaxEnergy > 0f ? CurrentEnergy / MaxEnergy : 0f;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
            if (CurrentState != GameState.MainMenu && CurrentState != GameState.Paused)
            {
                PauseGame();
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
