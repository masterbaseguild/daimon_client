using UnityEngine;
using Jint;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class InputManager : MonoBehaviour
{
    private JavascriptEngine javascriptEngine;
    public int slotCount;
    bool isEnabled;

    AbilitySlot[] slots;

    public void Enable()
    {
        javascriptEngine = new JavascriptEngine();

        slots = new AbilitySlot[slotCount];

        // temp
        javascriptEngine.LoadScript("Assets/Scripts/HookL.js");
        string hookLsymbol = "HookL";

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var strippedGameObject = new {
            transform = gameObject.transform,
            MainUser = gameObject.GetComponent<MainUser>(),
            Rigidbody = gameObject.GetComponent<Rigidbody>(),
        };

        string convertedGameObject = JsonConvert.SerializeObject(strippedGameObject, settings);

        javascriptEngine.Execute("var gameObject = " + convertedGameObject + ";");
        javascriptEngine.Execute("var HookL = new HookL(gameObject);");
        javascriptEngine.Execute("HookL.Test()");

        //SetSlot(0, new HookL(gameObject), KeyCode.Q, ScriptType.csharp);
        SetSlot(0, hookLsymbol, KeyCode.Q, ScriptType.javascript);
        SetSlot(1, new HookR(gameObject), KeyCode.E, ScriptType.csharp);
        isEnabled = true;
    }

    public void SetSlot(int index, Ability ability, KeyCode key, ScriptType type)
    {
        slots[index] = new AbilitySlot(ability, key);
    }
    public void SetSlot(int index, string ability, KeyCode key, ScriptType type)
    {
        slots[index] = new AbilitySlot(ability, key);
    }

    private void RunFunction(int index, string functionName)
    {
        switch (slots[index].type)
        {
            case ScriptType.csharp:
                slots[index].csharpAbility.GetType().GetMethod(functionName).Invoke(slots[index].csharpAbility, null);
                break;
            case ScriptType.javascript:
                // invoke javascript function
                javascriptEngine.Execute("HookL." + functionName + "();");
                break;
            case ScriptType.lua:
                break;
            case ScriptType.python:
                break;
        }
    }

    void Update()
    {
        if (!isEnabled) return;
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) continue;
            if (Input.GetKeyDown(slots[i].key))
            {
                RunFunction(i, "Start");
                //slots[i].ability.Start();
            }
            if (Input.GetKey(slots[i].key))
            {
                RunFunction(i, "Frame");
                //slots[i].ability.Frame();
            }
            if (Input.GetKeyUp(slots[i].key))
            {
                RunFunction(i, "Stop");
                //slots[i].ability.Stop();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isEnabled) return;
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) continue;
            if (Input.GetKey(slots[i].key))
            {
                RunFunction(i, "Tick");
                //slots[i].ability.Tick();
            }
        }
    }
}