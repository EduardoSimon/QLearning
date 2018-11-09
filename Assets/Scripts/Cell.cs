using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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

    private WinnerData CheckRowCombination()
    {
        Cell[] cells = GameManager.I.Cells;

        if (this.owner != CellOwner.None)
        {
            if (J == 0)
            {
                if (cells[I * 3 + (J + 1)].owner == this.owner && cells[I * 3 + (J + 2)].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if (J == 1)
            {
                if (cells[I * 3 + (J - 1)].owner == this.owner && cells[I * 3 + (J + 1)].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else
            {
                if (cells[I * 3 + (J - 2)].owner == this.owner && cells[I * 3 + (J - 1)].owner == this.owner)
                    return new WinnerData(owner,true);
            }
        }

        return new WinnerData(owner,false);
    }

    private WinnerData CheckColumnCombination()
    {
        Cell[] cells = GameManager.I.Cells;

        if (owner != CellOwner.None)
        {
            if (I == 0)
            {
                if (cells[(I + 1) * 3 + J].owner == this.owner && cells[(I + 2) * 3 + J].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if (I == 1)
            {
                if (cells[(I - 1) * 3 + J].owner == this.owner && cells[(I + 1) * 3 + J].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else
            {
                if (cells[(I - 1) * 3 + J].owner == this.owner && cells[(I - 2) * 3 + J].owner == this.owner)
                    return new WinnerData(owner,true);
            }
        }

        return new WinnerData(owner,false);
    }
    
    private WinnerData CheckMainDiagonalCombination()
    {
        Cell[] cells = GameManager.I.Cells;

        if (owner != CellOwner.None)
        {
            if (I == 0 && J == 0)
            {
                if (cells[4].owner == this.owner && cells[8].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if (I == 2 && J == 2)
            {
                if (cells[4].owner == this.owner && cells[0].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if(I == 1 && J ==1)
            {
                if (cells[0].owner == this.owner && cells[8].owner == this.owner)
                    return new WinnerData(owner,true);
            }
        }

        return new WinnerData(owner,false);
    }
    
    private WinnerData CheckSecondaryDiagonalCombination()
    {
        Cell[] cells = GameManager.I.Cells;

        if (owner != CellOwner.None)
        {
            if (I == 0 && J == 2)
            {
                if (cells[4].owner == this.owner && cells[6].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if (I == 2 && J == 0)
            {
                if (cells[4].owner == this.owner && cells[2].owner == this.owner)
                    return new WinnerData(owner,true);
            }
            else if(I == 1 && J ==1)
            {
                if (cells[2].owner == this.owner && cells[6].owner == this.owner)
                    return new WinnerData(owner,true);
            }
        }

        return new WinnerData(owner,false);
    }

    public WinnerData IsAWinnerCombination()
    {
        WinnerData rowData = CheckRowCombination();
        WinnerData colData = CheckColumnCombination();
        WinnerData mainDiagonalData = CheckMainDiagonalCombination();
        WinnerData secondaryDiagonalData = CheckSecondaryDiagonalCombination();
        

        if (rowData.IsWinner)
            return rowData;
        if(colData.IsWinner)
            return colData;
        if (mainDiagonalData.IsWinner)
            return mainDiagonalData;
        if (secondaryDiagonalData.IsWinner)
            return secondaryDiagonalData;

        return new WinnerData(CellOwner.None,false);// si no hay combinación ganadora devolvemos la que sea
    }

    public struct WinnerData
    {
        public CellOwner Owner;
        public bool IsWinner;

        public WinnerData(CellOwner owner, bool isWinner)
        {
            Owner = owner;
            IsWinner = isWinner;
        }
    }
}
