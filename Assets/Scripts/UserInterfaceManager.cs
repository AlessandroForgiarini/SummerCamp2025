using System;
using System.Collections.Generic;
using UnityEngine;
using GameState = GameManager.GameState;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private GameOverUI gameOverUI;

    public void ShowPanel(GameState gameState)
    {
        // Mostrare l'interfaccia corretta
        switch (gameState)
        {
            case GameState.Loading:
                break;
            case GameState.WaitToPlay:
                mainMenuUI.ShowPanel();
                gameOverUI.HidePanel();
                break;
            case GameState.Playing:
                mainMenuUI.HidePanel();
                gameOverUI.HidePanel();
                break;
            case GameState.EndGame:
                mainMenuUI.HidePanel();
                gameOverUI.ShowPanel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameState), gameState, null);
        }
    }

    public void UpdateMainMenuHighScore(int currentHighScore)
    {
        mainMenuUI.UpdateHighScore(currentHighScore);
    }
    
    public void UpdateGameOverUI(int currentScore, bool newHighScore, bool win)
    {
        gameOverUI.UpdateUI(currentScore, newHighScore, win);
    }
}