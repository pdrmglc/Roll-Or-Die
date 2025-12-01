using UnityEngine;

[System.Serializable]
public struct UnitStats
{
    public float speed;
    public float perception;
    public float endurance;
    public float strength;

    public float mana;

    public UnitStats(float newSpeed, float newPerception, float newEndurance, float newStrength, float newMana)
    {
        speed = newSpeed;
        perception = newPerception;
        endurance = newEndurance;
        strength = newStrength;
        mana = newMana;
    }
}
