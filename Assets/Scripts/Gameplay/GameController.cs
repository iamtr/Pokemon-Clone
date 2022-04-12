using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum GameState { FreeRoam, Battle, Dialog }
public class GameController : MonoBehaviour
{
    [SerializeField] private GameState state;

	[SerializeField] private PlayerController playerController;
	[SerializeField] private BattleSystem battleSystem;
	[SerializeField] private Camera worldCamera;

	private void Awake()
	{
		ConditionsDB.Init();
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

	private void StartBattle() 
	{ 
		state = GameState.Battle;
		battleSystem.gameObject.SetActive(true);
		worldCamera.gameObject.SetActive(false);

		var playerParty = playerController.GetComponent<PokemonParty>();
		var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
		battleSystem.StartBattle(playerParty, wildPokemon);
	}

	private void EndBattle(bool won)
	{
		state = GameState.FreeRoam;
		battleSystem.gameObject.SetActive(false);
		worldCamera?.gameObject.SetActive(true);
	}


}

