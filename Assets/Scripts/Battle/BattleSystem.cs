using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
	public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver }
	public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

	[SerializeField] private BattleUnit playerUnit;
	[SerializeField] private BattleUnit enemyUnit;
	[SerializeField] private BattleDialogBox dialogBox;
	[SerializeField] private PartyScreen partyScreen;

	[SerializeField] private BattleState? state; 
	[SerializeField] private BattleState? prevState;

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

		ActionSelection();
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
					prevState = state;
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
			var move = playerUnit.Pokemon.Moves[currentMove];
			if (move.PP == 0) 
				return;
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			StartCoroutine(RunTurns(BattleAction.Move));
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

			if (prevState == BattleState.ActionSelection)
			{
				prevState = null;
				StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
			}

			else
			{
				state = BattleState.Busy;
				StartCoroutine(SwitchPokemon(selectedMember));
			}
			
		}	
		else if (Input.GetKeyDown(KeyCode.X))
		{
			partyScreen.gameObject.SetActive(false);
			ActionSelection();
		}
	}
	private IEnumerator SwitchPokemon(Pokemon newPokemon)
	{
		dialogBox.EnableActionSelector(false);
		if (playerUnit.Pokemon.HP > 0)
		{
			yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
		}

		playerUnit.Setup(newPokemon);
		dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
		yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

		state = BattleState.RunningTurn;

	}
	private void OpenPartyScreen()
	{
		state = BattleState.PartyScreen;
		partyScreen.SetPartyData(playerParty.Pokemons);
		partyScreen.gameObject.SetActive(true);
	}

	private IEnumerator RunTurns (BattleAction playerAction)
	{
		if (playerAction == BattleAction.Move)
		{
			state = BattleState.RunningTurn;

			playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
			enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

			int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
			int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
			bool playerGoesFirst = true;

			if (enemyMovePriority > playerMovePriority)
				playerGoesFirst = false;
			else if (enemyMovePriority == playerMovePriority)
				playerGoesFirst = (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed);

			//Check who goes first

			var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
			var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

			var secondPokemon = secondUnit.Pokemon;

			//First Move
			yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
			yield return RunAfterTurn(firstUnit);
			if (state == BattleState.BattleOver)
				yield break;

			if (secondPokemon.HP > 0)
			{
				yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
				yield return RunAfterTurn(secondUnit);
				if (state == BattleState.BattleOver)
					yield break;
			}
		}
		else
		{
			if (playerAction == BattleAction.SwitchPokemon)
			{
				var selectedPokemon = playerParty.Pokemons[currentMember];
				state = BattleState.Busy;
				yield return SwitchPokemon(selectedPokemon);
			}

			//Enemy Turn
			var enemyMove = enemyUnit.Pokemon.GetRandomMove();
			yield return RunMove(enemyUnit, playerUnit, enemyMove);
			yield return RunAfterTurn(enemyUnit);
			if (state == BattleState.BattleOver)
				yield break;

		}

		if (state != BattleState.BattleOver)
			ActionSelection();
		

	}
	//private IEnumerator PlayerMove()
	//{
	//	state = BattleState.RunningTurn;
	//	var move = playerUnit.Pokemon.Moves[currentMove];
	//	yield return RunMove(playerUnit, enemyUnit, move);
	//	if (state == BattleState.RunningTurn)
	//		StartCoroutine(EnemyMove()) ;
	//}

	//private IEnumerator EnemyMove()
	//{
	//	state = BattleState.RunningTurn;
	//	var move = enemyUnit.Pokemon.GetRandomMove();

	//	yield return RunMove(enemyUnit, playerUnit, move);
	//	if (state == BattleState.RunningTurn)
	//		ActionSelection();
	//}

	private IEnumerator ShowStatusChanges(Pokemon pokemon)
	{
		while (pokemon.StatusChanges.Count > 0)
		{
			var message = pokemon.StatusChanges.Dequeue();
			yield return dialogBox.TypeDialog(message);

		}
	}

	private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
	{
		bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
		if (!canRunMove)
		{
			yield return ShowStatusChanges(sourceUnit.Pokemon);
			yield return sourceUnit.Hud.UpdateHP();
			yield break;
			//doesnt execute code below
		}
		yield return ShowStatusChanges(sourceUnit.Pokemon);

		move.PP--;
		yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");


		if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
		{
			sourceUnit.PlayAttackAnimation();
			yield return new WaitForSeconds(1f);
			targetUnit.PlayHitAnimation();

			// Check if it is a STATUS move
			if (move.Base.Category == MoveCategory.Status)
			{
				yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
			}
			else
			{
				var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
				yield return targetUnit.Hud.UpdateHP();
				yield return ShowDamageDetails(damageDetails);
			}

			if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
			{
				foreach (var secondary in move.Base.Secondaries)
				{
					var rnd = UnityEngine.Random.Range(0,101);
					if (rnd <= secondary.Chance)
						yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
				}
			} 

			if (targetUnit.Pokemon.HP <= 0)
			{
				yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted");
				targetUnit.PlayFaintAnimation();
				yield return new WaitForSeconds(2f);
				CheckForBattleOver(targetUnit);
			}

			
		}

		else
		{
			yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
		}
			
	}

	private IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
	{
		//Stat Boosting
		if (effects.Boosts != null)
		{

			if (moveTarget == MoveTarget.Self)
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

		if (effects.VolatileStatus != ConditionID.none)
		{
			target.SetVolatileStatus(effects.VolatileStatus);
		}

		yield return ShowStatusChanges(target);
		yield return ShowStatusChanges(source);
	}

	private IEnumerator RunAfterTurn(BattleUnit sourceUnit)
	{
		if (state == BattleState.BattleOver) yield break;

		yield return new WaitUntil(() => state == BattleState.RunningTurn);
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

	private bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
	{
		if (move.Base.AlwaysHits)
			return true;

		float moveAccuracy = move.Base.Accuracy;

		int accuracy = source.StatBoosts[Stat.Accuracy];
		int evasion = target.StatBoosts[Stat.Evasion];

		var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f }; // fractions

		if (accuracy > 0)
			moveAccuracy *= boostValues[accuracy];
		else 
			moveAccuracy /= boostValues[-accuracy];

		if (evasion > 0)
			moveAccuracy /= boostValues[evasion];
		else
			moveAccuracy *= boostValues[-evasion];

		return (UnityEngine.Random.Range(1, 101) <= moveAccuracy);
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
