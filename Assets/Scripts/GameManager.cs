using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
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
    public AiBrain[] Brains = new AiBrain[2];
    public float WaitingTime = 0.01f;
    public bool IsLearning;
    public Session LearningSession;
    public string SessionFileName = "session100";
    private BoardGenerator _board;
    private Player _player;
    private Turn _lastTurn;
    [SerializeField] private bool _isPlayerPlaying;
    public Cell.CellOwner Winner { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        
        //TODO RISKY junto con el sceneLoaded, puede que coja las referencias 2 veces y pete
        GatherReferences();

        if (!_isPlayerPlaying)
        {
            if(Brains[1].IsAgentLearning)
                Debug.LogError("Both agents are learning, please uncheck the learning toggle in the agent 2");
            
            IsLearning = Brains[0].IsAgentLearning;
            LearningSession = new Session(0.9f, 0.8f, 0.9,20000);
        }

        if (!IsLearning && Brains[0].isUsingFileData)
        {
            LearningSession = new Session();
            LearningSession.LoadSessionFile(SessionFileName);            
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
                case Gamestate.EndGame:
                    Debug.Log("Ending Game");
                    Debug.Log("The winner is: " + Winner);
                    IsGameFinished = true;
                    yield return null;
                    break;
                
                case Gamestate.GameStart:
                    yield return new WaitForSeconds(WaitingTime);
                    if (_isPlayerPlaying)
                        GameState = Random.value > 0.5f ? Gamestate.PlayerTurn : Gamestate.IaTurn;
                    else
                        GameState = Gamestate.IaTurn;

                    yield return null;
                    break;
                
                case Gamestate.PlayerTurn:
                    Debug.Log("Your turn Player 1");
                    while (!_player.HasPlacedAToken)
                    {
                        yield return new WaitForSeconds(WaitingTime);
                        Debug.Log("Selecciona una ficha por favor.");
                    }

                    _player.HasPlacedAToken = false;
                    GameState = Gamestate.ChangingTurn;
                    _lastTurn = Turn.Player;
                    
                    if (IsGameEnded())
                        GameState = Gamestate.EndGame;
                    yield return null;
                    break;
                
                case Gamestate.ChangingTurn:
                    Debug.Log("Changing turn.... ");
                    yield return new WaitForSeconds(WaitingTime);
                    
                    if (_isPlayerPlaying)
                        GameState = _lastTurn == Turn.Player ? Gamestate.IaTurn : Gamestate.PlayerTurn;
                    else
                        GameState = Gamestate.IaTurn;
                    yield return null;
                    break;
                
                case Gamestate.IaTurn:
                    Debug.Log("Enemy turn");

                    if (_isPlayerPlaying)
                    {
                        Brains[0].ProcessAgentPlay();
                        _lastTurn = Turn.Ia;
                        GameState = Gamestate.ChangingTurn;
                    }
                    else
                    {
                        //todo hacer que los dos esten aprendiendo y actuen sobre el mismo diccionario
                        Brains[0].ProcessAgentPlay();
                        Brains[1].ProcessAgentPlay();
                        Brains[0].UpdateQValue();
                    }
                    
                    if (IsGameEnded())
                        GameState = Gamestate.EndGame;

                    yield return null;
                    break;
            }
        }
        
        Debug.Log("Reiniciando el juego");
        SceneManager.LoadScene(0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GatherReferences();
        InitGame(); // TODO add menu functionality
    }

    private void InitGame()
    {
        IsGameFinished = false;
        Cells = new Cell[9];
        _board.GenerateBoard();
        Winner = Cell.CellOwner.None;
        GameState = Gamestate.GameStart;
        
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
            cellOwners[index] = cell.owner;
        }

        return cellOwners;
    }
}
