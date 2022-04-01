using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
	//private fields automatically instantiated by vs (auto implemented property)
	public MoveBase Base { get;  set; } 
	public int PP { get; set; }
	public Move(MoveBase pBase)
	{
		Base = pBase;
		PP = pBase.PP;
	}
}
