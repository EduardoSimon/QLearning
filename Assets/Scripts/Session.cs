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
	//todo crear dos diccionarios
	public Dictionary<GameState, float> QDictionary { get; private set; }
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
		QDictionary = new Dictionary<GameState, float>(Mathf.RoundToInt(Mathf.Pow(3,9) * 9), new GameStateComparer());
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
		QDictionary = SessionIo.LoadSessionFile(sessionFileName);  
	}

	public int Reward(Cell.CellOwner owner)
	{		
		if (GameManager.I.Winner == Cell.CellOwner.Agent1 && owner == Cell.CellOwner.Agent1 ||
		    GameManager.I.Winner == Cell.CellOwner.Agent2 && owner == Cell.CellOwner.Agent2)
			return 1;
		
		if (GameManager.I.Winner == Cell.CellOwner.Agent1 && owner == Cell.CellOwner.Agent2 ||
		    GameManager.I.Winner == Cell.CellOwner.Agent2 && owner == Cell.CellOwner.Agent1)
			return -1;

		//si empatan 0
		return 0;
	}

	private float CheckBestQValueAtGameState(Cell.CellOwner[] owners)
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
		return gameState.Cells[gameState.IndexAction] == Cell.CellOwner.None;
	}

	public GameState CheckBestActionAtGameState(Cell.CellOwner[] owners)
	{
		GameState tempBestAction = null;
		float tempMaxQ = int.MinValue;
		List<GameState> similarGameStates = new List<GameState>();
		
		for (int i = 0; i < 9; i++)
		{
			GameState gameState = new GameState(owners,i);

			if (IsValidAction(gameState))
			{
				if (QDictionary.ContainsKey(gameState))
				{
					if(QDictionary[gameState] == 0)
					{
						similarGameStates.Add(gameState);
					}
					else if (QDictionary[gameState] > tempMaxQ)
					{
						tempMaxQ = QDictionary[gameState];
						tempBestAction = gameState;
					}
				}
				else
				{
					similarGameStates.Add(gameState);
					QDictionary[gameState] = 0;
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

	public void UpdateHyperParamters()
	{
		//todo hacer decrease de learning rate, epsilon de manera mas eficiente. Y hacerlo visualizar
		/*if(LearningRate >= 0.01f) 
			LearningRate -= 0.00001f;
		if(Epsilon >= 0.1f) 
			Epsilon -= 0.0001f;*/
		Steps += 1;

		if (Steps >= MaxSteps && MaxSteps != 0)
			OnSessionCompleted();

	}
	
	public void UpdateQValue(GameState observedPlay, Cell.CellOwner owner)
	{
		
		int reward = GameManager.I.LearningSession.Reward(owner);


		if (!QDictionary.ContainsKey(observedPlay))
		{
			QDictionary[observedPlay] = 0;
		}
			
		float newQ = QDictionary[observedPlay]
		             + LearningRate * (reward + DiscountFactor * CheckBestQValueAtGameState(GameManager.I.GetCellsOwner(GameManager.I.Cells))  - QDictionary[observedPlay]);

		QDictionary[observedPlay] = newQ;
	}

	private void OnSessionCompleted()
	{
		SessionIo.SaveSessionFile("session_" + MaxSteps,QDictionary);
		Application.Quit();
		
		#if UNITY_EDITOR
			Debug.Break();
		#endif
	}
}
