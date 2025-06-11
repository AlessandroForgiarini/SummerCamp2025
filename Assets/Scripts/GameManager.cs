using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const string HighscoreLabelPlayerPrefs = "playerHighscore";

    public enum GameState
    {
        Loading,
        WaitToPlay,
        Playing,
        EndGame
    }
    
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelSO currentLevelSo;
    [SerializeField] private ElementsListSO elementsListSo;
    
    private List<LevelSO.GoblinWaveData> _goblinWaves;
    
    [SerializeField] private GoblinSpawner goblinSpawner;
    [SerializeField] private UserInterfaceManager interfaceManager;
    
    private GameState _currentState = GameState.Loading;
    private int _currentGoblinWaveIndex;
    private int _totalGoblinToBanish;
    private int _goblinBanishedCount;

    [SerializeField] private GameObject interactableCrystals;
    GameObject presentCrystals;

    private void Awake()
    {
        Instance = this;
        _goblinWaves = currentLevelSo.goblinWaves;

        presentCrystals = GameObject.Find("CollectibleCrystals");
        ResetCrystals();
    }

    private void Start()
    {
        LoadMainMenu();
    }
    
    private void Update()
    {
        HandleGameManagerState();
    }
    
    private void UpdateState(GameState newState)
    {
        if (_currentState == newState)
        {
            Debug.LogWarning($"Updating game state with same state: [{newState}]");
            return;
        }
        
        _currentState = newState;
        interfaceManager.ShowPanel(newState);
    }

    private void HandleGameManagerState()
    {
        switch (_currentState)
        {
            case GameState.Loading:
                break;
            case GameState.WaitToPlay:
                break;
            case GameState.Playing:
                if (goblinSpawner.SpawnedAllGoblins && _currentGoblinWaveIndex < _goblinWaves.Count - 1)
                {
                    LoadGoblinWave(_currentGoblinWaveIndex + 1);
                }

                if (_goblinBanishedCount == _totalGoblinToBanish)
                {
                    GameOver(true);
                }
                break;
            case GameState.EndGame:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void LoadMainMenu()
    {
        // Loading saved HighScore
        int currentHighScore = PlayerPrefs.GetInt(HighscoreLabelPlayerPrefs, 0);
        interfaceManager.UpdateMainMenuHighScore(currentHighScore);

        foreach (LevelSO.GoblinWaveData waveData in _goblinWaves)
        {
            _totalGoblinToBanish += waveData.totalGoblins;
        }
        
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        List<ElementsListSO.ElementData> elements = elementsListSo.elements;
        List<ElementsListSO.ElementData> validElements = 
            elements.Where(el => el.type != ElementsListSO.ElementType.INVALID).ToList();
        
        foreach (GameObject tower in towers)
        {
            BallSpawner[] spawners = tower.GetComponentsInChildren<BallSpawner>();
            int counter = 0;
            foreach (BallSpawner spawner in spawners)
            {
                int index = counter % validElements.Count;
                spawner.SetSpawnerElement(validElements[index].type);
                counter++;
            }
        }
        
        UpdateState(GameState.WaitToPlay);
    }

    public void StartGame()
    {
        ResetCrystals();
        DestroyAllBalls();
        
        _goblinBanishedCount = 0;
        LoadGoblinWave(0);

        UpdateState(GameState.Playing);
    }

    private void ResetCrystals()
    {
        foreach (var o in GameObject.FindGameObjectsWithTag("Crystal"))
        {
            Destroy(o);
        }
        Vector3 crystalsPos = Vector3.zero;
        if (presentCrystals != null)
        {
            crystalsPos = presentCrystals.transform.position;
            Destroy(presentCrystals);
        }
        presentCrystals = Instantiate(interactableCrystals, crystalsPos, Quaternion.identity);
        
    }

    public void GameOver(bool win)
    {
        DestroyAllBalls();
        DestroyAllGoblins();
        goblinSpawner.DisableSpawner();
        
        // handling highscore
        int currentHighScore = PlayerPrefs.GetInt(HighscoreLabelPlayerPrefs, 0);
        
        var crystalHandlers = GameObject.FindGameObjectsWithTag("Crystal");
        int currentScore = 0;
        foreach (var crystal in crystalHandlers)
        {
            currentScore += crystal.GetComponent<CrystalHandler>().Score;
        }
        
        bool newHighScore = currentScore > currentHighScore;
        if (newHighScore)
        {
            // saving new highscore
            PlayerPrefs.SetInt(HighscoreLabelPlayerPrefs, currentScore);
            PlayerPrefs.Save();
        }

        interfaceManager.UpdateGameOverUI(currentScore, newHighScore, win);
        
        UpdateState(GameState.EndGame);
    }

    public void RestartGame()
    {
        StartGame();
    }

    private void LoadGoblinWave(int index)
    {
        _currentGoblinWaveIndex = index;
        LevelSO.GoblinWaveData data = _goblinWaves[_currentGoblinWaveIndex];
        goblinSpawner.EnableSpawner(data);
    }
    
    public void DestroyAllBalls()
    {
        BallHandler[] balls = FindObjectsByType<BallHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (BallHandler ball in balls)
        {
            ball.DestroyBall();
        }
    }

    public void DestroyAllGoblins()
    {
        GoblinController[] goblins = FindObjectsByType<GoblinController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (GoblinController goblin in goblins)
        {
            goblin.RemoveGoblin();
        }
    }

    public void GoblinBanished()
    {
        _goblinBanishedCount += 1;
    }

    public void RemovedCrystal()
    {        
        var crystalHandlers = GameObject.FindGameObjectsWithTag("Crystal");
        if (crystalHandlers.Length == 0)
        {
            GameOver(false);
        }
    }
}
