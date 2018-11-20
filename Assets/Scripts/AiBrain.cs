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
	private Session.GameState _lastPlay;

	private void Start()
	{
		_aiBrainId = Array.IndexOf(GameManager.I.Brains, this);
	}

	public void ProcessAgentPlay()
	{
		if (IsUsingFileData)
		{
			Session.GameState play = GameManager.I.LearningSession.CheckBestActionAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells));
			GameManager.I.Cells[play.indexAction].owner = Cell.CellOwner.Agent1;
			GameManager.I.Cells[play.indexAction].UpdateColor();
		}
		else if(IsAgentLearningThisTurn)
		{
			_lastPlay = ObserveState();
			GameManager.I.Cells[_lastPlay.indexAction].owner =  Cell.CellOwner.Agent1;
			GameManager.I.Cells[_lastPlay.indexAction].UpdateColor();
		}
		else if (!IsAgentLearningThisTurn)
		{
			SelectRandomCell();
		}

	}

	private Session.GameState ObserveState()
	{
		float value = Random.value;

		if (value <= Session.Epsilon)
		{
			int action = Random.Range(0, 9);

			while (GameManager.I.Cells[action].owner != Cell.CellOwner.None)
			{
				action = Random.Range(0, 9);
			}
			
			return new Session.GameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),action);
		}
		
		return GameManager.I.LearningSession.CheckBestActionAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells));
	}

	public void UpdateQValue()
	{
		Cell.CellOwner owner = Cell.CellOwner.Agent1;
		int reward = GameManager.I.LearningSession.Reward(owner);

		Session session = GameManager.I.LearningSession;

		if (!session.QDictionary.ContainsKey(_lastPlay))
		{
			session.QDictionary[_lastPlay] = 0;
		}
			
		float newQ = session.QDictionary[_lastPlay]
		             + session.LearningRate * (reward + session.DiscountFactor * session.CheckBestQValueAtGameState(_lastPlay.Cells)  - session.QDictionary[_lastPlay]);

		session.QDictionary[_lastPlay] = newQ;
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

		GameManager.I.Cells[randomCell].owner = Cell.CellOwner.Agent2;
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
