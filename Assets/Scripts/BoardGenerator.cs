using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{

	public Cell CellPrefab;
	public Cell[] Cells;
	public Vector2 BoardSize;
	public float CellRadius = 1f;
	
	[Range(0.01f,0.99f)]
	public float ScaleFactor = 0.91f;
	
	// Use this for initialization
	void Awake()
	{
		Cells = new Cell[9];
		//GameManager.I.Cells = Cells;
	}

	[ContextMenu("Generate Board")]
	public void GenerateBoard()
	{
		Vector3 topLeft = transform.position - new Vector3(Mathf.RoundToInt(BoardSize.x / 2), 0, Mathf.RoundToInt( - BoardSize.y / 2));
		Debug.DrawRay(transform.position, topLeft, Color.green,20.0f);

		int cellCounter = 0;
		for (int i = 0; i < BoardSize.y; i++)
		{
			for (int j = 0; j < BoardSize.x; j++)
			{
				Vector3 worldPos = topLeft + Vector3.right * (CellRadius) +
				                   Vector3.back * CellRadius + //ajustamos el primero
				                   Vector3.right * (CellRadius) * j + //rellenamos hacia la derecha
				                   Vector3.back *  (CellRadius) * i; //rellenamos hacia delante, z es hacia delante
				
				Cell cell = Instantiate(CellPrefab, worldPos, Quaternion.identity);
				cell.transform.localScale *= ScaleFactor;
				cell.I = i;
				cell.J = j;
				Cells[cellCounter] = cell;
				cellCounter++;
			}
		}
		
		GameManager.I.Cells = Cells;
	}
}
