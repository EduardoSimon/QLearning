using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public bool HasPlacedAToken;
	private void OnEnable()
	{
		Cell.OnCellClicked += ProcessPlayerInput;
	}

	private void OnDisable()
	{
		Cell.OnCellClicked -= ProcessPlayerInput;
	}

	public void ProcessPlayerInput(Cell cell)
	{
		Debug.Log("El player ha seleccionado la pieza: " + cell.I + " " + cell.J);
		cell.owner = Cell.CellOwner.Player;
		HasPlacedAToken = true;
		cell.UpdateColor();
	}
}
