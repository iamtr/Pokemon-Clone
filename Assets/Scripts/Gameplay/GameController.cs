using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState { FreeRoam, Battle, Dialog, Paused }
public class GameController : MonoBehaviour
{
	public static GameController Instance { get; private set; } 
	
	[SerializeField] private GameState state;
	[SerializeField] private GameState stateBeforePause;

	[SerializeField] private PlayerController playerController;
	[SerializeField] private BattleSystem battleSystem;
	[SerializeField] private Camera worldCamera;

	

	public SceneDetails CurrentScene { get; private set; }
	public SceneDetails PreviousScene { get; private set; }

	private void Awake()
	{
		ConditionsDB.Init();
		Instance = this;
		
	}
	private void Start()
	{
		playerController.OnEncountered += StartBattle;
		battleSystem.OnBattleOver += EndBattle;

		DialogManager.Instance.OnShowDialog += () => state = GameState.Dialog;
		DialogManager.Instance.OnCloseDialog += () =>
		{
			if (state == GameState.Dialog)
				state = GameState.FreeRoam;
		};


	}
	private void Update()
	{
		if (state == GameState.FreeRoam)
		{ 
			playerController.HandleUpdate();
		}
		else if (state == GameState.Battle)
		{
			battleSystem.HandleUpdate();
		}
		else if (state == GameState.Dialog)
		{
			DialogManager.Instance.HandleUpdate();
		}
	}

	public void StartBattle() 
	{ 
		state = GameState.Battle;
		battleSystem.gameObject.SetActive(true);
		worldCamera.gameObject.SetActive(false);

		var playerParty = playerController.GetComponent<PokemonParty>();
		var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();
		battleSystem.StartBattle(playerParty, wildPokemon);
	}

	private void EndBattle(bool won)
	{
		state = GameState.FreeRoam;
		battleSystem.gameObject.SetActive(false);
		worldCamera?.gameObject.SetActive(true);
	}

	public void SetCurrentScene(SceneDetails currScene)
	{
		PreviousScene = CurrentScene;
		CurrentScene = currScene;
	}

	public void PauseGame(bool pause)
	{
		Debug.Log("Pause");
		if (pause)
		{
			stateBeforePause = state;
			state = GameState.Paused;
		}
		else
		{
			state = stateBeforePause;
		}
	}
}

