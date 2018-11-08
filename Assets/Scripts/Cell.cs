using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int i;
    public int j;
    public bool hasTokenOnTop;
    
    public void OnMouseDown()
    {
        if (!hasTokenOnTop)
        {
            Debug.Log("He entrado el item: " + i + ", " + j + ". ");
            hasTokenOnTop = true;
        }
    }
}
