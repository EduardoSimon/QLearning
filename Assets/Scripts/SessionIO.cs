using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class SessionIo
{
    public static void SaveSessionFile(string sessionName, Dictionary<GameState,float> qDictionary1, Dictionary<GameState,float> qDictionary2)
    {
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/" + sessionName + ".txt"))
        {
            foreach (var state in qDictionary1.Keys)
            {
                file.WriteLine(state + ":" + qDictionary1[state]);
            }
            
            file.WriteLine();

            foreach (var state in qDictionary2.Keys)
            {
                file.WriteLine(state + ":" + qDictionary2[state]);
            }
        }
    }
	
    public static Dictionary<GameState,float>[] LoadSessionFile(string sessionName)
    {
        Dictionary<GameState,float> qDictionary1 = new Dictionary<GameState, float>(new GameStateComparer());
        Dictionary<GameState,float> qDictionary2 = new Dictionary<GameState, float>(new GameStateComparer());
		
        using (StreamReader file = new StreamReader(Application.dataPath + "/" + sessionName + ".txt"))
        {
            //the string looks like this 0,0,0,0,0,0,0,0,0,:0:0
            string s = file.ReadLine();
            while (s != "")
            {
                String[] values = s.Split(':');
                String[] cells = values[0].Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
				
                GameState state = new GameState(new []
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
				
                qDictionary1[state] = Q;
                s = file.ReadLine();
            }
            
            //hemos leido el primer diccionario ahora el segundo

            s = file.ReadLine();
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
				
                qDictionary2[state] = Q;
                s = file.ReadLine();
            }
        }
        
        Assert.IsNotNull(qDictionary1);
        Assert.IsNotNull(qDictionary2);

        return new[]{ qDictionary1, qDictionary2};
        
    }


}
