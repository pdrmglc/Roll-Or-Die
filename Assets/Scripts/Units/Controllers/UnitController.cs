using UnityEngine;

public abstract class UnitController : MonoBehaviour, ITickable
{
    protected Unit unit;

    public virtual void Initialize(Unit u)
    {
        unit = u;
    }

    public abstract void Tick();
}
