using UnityEngine;

public class AbilitySlot
{
    public Ability csharpAbility;
    public object javascriptAbility;
    public KeyCode key;
    public ScriptType type;
    public AbilitySlot(Ability csharpAbility, KeyCode key)
    {
        this.csharpAbility = csharpAbility;
        this.key = key;
        this.type = ScriptType.csharp;
    }
    public AbilitySlot(object javascriptAbility, KeyCode key)
    {
        this.javascriptAbility = javascriptAbility;
        this.key = key;
        this.type = ScriptType.javascript;
    }
}