using UnityEngine;

public class AbilitySlot
{
    public Ability ability;
    public KeyCode key;
    public AbilitySlot(Ability ability, KeyCode key)
    {
        this.ability = ability;
        this.key = key;
    }
}