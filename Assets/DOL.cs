using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOL : MonoBehaviour {
	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
}
