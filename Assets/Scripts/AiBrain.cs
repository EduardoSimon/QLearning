using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiBrain : MonoBehaviour
{
	private int _aiBrainId;
	public bool IsAgentLearningThisTurn;
	public bool IsUsingFileData;
	public bool isFirstAgainstPlayer;
	public GameState LastPlay { get; private set; }

	private void Start()
	{
		_aiBrainId = Array.IndexOf(GameManager.I.Brains, this);
	}

	public void ProcessAgentPlay()
	{
		if (IsUsingFileData)
		{
			GameState play = GameManager.I.LearningSession.CheckBestActionAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),isFirstAgainstPlayer ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2);
			GameManager.I.Cells[play.IndexAction].owner = isFirstAgainstPlayer ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2;
			GameManager.I.Cells[play.IndexAction].UpdateColor();
		}
		else if(IsAgentLearningThisTurn)
		{
			LastPlay = ObserveState();
			GameManager.I.Cells[LastPlay.IndexAction].owner =  _aiBrainId == 0 ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2;
			GameManager.I.Cells[LastPlay.IndexAction].UpdateColor();
		}
		else if (!IsAgentLearningThisTurn)
		{
			SelectRandomCell();
		}

	}

	private GameState ObserveState()
	{
		float value = Random.value;

		if (value <= GameManager.I.LearningSession.Epsilon)
		{
			int action = Random.Range(0, 9);

			while (GameManager.I.Cells[action].owner != Cell.CellOwner.None)
			{
				action = Random.Range(0, 9);
			}
			
			return new GameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),action);
		}
		
		return GameManager.I.LearningSession.CheckBestActionAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),_aiBrainId == 0 ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2);
	}
	
	private void SelectRandomCell()
	{
		int randomCell = Random.Range(0, GameManager.I.Cells.Length);

		while (GameManager.I.Cells[randomCell].owner != Cell.CellOwner.None)
		{
			randomCell = Random.Range(0, GameManager.I.Cells.Length);

			if (!AreCellsLeft())
			{
				return;
			}
		}

		GameManager.I.Cells[randomCell].owner = _aiBrainId == 0 ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2;
		GameManager.I.Cells[randomCell].UpdateColor();
	}

	private bool AreCellsLeft()
	{
		foreach (var cell in GameManager.I.Cells)
		{
			if (cell.owner == Cell.CellOwner.None)
				return true;
		}

		return false;
	}

}
