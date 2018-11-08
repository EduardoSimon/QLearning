using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellOwner
    {
        Player,
        Agent1,
        Agent2,
        None
    }
    public int I;
    public int J;
    public CellOwner owner;
    public static event Action<Cell> OnCellClicked;

    private void Start()
    {
        owner = CellOwner.None;
    }

    public void OnMouseDown()
    {
        if (GameManager.I.GameState == GameManager.Gamestate.PlayerTurn)
        {
            if (owner == CellOwner.None)
            {
                if (OnCellClicked != null)
                    OnCellClicked(this);
            }
        }
    }

    public void UpdateColor()
    {
        if (owner == CellOwner.Player)
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else if (owner == CellOwner.Agent1)
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        else if (owner == CellOwner.Agent2)
        {
            GetComponent<MeshRenderer>().material.color = Color.green;
        }       
    }
}
