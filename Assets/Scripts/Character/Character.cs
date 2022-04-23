using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Character : MonoBehaviour
{
	[SerializeField] private float moveSpeed;

	public bool IsMoving { get; private set; }
	public float OffsetY { get; private set; } = 0.2f;

	private CharacterAnimator animator;
	public CharacterAnimator Animator => animator;

	private void Awake()
	{
		animator = GetComponent<CharacterAnimator>();
	}

	public void SetPositionAndSnapToTile(Vector2 pos)
	{
		pos.x = Mathf.FloorToInt(pos.x) + 0.5f;
		pos.y = Mathf.FloorToInt(pos.y) + 0.5f + OffsetY;

		transform.position = pos;
	}

	public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null)
	{
		animator.moveX = Mathf.Clamp(moveVector.x, -1, 1);
		animator.moveY = Mathf.Clamp(moveVector.y, -1, 1);
		var targetPos = transform.position;
		targetPos.x += moveVector.x;
		targetPos.y += moveVector.y;

		if (!IsWalkable(targetPos))
			yield break;

		IsMoving = true;
		
		while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
			yield return null;
		}
		transform.position = targetPos;
		IsMoving = false;

		OnMoveOver?.Invoke();
	}

	public void HandleUpdate()
	{
		animator.IsMoving = IsMoving;
	}

	private bool IsWalkable(Vector3 targetPos)
	{
		if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidObjectsLayer | GameLayers.i.InteractableLayer) != null)
			return false;
		else return true;
	}

}
