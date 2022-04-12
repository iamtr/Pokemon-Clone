using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Move/Create New Move")]
public class MoveBase : ScriptableObject
{
	[SerializeField] private string moveName;
	[TextArea][SerializeField] private string description;
	[SerializeField] private PokemonType type;
	[SerializeField] private int power;
	[SerializeField] private int accuracy;
	[SerializeField] private int pp;
	[SerializeField] private int priority;
	[SerializeField] private MoveCategory category;
	[SerializeField] private MoveEffects effects;
	[SerializeField] private MoveTarget target;
	[SerializeField] private bool alwaysHits;
	[SerializeField] private List<SecondaryMoveEffects> secondaries;
	public string Name => name;
	public string Description => description;
	public PokemonType Type => type;
	public int Power => power;
	public int Accuracy => accuracy;
	public int PP => pp;
	public int Priority => priority;
	public MoveCategory Category => category;
	public MoveEffects Effects => effects;
	public MoveTarget Target => target;
	public bool AlwaysHits => alwaysHits;
	public List<SecondaryMoveEffects> Secondaries => secondaries; //secondary effects
}
public enum MoveCategory
{
	Physical, Special, Status
}

public enum MoveTarget
{
	Foe, Self
}

//For STATUS moves
[System.Serializable]
public class MoveEffects 
{
	[SerializeField] private List<StatBoost> boosts;
	[SerializeField] private ConditionID status;
	[SerializeField] private ConditionID volatileStatus;
	public List<StatBoost> Boosts => boosts;
	public ConditionID Status => status;
	public ConditionID VolatileStatus => volatileStatus;
}

[System.Serializable]
public class SecondaryMoveEffects : MoveEffects
{
	[SerializeField] int chance;
	[SerializeField] MoveTarget target;

	public int Chance => chance;
	public MoveTarget Target => target;
}
//This class is used to serialize the variables "stat" and "boost"
[System.Serializable]
public class StatBoost
{
	public Stat stat;
	public int boost;
}

