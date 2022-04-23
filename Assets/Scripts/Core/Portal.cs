using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
	[SerializeField] private int sceneToLoad = -1;
	[SerializeField] private DestinationIdentifier destinationPortal;
	[SerializeField] private Transform spawnPoint;

	private PlayerController player;
	private Fader fader;

	private void Start()
	{
		fader = GetComponent<Fader>();
	}

	public void OnPlayerTriggered(PlayerController player)
	{
		//Debug.Log("Player entered the portal");
		this.player = player;
		StartCoroutine(SwitchScene());
	}

	private IEnumerator SwitchScene()
	{
		DontDestroyOnLoad(gameObject);
		//DontDestroyOnLoad(gameObject);
		yield return fader.FadeIn(0.5f);
		GameController.Instance.PauseGame(true);

		yield return SceneManager.LoadSceneAsync(sceneToLoad);
		var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
		player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

		GameController.Instance.PauseGame(false);
		yield return fader.FadeOut(0.5f);

		Destroy(gameObject);
	}
}

public enum DestinationIdentifier { A, B, C, D, E}