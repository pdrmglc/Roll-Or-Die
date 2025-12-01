using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Weapon Ability")]
public class WeaponAbility : AbilityData
{
    public int damage;

    public override void Execute(Unit user, Unit target)
    {
        Debug.Log($"{user.name} atacou {target.name} com {abilityName} causando {damage}");
        target.TakeDamage(damage);
    }
}
