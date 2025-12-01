using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Magic Ability")]
public class MagicAbility : AbilityData
{
    public int baseDamage;
    public int manaCost;
    public bool hasAreaOfEffect;
    public int aoeRadius;

    public override void Execute(Unit user, Unit target)
    {
        if (user.mana < manaCost)
        {
            Debug.Log("Mana insuficiente!");
            return;
        }

        user.mana -= manaCost;

        Debug.Log($"{user.name} conjurou {abilityName} causando {baseDamage}");
        target.TakeDamage(baseDamage);

        if (hasAreaOfEffect)
        {
            // AOE expansion
        }
    }
}
