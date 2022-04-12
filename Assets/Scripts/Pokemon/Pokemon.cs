using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon 
{
    [SerializeField] private PokemonBase _base;
    [SerializeField] private int  level;

    public PokemonBase Base => _base;
    public int Level => level;
    public int HP { get; set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    ///<summary> Allows messages to be shown onto dialog box, eg poison. <br></br> <br></br> 
    ///Text displayed via BattleSystem.ShowStatusChanges() </summary>
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public Condition Status { get; private set; }
    public Condition VolatileStatus { get; private set; }
    public event System.Action OnStatusChanged;
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int MaxHp
    {
        get; private set;
    }

    public bool HpChanged { get; set; }
    public void Init()
	{
        Moves = new List<Move>(); //why not declare at the start?
        foreach (var move in Base.LearnableMoves)
		{
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base)); // only allows moves >= pokemon level
            if (Moves.Count >= 4) break;
		}

        CalculateStats();
        HP = MaxHp;
        ResetStatBoost();

        Status = null;
        VolatileStatus = null;
    }

	private void CalculateStats()
	{
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100) + 50 + Level;
    }
    private void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0 },
            {Stat.Speed, 0},

            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}   
        };
    }
        
    private int GetStat(Stat stat)
	{
        int statValue = Stats[stat];
        //Logic...
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statValue = Mathf.FloorToInt(statValue * boostValues[boost]);
        else if (boost < 0)
            statValue = Mathf.FloorToInt(statValue * boostValues[-boost]);
        
        return statValue;
	}

    public void ApplyBoosts(List<StatBoost> statBoosts) 
	{
        foreach (var statBoost in statBoosts)
		{
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
			{
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
			}
			else
			{
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {boost}");
		}
	}

    public void SetVolatileStatus(ConditionID conditionID)
	{
        if (VolatileStatus != null) return;
        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
	}

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;
        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
        
    }
    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
       
    }
    public DamageDetails TakeDamage(Move move, Pokemon Attacker)
    {
        float critical = 1f;
        float temp = Random.value * 100f;
        if (temp <= 6.25)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Type = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special)? Attacker.SpAttack: Attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * Attacker.Level * 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public Move GetRandomMove()
	{
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
	}

    public void OnBattleOver()
	{
        VolatileStatus = null;
        ResetStatBoost();
	}

    public void UpdateHP(int damage)
	{
        HpChanged = true;
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
	}


    public bool OnBeforeMove()
	{
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null)
		{
            if (!Status.OnBeforeMove(this))
                canPerformMove = false; 
		}

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
	}
    ///<summary> Pokemon.OnAfterTurn calls OnAfterTurn from Conditions class.Takes no parameters <br></br> <br></br> 
    ///Do not confuse with Condition.OnAfterTurn()! </summary>
    public void OnAfterTurn()
	{
        // delegate() and delegate.Invoke() does the same thing.
        // We wanted to check if OnAfterTurn is null, therefore Invoke() has to be used
        // Or else there will be errors

        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
	}
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Type { get; set; }
}


