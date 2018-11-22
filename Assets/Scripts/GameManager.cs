using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GamePhase
    {
        PlayerTurn,
        Learning,
        IaTurn,
        ChangingTurn,
        GameStart,
        EndGame
    }

    private enum PlayingTurn
    {
        Player = 0,
        Ia = 1
    }
    
    private enum LearningAgent
    {
        Agent1,
        Agent2
    }

    public Cell[] Cells = new Cell[9];
    public GamePhase GameState = GamePhase.GameStart;
    public bool IsGameFinished;
    public AiBrain[] Brains = new AiBrain[2];
    public float WaitingTime = 0.01f;
    public bool IsLearning;
    public Session LearningSession;
    public string SessionFileName = "session100";
    public int SessionIterations = 200;
    private BoardGenerator _board;
    private Player _player;
    private PlayingTurn _lastPlayingTurn;
    [SerializeField] private bool _isPlayerPlaying;
    public Cell.CellOwner Winner { get; private set; }

    [SerializeField] private LearningAgent _learningAgent;


    protected override void Awake()
    {
        base.Awake();
        Assert.raiseExceptions = true;
        
        GatherReferences();

        //todo refactor
        // if is not playing the player then is learning
        if (!_isPlayerPlaying)
        {
            IsLearning = true;
            LearningSession = new Session(0.5f, 0.8f, 0.5f,SessionIterations);
        }
        else
        {
            //load the corresponding filename
            IsLearning = false;
            Brains[0].IsUsingFileData = true;
            LearningSession = new Session(SessionFileName);       
        } 

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator GameLoop()
    {
        while (!IsGameFinished)
        {
            switch (GameState)
            {
                case GamePhase.EndGame:
                    LearningSession.UpdateHyperParamters();
                    IsGameFinished = true;
                    yield return null;
                    break;
                
                case GamePhase.GameStart:
                    yield return new WaitForSeconds(WaitingTime);
                    StartGame();
                    yield return null;
                    break;
                
                case GamePhase.PlayerTurn:
                    while (!_player.HasPlacedAToken)
                    {
                        yield return new WaitForSeconds(WaitingTime);
                        Debug.Log("Selecciona una ficha por favor.");
                    }

                    _player.HasPlacedAToken = false;
                    GameState = GamePhase.ChangingTurn;
                    _lastPlayingTurn = PlayingTurn.Player;
                    
                    if (IsGameEnded())
                        GameState = GamePhase.EndGame;
                    yield return null;
                    break;
                
                case GamePhase.ChangingTurn:
                    yield return new WaitForSeconds(WaitingTime);
                    
                    if (!IsLearning)
                        GameState = _lastPlayingTurn == PlayingTurn.Player ? GamePhase.IaTurn : GamePhase.PlayerTurn;
                        
                    yield return null;
                    break;
                
                //todo refactor
                case GamePhase.Learning:
                    //hacer que haga un entrenamiento si es primero y otro si es segundo
                    LearningTurn();    
                   
                    if (IsGameEnded())
                        GameState = GamePhase.EndGame;

                    yield return null;
                    break;
                
                case GamePhase.IaTurn:
                    
                    if (!IsLearning)
                    {
                        Brains[0].ProcessAgentPlay();
                        _lastPlayingTurn = PlayingTurn.Ia;
                        GameState = GamePhase.ChangingTurn;
                    }
                    
                    if (IsGameEnded())
                        GameState = GamePhase.EndGame;
                    
                    
                    yield return null;
                    break;
                    
            }
        }
        
        Debug.Log("Reiniciando el juego");
        SceneManager.LoadScene(0); //todo load learning session or playing game
    }

    private void LearningTurn()
    {
        if (_learningAgent == LearningAgent.Agent1)
        {
            Brains[0].IsAgentLearningThisTurn = true;
            Brains[1].IsAgentLearningThisTurn = false;
            Brains[0].ProcessAgentPlay();
            Brains[1].ProcessAgentPlay();
            LearningSession.UpdateQValue(Brains[0].LastPlay,Cell.CellOwner.Agent1);
        }
        else
        {
            Brains[0].IsAgentLearningThisTurn = false;
            Brains[1].IsAgentLearningThisTurn = true;
            Brains[1].ProcessAgentPlay();
            Brains[0].ProcessAgentPlay();
            LearningSession.UpdateQValue(Brains[1].LastPlay,Cell.CellOwner.Agent2);
        }
    }

    private void StartGame()
    {
        if (!IsLearning)
            GameState = Random.value > 0.5f ? GamePhase.PlayerTurn : GamePhase.IaTurn;
        else
        {
            //refactor
            if (IsLearning)
                _learningAgent = Random.value >= 0.5 ? LearningAgent.Agent1 : LearningAgent.Agent2;
            
            if (_learningAgent == LearningAgent.Agent2)
            {
                Brains[0].IsAgentLearningThisTurn = false;
                Brains[1].IsAgentLearningThisTurn = true;
                Brains[0].ProcessAgentPlay();
            }
            else
            {
                Brains[0].IsAgentLearningThisTurn = true;
                Brains[1].IsAgentLearningThisTurn = false;
            }
            
            GameState = GamePhase.Learning;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GatherReferences();
        InitGame();
    }

    private void InitGame()
    {
        IsGameFinished = false;
        Cells = new Cell[9];
        _board.GenerateBoard();
        Winner = Cell.CellOwner.None;
        GameState =  GamePhase.GameStart;
        StartCoroutine(GameLoop());
    }
    
    private void GatherReferences()
    {
        //find the board and check for it
        _board = FindObjectOfType<BoardGenerator>();

        if (_board == null)
            Debug.LogError("No Board found.");

        
        //find the player and check if its training or playing
        _player = FindObjectOfType<Player>();

        if (_player == null)
        {
            Debug.LogError("Not player Found");
            _isPlayerPlaying = false;
            Brains[1] = GameObject.FindGameObjectWithTag("Agent2").GetComponent<AiBrain>();
        }
        else
        {
            _isPlayerPlaying = true;
        }

        Brains[0] = GameObject.FindGameObjectWithTag("Agent1").GetComponent<AiBrain>();
        
        if(!Brains[0].IsUsingFileData)
            Brains[1] = GameObject.FindGameObjectWithTag("Agent2").GetComponent<AiBrain>();

        if(Brains == null)
            Debug.LogError("Not brains found please drag them in the inspector. Brain 0 trains and brain 1 plays randomly");

        if (Brains.Length > 1  && _isPlayerPlaying)
        {
            Debug.Log("There are 2 brains and one player, please remove one brain.");
        }
    }
    
    public bool IsGameEnded()
    {
        if (CheckIfAllOwned()) return true;

        foreach (var cell in Cells)
        {
            Cell.WinnerData winData = cell.IsAWinnerCombination();

            if (winData.IsWinner)
            {
                Winner = winData.Owner;
                return true;
            }
        }

        return false;
    }

    private bool CheckIfAllOwned()
    {
        foreach (var cell in Cells)
        {
            if (cell.owner == Cell.CellOwner.None)
                return false;
        }

        return true;
    }

    public Cell.CellOwner[] GetCellsOwner(Cell[] cells)
    {
        Cell.CellOwner[] cellOwners = new Cell.CellOwner[9];
        
        for (var index = 0; index < GameManager.I.Cells.Length; index++)
        {
            var cell = GameManager.I.Cells[index];

            if (Brains[0].IsUsingFileData && cell.owner == Cell.CellOwner.Player)
            {
                cellOwners[index] = Cell.CellOwner.Agent2;
                continue;
            }
            
            cellOwners[index] = cell.owner;
        }

        return cellOwners;
    }
}
