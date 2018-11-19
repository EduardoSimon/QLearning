﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class Session {

	public class GameState
	{
		public Cell.CellOwner[] Cells;
		public int indexAction;

		public GameState(Cell.CellOwner[] cells, int indexAction)
		{
			this.Cells = cells;
			this.indexAction = indexAction;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			foreach (var cellOwner in Cells)
			{
				int i = (int)cellOwner;
				sb.Append(i +",");
			}
			return sb + ":" + indexAction;
		}
	}
	
	//Cada GameState posee una posible acción. Si no se encuentra la clave en el diccionario significa que aun no se ha explorado
	//Le ponemos como máxima capacidad todos los posibles estados sin discriminar erroneos 3^9 * cada acción posible por estado: 9
	public Dictionary<GameState, float> QDictionary { get; private set; }
	public float LearningRate { get; private set; }
	public float DiscountFactor { get; private set; }
	public static double Epsilon { get; private set; }
	public int Steps { get; private set; }
	public int MaxSteps { get; private set; }

	public Session(float learningRate, float discountFactor, double epsilon, int maxSteps)
	{
		QDictionary = new Dictionary<GameState, float>(Mathf.RoundToInt(Mathf.Pow(3,9) * 9));
		LearningRate = Mathf.Clamp01(learningRate);
		DiscountFactor = Mathf.Clamp01(discountFactor);
		Epsilon = epsilon;
		Steps = 0;
		MaxSteps = maxSteps;
	}

	public Session()
	{
	}


	public void SaveSessionFile(string sessionName)
	{
		using (StreamWriter file = new StreamWriter(Application.dataPath + sessionName + ".txt"))
		{
			foreach (var state in QDictionary.Keys)
			{
				file.WriteLine(state + ":" + QDictionary[state]);
			}
		}
	}
	
	public void LoadSessionFile(string sessionName)
	{
		Dictionary<GameState,float> qDictionary = new Dictionary<GameState, float>();
		
		using (StreamReader file = new StreamReader(Application.dataPath + sessionName + ".txt"))
		{
			//the string looks like this 0,0,0,0,0,0,0,0,0,:0:0
			string s = file.ReadLine();
			while (s != null)
			{
				String[] values = s.Split(':');
				String[] cells = values[0].Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
				
				GameState state = new GameState(new Cell.CellOwner[]
					{
						(Cell.CellOwner)int.Parse(cells[0]),
						(Cell.CellOwner)int.Parse(cells[1]),
						(Cell.CellOwner)int.Parse(cells[2]),
						(Cell.CellOwner)int.Parse(cells[3]),
						(Cell.CellOwner)int.Parse(cells[4]),
						(Cell.CellOwner)int.Parse(cells[5]),
						(Cell.CellOwner)int.Parse(cells[6]),
						(Cell.CellOwner)int.Parse(cells[7]),
						(Cell.CellOwner)int.Parse(cells[8])
					}, int.Parse(values[1]));
				float Q = float.Parse(values[2]);
				
				qDictionary[state] = Q;
				s = file.ReadLine();
			}
		}

		QDictionary = qDictionary;
	}

	public int Reward(Cell.CellOwner agent)
	{		
		if (!GameManager.I.IsGameEnded())
			return 0;

		if (agent == Cell.CellOwner.Agent1 && GameManager.I.Brains[0].IsAgentLearningThisTurn ||
		    agent == Cell.CellOwner.Agent2 && GameManager.I.Brains[1].IsAgentLearningThisTurn)
			return 1;
		
		if (GameManager.I.Brains[0].IsAgentLearningThisTurn && GameManager.I.Winner == Cell.CellOwner.Agent2 ||
		    GameManager.I.Brains[1].IsAgentLearningThisTurn && GameManager.I.Winner == Cell.CellOwner.Agent1)
			return -1;

		return 0;
	}

	public float CheckBestQValueAtGameState(Cell.CellOwner[] owners)
	{
		float tempMaxQ = 0;
		
		for (int i = 0; i < 9; i++)
		{
			GameState gameState = new GameState(owners,i);
			
			if(QDictionary.ContainsKey(gameState))
				if (QDictionary[gameState] > tempMaxQ)
					tempMaxQ = QDictionary[gameState];
			
		}
		
		return tempMaxQ;
	}

	private bool IsValidAction(GameState gameState)
	{
		return gameState.Cells[gameState.indexAction] == Cell.CellOwner.None;
	}

	public GameState CheckBestActionAtGameState(Cell.CellOwner[] owners)
	{
		GameState tempBestAction = null;
		float tempMaxQ = 0;
		
		for (int i = 0; i < 9; i++)
		{
			GameState gameState = new GameState(owners,i);

			if (IsValidAction(gameState))
			{
				if (QDictionary.ContainsKey(gameState))
				{
					if (QDictionary[gameState] > tempMaxQ)
					{
						tempMaxQ = QDictionary[gameState];
						tempBestAction = gameState;
					}
				}
				else
				{
					QDictionary[gameState] = 0;
					tempBestAction = gameState;
				}
				
			}
			
		}

		return tempBestAction;
	}

	public void UpdateHyperParamters()
	{
		//todo hacer decrease de learning rate, epsilon de manera mas eficiente. Y hacerlo visualizar
		if(LearningRate >= 0.01f) 
			LearningRate -= 0.001f;
		if(Epsilon >= 0.1f) 
			Epsilon -= 0.001f;
		Steps += 1;

		if (Steps >= MaxSteps && MaxSteps != 0)
			OnSessionCompleted();

	}

	private void OnSessionCompleted()
	{
		SaveSessionFile("session" + MaxSteps);
		Application.Quit();
		
		#if UNITY_EDITOR
			Debug.Break();
		#endif
	}
}
