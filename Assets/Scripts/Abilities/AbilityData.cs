using UnityEngine;

public enum AbilityType
{
    Melee,
    Ranged,
    Magic,
    Utility
}

[System.Serializable]
public abstract class AbilityData : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public AbilityType type;

    public int range;
    public int cost;
    public bool requiresLineOfSight = true;

    public abstract void Execute(Unit user, Unit target);
}
