using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialog", menuName = "Dialog/Create new dialog")]
public class Dialog : ScriptableObject
{
	[SerializeField] private List<string> lines;

	public List<string> Lines => lines;


}

