using UnityEngine;

public class InputManager : MonoBehaviour
{
    public int slotCount;
    private bool isEnabled;

    private AbilitySlot[] slots;

    public void Enable()
    {
        slots = new AbilitySlot[slotCount];

        // temp
        SetSlot(0, new Ability("Assets/Scripts/Abilities/HookL.lua", gameObject), KeyCode.Q);
        SetSlot(1, new Ability("Assets/Scripts/Abilities/HookR.lua", gameObject), KeyCode.E);
        isEnabled = true;
    }

    private void SetSlot(int index, Ability ability, KeyCode key)
    {
        slots[index] = new AbilitySlot(ability, key);
    }

    private void Update()
    {
        if (!isEnabled)
        {
            return;
        }


        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }


            if (Input.GetKeyDown(slots[i].key))
            {
                slots[i].ability.Start();
            }
            if (Input.GetKey(slots[i].key))
            {
                slots[i].ability.Frame();
            }
            if (Input.GetKeyUp(slots[i].key))
            {
                slots[i].ability.Stop();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isEnabled)
        {
            return;
        }


        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }


            if (Input.GetKey(slots[i].key))
            {
                slots[i].ability.Tick();
            }
        }
    }
}