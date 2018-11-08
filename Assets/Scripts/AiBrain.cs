using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiBrain : MonoBehaviour
{
	private int AIBrainID;
	
	private void Start()
	{
		AIBrainID = Array.IndexOf(GameManager.I.Brains, this);
	}

	public void ProcessAgentPlay()
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

		if(AIBrainID == 0)
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
