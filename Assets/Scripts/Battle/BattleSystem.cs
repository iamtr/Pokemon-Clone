using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
	public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }

	[SerializeField] private BattleUnit playerUnit;
	[SerializeField] private BattleUnit enemyUnit;
	[SerializeField] private BattleDialogBox dialogBox;
	[SerializeField] private PartyScreen partyScreen;

	[SerializeField] private BattleState state;

	[SerializeField] private int currentMove;
	[SerializeField] private int currentAction;
	[SerializeField] private int currentMember;

	public event Action<bool> OnBattleOver; // if enemy fainted, true; if player fainted, false;

	private PokemonParty playerParty;
	private Pokemon wildPokemon;

	public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
	{
		this.playerParty = playerParty;
		this.wildPokemon = wildPokemon;
		StartCoroutine(SetupBattle());
	}
	public void HandleUpdate()
	{
		if (state == BattleState.ActionSelection)
		{
			HandleActionSelection();
		}

		else if (state == BattleState.MoveSelection)
		{
			HandleMoveSelection();
		}

		else if (state == BattleState.PartyScreen)
		{
			HandlePartyScreenSelection();
		}
	}
	private IEnumerator SetupBattle()
	{
		playerUnit.Setup(playerParty.GetHealthyPokemon());	
		enemyUnit.Setup(wildPokemon);
		dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
		partyScreen.Init();
		 
		yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!"); //why yield return?
		yield return new WaitForSeconds(1f);

		ChooseFirstTurn();
	}
	private void ChooseFirstTurn()
	{
		if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
		{
			ActionSelection();
		}
		else
			StartCoroutine(EnemyMove());
	}
	private void ActionSelection()
	{
		state = BattleState.ActionSelection;
		dialogBox.SetDialog("Choose an action");
		dialogBox.EnableActionSelector(true);
	}
	private void MoveSelection()
	{
		state = BattleState.MoveSelection;
		dialogBox.EnableActionSelector(false);
		dialogBox.EnableDialogText(false);
		dialogBox.EnableMoveSelector(true);

	}
	private void HandleActionSelection()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) currentAction -= 2;
		else if (Input.GetKeyDown(KeyCode.DownArrow)) currentAction += 2;
		else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentAction;
		else if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentAction;

		currentAction = Mathf.Clamp(currentAction, 0, 3);

		dialogBox.UpdateActionSelection(currentAction);
		if (Input.GetKeyDown(KeyCode.Z)) 
		{
			switch (currentAction)
			{
				case (0):
					//fight 
					MoveSelection();
					break;
				case (1):
					//Bag
					break;
				case (2):
					//Pokemon
					OpenPartyScreen();
					break;
				case (3):
					//Run
					break;
					
				default:
					break;
			}
		}
	}
	private void HandleMoveSelection()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) currentMove -= 2;
		else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMove += 2;
		else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMove;
		else if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMove;

		currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);


		dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			StartCoroutine(PlayerMove());
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			ActionSelection();
		}
	}
	private void HandlePartyScreenSelection()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) currentMember -= 2;
		else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMember += 2;
		else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMember;
		else if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMember;

		currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

		partyScreen.UpdateMemberSelection(currentMember);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			var selectedMember = playerParty.Pokemons[currentMember];	

			if(selectedMember.HP <= 0)
			{
				partyScreen.SetMessageText("You can't send out a fainted pokemon");
				return;
			}
			if (selectedMember == playerUnit.Pokemon)
			{
				partyScreen.SetMessageText("Pokemon is already active in battle");
				return;
			}

			partyScreen.gameObject.SetActive(false);
			state = BattleState.Busy;
			StartCoroutine(SwitchPokemon(selectedMember));
		}	
		else if (Input.GetKeyDown(KeyCode.X))
		{
			partyScreen.gameObject.SetActive(false);
			ActionSelection();
		}
	}
	private IEnumerator SwitchPokemon(Pokemon newPokemon)
	{
		bool currentPokemonFainted = true;
		dialogBox.EnableActionSelector(false);
		if (playerUnit.Pokemon.HP > 0)
		{
			currentPokemonFainted = false;
			yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
		}

		playerUnit.Setup(newPokemon);
		dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
		yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");
		if (currentPokemonFainted)
			ChooseFirstTurn();
		else
			StartCoroutine(EnemyMove());
	}
	private void OpenPartyScreen()
	{
		state = BattleState.PartyScreen;
		partyScreen.SetPartyData(playerParty.Pokemons);
		partyScreen.gameObject.SetActive(true);
	}
	private IEnumerator PlayerMove()
	{
		state = BattleState.PerformMove;
		var move = playerUnit.Pokemon.Moves[currentMove];
		yield return RunMove(playerUnit, enemyUnit, move);
		if (state == BattleState.PerformMove)
			StartCoroutine(EnemyMove()) ;
	}
	private IEnumerator EnemyMove()
	{
		state = BattleState.PerformMove;
		var move = enemyUnit.Pokemon.GetRandomMove();

		yield return RunMove(enemyUnit, playerUnit, move);
		if (state == BattleState.PerformMove)
			ActionSelection();
	}
	private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
	{
		move.PP--;
		yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

		sourceUnit.PlayAttackAnimation();
		yield return new WaitForSeconds(1f);
		targetUnit.PlayHitAnimation();

		// Check if it is a STATUS move
		if (move.Base.Category == MoveCategory.Status)
		{
			yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
		}
		else 
		{
			var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
			yield return targetUnit.Hud.UpdateHP();
			yield return ShowDamageDetails(damageDetails);
		}

		if (targetUnit.Pokemon.HP <= 0)
		{
			yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted");
			targetUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
			CheckForBattleOver(targetUnit);
		}

		// Statuses like burn or poison will hurt the pokemon after the turn
		sourceUnit.Pokemon.OnAfterTurn();
		yield return ShowStatusChanges(sourceUnit.Pokemon);
		yield return sourceUnit.Hud.UpdateHP();
		if (sourceUnit.Pokemon.HP <= 0)
		{
			yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted");
			sourceUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
			CheckForBattleOver(sourceUnit);
		}
	}
	private IEnumerator ShowStatusChanges(Pokemon pokemon)
	{
		while (pokemon.StatusChanges.Count > 0)
		{
			var message = pokemon.StatusChanges.Dequeue();
			yield return dialogBox.TypeDialog(message);

		}
	}
	private IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
	{
		var effects = move.Base.Effects;

		//Stat Boosting
		if (effects.Boosts != null)
		{

			if (move.Base.Target == MoveTarget.Self)
			{
				source.ApplyBoosts(effects.Boosts);
			}
			else
			{
				target.ApplyBoosts(effects.Boosts);
			}

		}
		//Status Condition
		if (effects.Status != ConditionID.none)
		{
			target.SetStatus(effects.Status);
		}

		yield return ShowStatusChanges(target);
		yield return ShowStatusChanges(source);
	}
	private void CheckForBattleOver(BattleUnit faintedUnit)
	{
		if (faintedUnit.IsPlayerUnit)
		{
			var nextPokemon = playerParty.GetHealthyPokemon();
			if (nextPokemon != null)
			{
				OpenPartyScreen();
			}
			else BattleOver(false);
		}
		else BattleOver(true);
	}
	private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
	{
		if (damageDetails.Critical > 1f)
			yield return dialogBox.TypeDialog("A critical hit!");
		if (damageDetails.Type > 1f)
			yield return dialogBox.TypeDialog("It's super effective!");
		else if (damageDetails.Type < 1f)
			yield return dialogBox.TypeDialog("It's not very effective...");

		
	}
	private void BattleOver(bool won)
	{
		state = BattleState.BattleOver;
		playerParty.Pokemons.ForEach(p => p.OnBattleOver());
		OnBattleOver(won);
	}
}
