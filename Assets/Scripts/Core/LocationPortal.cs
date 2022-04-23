using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
	[SerializeField] private DestinationIdentifier destinationPortal;
	[SerializeField] private Transform spawnPoint;

	private PlayerController player;
	private Fader fader;

	public Transform SpwanPoint => spawnPoint;

	private void Start()
	{
		fader = FindObjectOfType<Fader>();
	}

	public void OnPlayerTriggered(PlayerController player)
	{
		//Debug.Log("Player entered the portal");
		this.player = player;
		
		player.Character.Animator.IsMoving = false;
		StartCoroutine(SwitchScene());
		
	}

	private IEnumerator SwitchScene()
	{
		GameController.Instance.PauseGame(true);
		yield return fader.FadeIn(0.5f);
	
		var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == destinationPortal);
		player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

		yield return fader.FadeOut(0.5f);
		GameController.Instance.PauseGame(false);

	}
}
