using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
	public void OnPlayerTriggered(PlayerController player)
	{
		if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
		{
			if (UnityEngine.Random.Range(1, 101) <= 10)
			{
				player.Character.Animator.IsMoving = false;
				GameController.Instance.StartBattle();
			}
		}
	}

	
}
