using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConditionsDB
{
    // static, so that we do not have to create a new instance everytime we want to access Conditions

	public static void Init()
	{
		foreach (var kvp in Conditions)
		{
			var conditionID = kvp.Key;
			var condition = kvp.Value;

			condition.Id = conditionID;
		}
	}

	/// <summary>
	/// Static database for pokemon conditions
	/// </summary>
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
	{
		{
			ConditionID.psn,
			new Condition()
			{
				Name = "Poison",
				StartMessage = "has been poisoned",
				OnAfterTurn = (Pokemon pokemon) =>
				{
					pokemon.UpdateHP(pokemon.MaxHp / 8);
					pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison!");
				}
			}

		},

		{
			ConditionID.brn,
			new Condition()
			{
				Name = "Burn",
				StartMessage = "has been burned",
				OnAfterTurn = (Pokemon pokemon) =>
				{
					pokemon.UpdateHP(pokemon.MaxHp / 16);
					pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is burning!");
				}
			}
		},

		{
			ConditionID.par,
			new Condition()
			{
				Name = "Paralyze",
				StartMessage = "has been paralyzed",
				OnBeforeMove = (Pokemon pokemon) =>
				{
					if (Random.Range(1, 5) == 1)
					{
						pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and cannot move!");
						return false;
					}

					return true;
				},
				//OnAfterTurn = (Pokemon pokemon) => 
				//{
				//	pokemon.CureStatus();
				//	pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer paralyzed!");
				//} 
					
				
			}
		},

		{
			ConditionID.frz,
			new Condition()
			{
				Name = "Freeze",
				StartMessage = "has been frozen",
				OnBeforeMove = (Pokemon pokemon) =>
				{
					if (Random.Range(1, 5) == 1)
					{
						pokemon.CureStatus();
						pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s not frozen anymore.");
						return true;
					}

					return false;
				}
			}
		},

		{
			ConditionID.slp,
			new Condition()
			{
				Name = "Sleep",
				StartMessage = "fell asleep!",
				OnStart = (Pokemon pokemon) =>
				{
					pokemon.StatusTime = Random.Range(1, 4);
					Debug.Log($"Will be asleep for {pokemon.StatusTime} turns");
				},
				OnBeforeMove = (Pokemon pokemon) =>
				{
					if (pokemon.StatusTime <= 0)
					{
						pokemon.CureStatus();
						pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
						return true;

					}
					pokemon.StatusTime--;
					pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping!");
					return false;
				}
			}
		},

		// Volatile Stasuses
		{
			ConditionID.con,
			new Condition()
			{
				Name = "Confusion",
				StartMessage = "is confused!",
				OnStart = (Pokemon pokemon) =>
				{
					pokemon.VolatileStatusTime = Random.Range(1, 4);
					Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} turns");
				},
				OnBeforeMove = (Pokemon pokemon) =>
				{
					if (pokemon.VolatileStatusTime <= 0)
					{
						pokemon.CureVolatileStatus();
						pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer confused!");

					}
					pokemon.VolatileStatusTime--;

					if (Random.Range(1, 3) == 1)
						return true;

					pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");
					pokemon.UpdateHP(pokemon.MaxHp / 8);
					pokemon.StatusChanges.Enqueue($"It hurt itself due to confusion!");
					return false;
				}
			}
		}


	};

    
}
/// <summary>
/// Enum that contains all possible pokemon conditions
/// </summary>
public enum ConditionID
{
    //poison, burn, sleep, paralyze, freeze
    none, psn, brn, slp, par, frz,
	con
}