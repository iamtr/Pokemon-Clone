using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Walking }
public class NPCController : MonoBehaviour, IInteractable
{
	[SerializeField] Dialog dialog;
	[SerializeField] List<Vector2> movementPattern;
	[SerializeField] float timeBetweenPattern;

	private Character character;
	private NPCState state;
	float idleTimer = 0f;
	int currentPattern = 0;

	private void Awake()
	{
		character = GetComponent<Character>();
	}

	//private void Start()
	//{
	//	spriteAnimator = new SpriteAnimator(sprites, GetComponent<SpriteRenderer>());
	//	spriteAnimator.Start();
	//}

	private void Update()
	{
		if (DialogManager.Instance.IsShowing) return;

		if (state == NPCState.Idle)
		{
			idleTimer += Time.deltaTime;
			if (idleTimer > timeBetweenPattern)
			{
				idleTimer = 0f;
				if (movementPattern.Count > 0)
				{
					StartCoroutine(Walk());
				}
			}
		}

		character.HandleUpdate();
	}

	public void Interact()
	{	
		if (state == NPCState.Idle)
			StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
		
	}

	private IEnumerator Walk()
	{
		state = NPCState.Walking;

		yield return character.Move(movementPattern[currentPattern]);
		currentPattern = (currentPattern + 1) % movementPattern.Count;

		state = NPCState.Idle;
	}

	
}
