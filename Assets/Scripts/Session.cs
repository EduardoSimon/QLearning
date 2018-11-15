using System;
using System.Collections;
using System.Collections.Generic;
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
	}
	
	//Cada GameState posee una posible acción. Si no se encuentra la clave en el diccionario significa que aun no se ha explorado
	//Le ponemos como máxima capacidad todos los posibles estados sin discriminar erroneos 3^9 * cada acción posible por estado: 9
	public Dictionary<GameState, float> QDictionary { get; private set; }
	public float LearningRate { get; private set; }
	public float DiscountFactor { get; private set; }
	public static double Epsilon { get; private set; }

	public Session(float learningRate, float discountFactor, double epsilon)
	{
		QDictionary = new Dictionary<GameState, float>(Mathf.RoundToInt(Mathf.Pow(3,9) * 9));
		LearningRate = Mathf.Clamp01(learningRate);
		DiscountFactor = Mathf.Clamp01(discountFactor);
		Epsilon = epsilon;
	}
	
	//TODO guardarlo en un archivo
	//TODO cargarlo de un archivo

	public int Reward(Cell.CellOwner agent)
	{
		if (!GameManager.I.IsGameEnded())
			return 0;

		if (GameManager.I.Winner == agent)
			return 1;
		
		if (GameManager.I.Winner == Cell.CellOwner.Agent2)
			return -1;

		return 0;
	}

	public float CheckBestQValueAtGameState(Cell.CellOwner[] owners)
	{
		//todo solo cogiendo las posibles
		return 0;
	}

	public GameState CheckBestActionAtGameState(Cell.CellOwner[] owners)
	{
		//todo cogiendo las posibles
		return null;
	}

	public void UpdateHyperParamters()
	{
		//todo hacer decrease de learning rate, discount factor y epsilon
		//lo de epsilon esta en el correo que me ha mandado
	}
}
