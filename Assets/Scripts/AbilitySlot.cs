using UnityEngine;

public class AbilitySlot
{
    public Ability ability;
    public KeyCode key;
    public ScriptType type;
    public AbilitySlot(Ability ability, KeyCode key)
    {
        this.ability = ability;
        this.key = key;
        this.type = ScriptType.csharp;
    }
}