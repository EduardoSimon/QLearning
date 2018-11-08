using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum Gamestate
    {
        PlayerTurn = 0,
        IaTurn = 1,
        ChangingTurn = 2,
        GameStart = 3,
        EndGame = 4
    }

    private enum Turn
    {
        Player = 0,
        Ia = 1
    }

    public Cell[] Cells = new Cell[9];
    public Gamestate GameState = Gamestate.GameStart;
    public bool IsGameFinished;

    private BoardGenerator _board;
    private Player _player;
    private AiBrain[] _brains = new AiBrain[2];
    private Turn _lastTurn;
    private bool _isPlayerPlaying;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        GatherReferences();
    }

    private void Start()
    {
        InitGame();
    }

    //todo add supoort for session and player against ia
    private IEnumerator GameLoop()
    {
        while (!IsGameFinished)
        {
            if (CheckEndGame())
                GameState = Gamestate.EndGame;
            
            switch (GameState)
            {
                case Gamestate.EndGame:
                    Debug.Log("Ending Game");
                    InitGame();
                    break;
                
                case Gamestate.GameStart:
                    yield return new WaitForSeconds(2);
                    if (_isPlayerPlaying)
                        GameState = Random.value > 0.5f ? Gamestate.PlayerTurn : Gamestate.IaTurn;
                    else
                        GameState = Gamestate.IaTurn;
                    break;
                
                case Gamestate.PlayerTurn:
                    Debug.Log("Your turn Player 1");
                    _player.ProcessPlayerInput();
                    GameState = Gamestate.ChangingTurn;
                    _lastTurn = Turn.Player;
                    break;
                
                case Gamestate.ChangingTurn:
                    Debug.Log("Changing turn.... ");
                    yield return new WaitForSeconds(2);
                    
                    if (_isPlayerPlaying)
                        GameState = _lastTurn == Turn.Player ? Gamestate.IaTurn : Gamestate.PlayerTurn;
                    else
                        GameState = Gamestate.IaTurn;
                    
                    break;
                
                case Gamestate.IaTurn:
                    Debug.Log("Enemy turn");

                    if (_isPlayerPlaying)
                    {
                        _brains[0].ProcessAgentPlay();
                        _lastTurn = Turn.Ia;
                        GameState = Gamestate.ChangingTurn;
                    }
                    else
                    {
                        _brains[0].ProcessAgentPlay();
                        _brains[1].ProcessAgentPlay();
                    }
                    
                    break;
          
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GatherReferences();
        InitGame(); // TODO add menu functionality
    }

    private void InitGame()
    {
        IsGameFinished = false;
        _board.GenerateBoard();
        StartCoroutine(GameLoop());
    }
    
    private void GatherReferences()
    {
        //find the board and check for it
        _board = FindObjectOfType<BoardGenerator>();
        
        if(_board == null)
            Debug.LogError("No Board found.");
        
        //find the player and check if its training or playing
        _player = FindObjectOfType<Player>();

        if (_player == null)
        {
            Debug.LogError("Not player Found");
            _isPlayerPlaying = false;
        }
        else
            _isPlayerPlaying = true;

        
        //check how many brains there are
        _brains = FindObjectsOfType<AiBrain>();
        
        if(_brains == null)
            Debug.LogError("Not brains found");

        if (_brains.Count(elem => elem != null) > 1  && _isPlayerPlaying)
        {
            Debug.Log("There are 2 brains and one player, please remove one brain.");

            while (_brains.Count(elem => elem != null) > 1)
            {
                _brains = FindObjectsOfType<AiBrain>();
            }
        }
    }
    
    private bool CheckEndGame()
    {
        throw new System.NotImplementedException();
    }
}
