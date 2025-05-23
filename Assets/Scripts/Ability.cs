using UnityEngine;
using MoonSharp.Interpreter;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

public class Ability
{
    private readonly Script script;
    private InputManager inputManager;

    public int prefix;

    // support functions
    private GameObject CreateGameObject(string name)
    {
        return new GameObject(name);
    }
    private Material CreateMaterial(Shader shader)
    {
        return new Material(shader);
    }
    private LineRenderer AddLineRenderer(GameObject gameObject)
    {
        return gameObject.AddComponent<LineRenderer>();
    }
    private bool RaycastCheck(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        return Physics.Raycast(origin, direction, distance, layerMask);
    }
    private RaycastHit RaycastValue(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        _ = Physics.Raycast(origin, direction, out RaycastHit hit, distance, layerMask);
        return hit;
    }
    private Vector3 Normalize(Vector3 vector)
    {
        return vector.normalized;
    }
    private void AddForce(Rigidbody rigidbody, Vector3 force, string forceMode)
    {
        ForceMode mode = (ForceMode)Enum.Parse(typeof(ForceMode), forceMode);
        rigidbody.AddForce(force, mode);
    }
    private Vector3 CreateVector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }
    private void Send(string[] packet)
    {
        inputManager.Send(packet);
    }

    public Ability(string path, GameObject user)
    {
        // get input manager
        inputManager = GameObject.Find("MainUser").GetComponent<InputManager>();
        // init script
        script = new Script();

        // register unity namespace
        RegisterUnityEngineTypes();
        _ = UserData.RegisterType<MainUser>();
        _ = UserData.RegisterType<RaycastHit>();
        _ = UserData.RegisterType<Rigidbody>();

        // pass objects
        script.Globals["Debug"] = DynValue.FromObject(script, new Debug());
        script.Globals["Vector3"] = DynValue.FromObject(script, new Vector3());

        // pass classes
        script.Globals["Shader"] = DynValue.FromObject(script, typeof(Shader));
        script.Globals["Color"] = DynValue.FromObject(script, typeof(Color));

        script.Globals["CreateGameObject"] = DynValue.FromObject(script, new Func<string, GameObject>(CreateGameObject));
        script.Globals["CreateMaterial"] = DynValue.FromObject(script, new Func<Shader, Material>(CreateMaterial));
        script.Globals["AddLineRenderer"] = DynValue.FromObject(script, new Func<GameObject, LineRenderer>(AddLineRenderer));
        script.Globals["RaycastCheck"] = DynValue.FromObject(script, new Func<Vector3, Vector3, float, LayerMask, bool>(RaycastCheck));
        script.Globals["RaycastValue"] = DynValue.FromObject(script, new Func<Vector3, Vector3, float, LayerMask, RaycastHit>(RaycastValue));
        script.Globals["Normalize"] = DynValue.FromObject(script, new Func<Vector3, Vector3>(Normalize));
        script.Globals["AddForce"] = DynValue.FromObject(script, new Action<Rigidbody, Vector3, string>(AddForce));
        script.Globals["CreateVector3"] = DynValue.FromObject(script, new Func<float, float, float, Vector3>(CreateVector3));
        script.Globals["Send"] = DynValue.FromObject(script, new Action<string[]>(Send));

        // pass user
        script.Globals["user"] = DynValue.FromObject(script, user);

        // run script
        _ = script.DoString(File.ReadAllText(path));

        // get prefix from lua global variable "prefix"
        DynValue prefixValue = script.Globals.Get("Prefix");
        if (prefixValue == null)
        {
            Debug.LogError("Prefix not found in script");
            return;
        }
        prefix = (int)prefixValue.Number;
    }

    private void RegisterUnityEngineTypes()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(GameObject));
        Type[] types = assembly.GetTypes().Where(static t => t.Namespace == "UnityEngine").ToArray();

        foreach (Type type in types)
        {
            _ = UserData.RegisterType(type);
        }
    }

    public void Start()
    {
        object function = script.Globals["Start"];
        if (function == null)
        {
            return;
        }
        _ = script.Call(script.Globals["Start"]);
    }

    public void Frame()
    {
        object function = script.Globals["Frame"];
        if (function == null)
        {
            return;
        }
        _ = script.Call(script.Globals["Frame"]);
    }

    public void Tick()
    {
        object function = script.Globals["Tick"];
        if (function == null)
        {
            return;
        }
        _ = script.Call(script.Globals["Tick"]);
    }

    public void Stop()
    {
        object function = script.Globals["Stop"];
        if (function == null)
        {
            return;
        }
        _ = script.Call(script.Globals["Stop"]);
    }

    public void Receive(string[] packet)
    {
        var packetString = string.Join("_", packet);
        Debug.Log("Ability received packet: " + packetString);
        Table luaTable = new Table(script);
        for (int i = 0; i < packet.Length; i++)
        {
            luaTable[i + 1] = packet[i]; // Lua is 1-based index
        }
        object function = script.Globals["Receive"];
        if (function == null)
        {
            Debug.LogError("Receive function not found in script");
            return;
        }
        _ = script.Call(script.Globals["Receive"], luaTable);
    }
}