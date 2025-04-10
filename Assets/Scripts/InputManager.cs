using UnityEngine;
using System.Linq;

public class InputManager : MonoBehaviour
{
    [SerializeField] private MainUdpClient mainUdpClient;
    [SerializeField] private int slotCount;

    private AbilitySlot[] slots;

    private void Start()
    {
        slots = new AbilitySlot[slotCount];

        string path = Application.streamingAssetsPath + "/Abilities/";

        // temp
        SetSlot(0, new Ability(path + "HookL.lua", gameObject), KeyCode.Q);
        SetSlot(1, new Ability(path + "HookR.lua", gameObject), KeyCode.E);
    }

    private void SetSlot(int index, Ability ability, KeyCode key)
    {
        slots[index] = new AbilitySlot(ability, key);
    }

    private void Update()
    {
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

    public void Receive(string[] packet)
    {
        int packetType = int.Parse(packet[0]);
        string[] data = packet.Skip(1).ToArray();
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            if (slots[i].ability.prefix == packetType)
            {
                slots[i].ability.Receive(data);
            }
        }
    }

    public void Send(string[] packet)
    {
        int packetType = Packet.Server.SCRIPT;
        int index = mainUdpClient.GetIndex();
        var packetString = string.Join("\t", packet);
        string message = packetType + "\t" + index + "\t" + packetString;
        Debug.Log("Sending packet: " + message);
        mainUdpClient.Send(message);
    }
}