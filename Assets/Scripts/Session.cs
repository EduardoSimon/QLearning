using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[System.Serializable]
public class Session {

	//Cada GameState posee una posible acción. Si no se encuentra la clave en el diccionario significa que aun no se ha explorado
	//Le ponemos como máxima capacidad todos los posibles estados sin discriminar erroneos 3^9 * cada acción posible por estado: 9
	public Dictionary<GameState, float> QDictionaryAgent1 { get; private set; }
	public Dictionary<GameState, float> QDictionaryAgent2 { get; private set; }
	public float LearningRate { get; private set; }
	public float DiscountFactor { get; private set; }
	public double Epsilon { get; private set; }
	public int Steps { get; private set; }
	public int MaxSteps { get; private set; }

	/// <summary>
	/// Use this constructor when you want to start a Leaning Session
	/// </summary>
	/// <param name="learningRate">How much you want to take into account the new learnt play</param>
	/// <param name="discountFactor">How you want the future to affect the current state. Good high if not intermediate rewards</param>
	/// <param name="epsilon">The probability of using a random play</param>
	/// <param name="maxSteps">Max learning steps</param>
	public Session(float learningRate, float discountFactor, double epsilon, int maxSteps)
	{
		QDictionaryAgent1 = new Dictionary<GameState, float>(Mathf.RoundToInt(Mathf.Pow(3,9) * 9), new GameStateComparer());
		QDictionaryAgent2 = new Dictionary<GameState, float>(Mathf.RoundToInt(Mathf.Pow(3,9) * 9), new GameStateComparer());
		LearningRate = Mathf.Clamp01(learningRate);
		DiscountFactor = Mathf.Clamp01(discountFactor);
		Epsilon = epsilon;
		Steps = 0;
		MaxSteps = maxSteps;	
		
		Assert.AreNotEqual(maxSteps,0);
	}

	/// <summary>
	/// Use this constructor when you want to read a session file
	/// </summary>
	public Session(string sessionFileName)
	{
		var dics = SessionIo.LoadSessionFile(sessionFileName);
		QDictionaryAgent1 = dics[0];
		QDictionaryAgent2 = dics[1];
	}

	public int Reward(Cell.CellOwner owner)
	{
		if (!GameManager.I.IsGameEnded())
			return 0;
		
		if (GameManager.I.Winner == Cell.CellOwner.Agent1 && owner == Cell.CellOwner.Agent1 ||
		    GameManager.I.Winner == Cell.CellOwner.Agent2 && owner == Cell.CellOwner.Agent2)
			return 1;
		
		if (GameManager.I.Winner == Cell.CellOwner.Agent1 && owner == Cell.CellOwner.Agent2 ||
		    GameManager.I.Winner == Cell.CellOwner.Agent2 && owner == Cell.CellOwner.Agent1)
			return -1;

		//si empatan 0
		return 0;
	}

	private float CheckBestQValueAtGameState(Cell.CellOwner[] owners, Cell.CellOwner owner)
	{
		float tempMaxQ1 = 0;
		float tempMaxQ2 = 0;
		
		for (int i = 0; i < 9; i++)
		{
			GameState gameState = new GameState(owners,i);

			if (owner == Cell.CellOwner.Agent1)
			{
				if(QDictionaryAgent1.ContainsKey(gameState))
					if (QDictionaryAgent1[gameState] > tempMaxQ1)
						tempMaxQ1 = QDictionaryAgent1[gameState];				
			}
			else
			{
				if(QDictionaryAgent2.ContainsKey(gameState))
					if (QDictionaryAgent2[gameState] > tempMaxQ2)
						tempMaxQ2 = QDictionaryAgent2[gameState];
			}
				
		}

		return owner == Cell.CellOwner.Agent1 ? tempMaxQ1 : tempMaxQ2;
	}

	private bool IsValidAction(GameState gameState)
	{
		return gameState.Cells[gameState.IndexAction] == Cell.CellOwner.None;
	}

	public GameState CheckBestActionAtGameState(Cell.CellOwner[] owners, Cell.CellOwner owner)
	{
		GameState tempBestAction = null;
		float tempMaxQ = int.MinValue;
		List<GameState> similarGameStates = new List<GameState>();

		if (owner == Cell.CellOwner.Agent1)
		{
			for (int i = 0; i < 9; i++)
			{
				GameState gameState = new GameState(owners, i);

				if (IsValidAction(gameState))
				{

					if (QDictionaryAgent1.ContainsKey(gameState))
					{
						if (QDictionaryAgent1[gameState] == 0)
						{
							similarGameStates.Add(gameState);
						}
						else if (QDictionaryAgent1[gameState] > tempMaxQ)
						{
							tempMaxQ = QDictionaryAgent1[gameState];
							tempBestAction = gameState;
						}
					}
					else
					{
						similarGameStates.Add(gameState);
						QDictionaryAgent1[gameState] = 0;
					}
				}

			}
		}
		else
		{
			for (int i = 0; i < 9; i++)
			{
				GameState gameState = new GameState(owners,i);
	
				if (IsValidAction(gameState))
				{
					
					if (QDictionaryAgent2.ContainsKey(gameState))
					{
						if(QDictionaryAgent2[gameState] == 0)
						{
							similarGameStates.Add(gameState);
						}
						else if (QDictionaryAgent2[gameState] > tempMaxQ)
						{
							tempMaxQ = QDictionaryAgent2[gameState];
							tempBestAction = gameState;
						}
					}
					else
					{
						similarGameStates.Add(gameState);
						QDictionaryAgent2[gameState] = 0;
					}
				}
				
			}
		}

		if (tempBestAction == null)
		{
			if(similarGameStates.Count > 0)
				return similarGameStates[Random.Range(0, similarGameStates.Count - 1)];
			
		}

		return tempBestAction;
	}

	public void UpdateHyperParameters()
	{
		//conforme esta esto si iteramos unas 100k el agente 1 va genial.
		//todo entrenar 200k veces haciendo que baje poco el learning rate y el epsilon.
		//todo entrenar 200k veces bajando poco el learning rate y bajando bien el epsilon
		
		if(LearningRate >= 0.3f) 
			LearningRate -= 0.000001f;
		if(Epsilon >= 0.3f) 
			Epsilon -= 0.000001f;
		Steps += 1;

		if (Steps >= MaxSteps && MaxSteps != 0)
			OnSessionCompleted();

	}
	
	public void UpdateQValue(GameState observedPlay, Cell.CellOwner owner)
	{
		
		int reward = GameManager.I.LearningSession.Reward(owner);

		if (owner == Cell.CellOwner.Agent1)
		{
			if (!QDictionaryAgent1.ContainsKey(observedPlay))
			{
				QDictionaryAgent1[observedPlay] = 0;
			}
				
			float newQ = QDictionaryAgent1[observedPlay]
						 + LearningRate * (reward + DiscountFactor * CheckBestQValueAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),owner)  - QDictionaryAgent1[observedPlay]);
	
			QDictionaryAgent1[observedPlay] = newQ;
		}
		else
		{
			if (!QDictionaryAgent2.ContainsKey(observedPlay))
			{
				QDictionaryAgent2[observedPlay] = 0;
			}
				
			float newQ = QDictionaryAgent2[observedPlay]
			             + LearningRate * (reward + DiscountFactor * CheckBestQValueAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells),owner)  - QDictionaryAgent2[observedPlay]);
	
			QDictionaryAgent2[observedPlay] = newQ;
		}
	}

	public void OnSessionCompleted()
	{
		SessionIo.SaveSessionFile("session_" + MaxSteps,QDictionaryAgent1,QDictionaryAgent2);
		Application.Quit();
		
		#if UNITY_EDITOR
			Debug.Break();
		#endif
	}
}
