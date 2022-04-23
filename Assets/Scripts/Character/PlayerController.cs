using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public event Action OnEncountered;

	private bool isMoving;
	private Vector2 input;
	

	private Character character;

	public Character Character => character;
	private void Awake()
	{
		character = GetComponent<Character>();
		Character.SetPositionAndSnapToTile(transform.position);
			
	}


	public void HandleUpdate()
	{
		if (!character.IsMoving)
		{
			input.x = Input.GetAxisRaw("Horizontal");
			input.y = Input.GetAxisRaw("Vertical");
			if (!(input.x == 0)) 
				input.y = 0;

			if (input != Vector2.zero)
			{
				StartCoroutine(character.Move(input, OnMoveOver));
			}
		}

		character.HandleUpdate();

		if (Input.GetKeyDown(KeyCode.Z))
		{
			Interact();
		}
	}

	private void Interact()
	{
		var facingDir = new Vector3(character.Animator.moveX, character.Animator.moveY);
		var interactPos = transform.position + facingDir;

		//Debug.DrawLine(transform.position, interactPos, Color.yellow, .5f);

		var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

		if (collider != null)
		{
			collider.GetComponent<IInteractable>()?.Interact();
		}
	}

	private void OnMoveOver()
	{
		var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.InteractablesLayer);

		foreach (var collider in colliders)
		{
			var triggerable = collider.GetComponent<IPlayerTriggerable>();
			if (triggerable != null)
			{
				triggerable.OnPlayerTriggered(this);
				break;
			}
		}
	}

	
}
