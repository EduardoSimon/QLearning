using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class SessionIo
{
    public static void SaveSessionFile(string sessionName, Dictionary<GameState,float> qDictionary)
    {
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/" + sessionName + ".txt"))
        {
            foreach (var state in qDictionary.Keys)
            {
                file.WriteLine(state + ":" + qDictionary[state]);
            }
        }
    }
	
    public static Dictionary<GameState,float> LoadSessionFile(string sessionName)
    {
        Dictionary<GameState,float> qDictionary = new Dictionary<GameState, float>(new GameStateComparer());
		
        using (StreamReader file = new StreamReader(Application.dataPath + "/" + sessionName + ".txt"))
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
        
        Assert.IsNotNull(qDictionary);
        return qDictionary;
        
    }


}
