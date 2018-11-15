using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiBrain : MonoBehaviour
{
	private int AIBrainID;
	public bool IsAgentLearning;
	private Session.GameState lastPlay;
	
	private void Start()
	{
		AIBrainID = Array.IndexOf(GameManager.I.Brains, this);
	}

	public void ProcessAgentPlay()
	{
		if (!IsAgentLearning)
		{
			SelectRandomCell();
		}
		else
		{
			lastPlay = ObserveState();
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
		Cell.CellOwner owner = AIBrainID == 0 ? Cell.CellOwner.Agent1 : Cell.CellOwner.Agent2;
		int reward = GameManager.I.LearningSession.Reward(owner);

		Session session = GameManager.I.LearningSession;

		float newQ = session.QDictionary[lastPlay]
		             + session.LearningRate
		             * (reward +
		                (session.DiscountFactor * session.CheckBestQValueAtGameState(lastPlay.Cells))
		                - session.QDictionary[lastPlay]);

		session.QDictionary[lastPlay] = newQ;
		session.UpdateHyperParamters();
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

		if (AIBrainID == 0)
			GameManager.I.Cells[randomCell].owner = Cell.CellOwner.Agent1;
		else if (AIBrainID == 1)
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
