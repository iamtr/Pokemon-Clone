using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] public float moveSpeed;
	public LayerMask solidObjectsLayer;
	public LayerMask grassLayer;
	public LayerMask interactableLayer;

	public event Action OnEncountered;

	private bool isMoving;
	private Vector2 input;
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}


	public void HandleUpdate()
	{
		if (!isMoving)
		{
			input.x = Input.GetAxisRaw("Horizontal");
			input.y = Input.GetAxisRaw("Vertical");
			if (!(input.x == 0)) input.y = 0;

			if (input != Vector2.zero)
			{
				animator.SetFloat("moveX", input.x);
				animator.SetFloat("moveY", input.y);
				var targetPos = transform.position;
				targetPos.x += input.x;
				targetPos.y += input.y;

				if (IsWalkable(targetPos)) StartCoroutine(Move(targetPos));
			}
		}

		animator.SetBool("isMoving", isMoving);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			Interact();
		}
	}
	IEnumerator Move(Vector3 targetPos)
	{
		isMoving = true;
		while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
			yield return null;
		}
		transform.position = targetPos;
		isMoving = false;

		CheckForEncounters();
	}

	private void CheckForEncounters()
	{
		
		if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
		{
			if (UnityEngine.Random.Range(1, 101) <= 10) 
			{
				animator.SetBool("isMoving", false);
				OnEncountered();
			}
			 
		}
	}

	private bool IsWalkable(Vector3 targetPos)
	{
		if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null) 
			return false;
		else return true;
	}

	private void Interact()
	{
		var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
		var interactPos = transform.position + facingDir;

		Debug.DrawLine(transform.position, interactPos, Color.yellow, .5f);

		var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
		if (collider != null)
		{
			collider.GetComponent<IInteractable>()?.Interact();
		}


	}
}