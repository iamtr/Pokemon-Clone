using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleHud : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text statusText;
    [SerializeField] private HPBar hpBar;

    [SerializeField] private Color psnColor;
    [SerializeField] private Color brnColor;
    [SerializeField] private Color frzColor;
    [SerializeField] private Color parColor;
    [SerializeField] private Color slpColor;

    private Pokemon _pokemon;

    private Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
	{
        _pokemon = pokemon;
        nameText.text = pokemon.Base.name;
        levelText.text = "Lvl " + pokemon.Level.ToString();
        hpBar.SetHp((float) pokemon.HP / pokemon.MaxHp);

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.frz, frzColor },
            {ConditionID.par, parColor },
            {ConditionID.slp, slpColor }
        };
	}

    public void SetStatusText()
	{
        if (_pokemon.Status == null)
		{
            statusText.text = "";
		}
        else
		{
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
		}

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
