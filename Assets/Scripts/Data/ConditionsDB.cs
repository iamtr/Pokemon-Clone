using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConditionsDB
{
    // static, so that we do not have to create a new instance everytime we want to access Conditions
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

        }


    };

    
}

public enum ConditionID
{
    //poison, burn, sleep, paralyze, freeze
    none, psn, brn, slp, par, frz
}