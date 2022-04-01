using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleHud : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private HPBar hpBar;

    private Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
	{
        _pokemon = pokemon;
        nameText.text = pokemon.Base.name;
        levelText.text = "Lvl " + pokemon.Level.ToString();
        hpBar.SetHp((float) pokemon.HP / pokemon.MaxHp);
	}

    public IEnumerator UpdateHP()
	{
        if (_pokemon.HpChanged)
		{
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
        
    }
}
