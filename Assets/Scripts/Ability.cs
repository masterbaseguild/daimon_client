using UnityEngine;
using MoonSharp.Interpreter;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

public class Ability
{
    Script script;

    // support functions
    GameObject CreateGameObject(string name)
    {
        return new GameObject(name);
    }
    Material CreateMaterial(Shader shader)
    {
        return new Material(shader);
    }
    LineRenderer AddLineRenderer(GameObject gameObject)
    {
        return gameObject.AddComponent<LineRenderer>();;
    }
    bool RaycastCheck(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        return Physics.Raycast(origin, direction, distance, layerMask);
    }
    RaycastHit RaycastValue(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
    {
        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, distance, layerMask);
        return hit;
    }
    Vector3 Normalize(Vector3 vector)
    {
        return vector.normalized;
    }
    void AddForce(Rigidbody rigidbody, Vector3 force, string forceMode)
    {
        ForceMode mode = (ForceMode)Enum.Parse(typeof(ForceMode), forceMode);
        rigidbody.AddForce(force, mode);
    }
    Vector3 CreateVector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    public Ability(string path, GameObject user)
    {
        // init script
        script = new Script();

        // register unity namespace
        RegisterUnityEngineTypes();
        UserData.RegisterType<MainUser>();
        UserData.RegisterType<RaycastHit>();
        UserData.RegisterType<Rigidbody>();

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

        // pass user
        script.Globals["user"] = DynValue.FromObject(script, user);

        // run script
        script.DoString(File.ReadAllText(path));
    }

    void RegisterUnityEngineTypes()
    {
        var assembly = Assembly.GetAssembly(typeof(GameObject));
        var types = assembly.GetTypes().Where(t => t.Namespace == "UnityEngine").ToArray();

        foreach (var type in types)
        {
            UserData.RegisterType(type);
        }
    }

    public void Start()
    {
        object function = script.Globals["Start"];
        if (function == null)
        {
            return;
        }
        script.Call(script.Globals["Start"]);
    }

    public void Frame()
    {
        object function = script.Globals["Frame"];
        if (function == null)
        {
            return;
        }
        script.Call(script.Globals["Frame"]);
    }

    public void Tick()
    {
        object function = script.Globals["Tick"];
        if (function == null)
        {
            return;
        }
        script.Call(script.Globals["Tick"]);
    }

    public void Stop()
    {
        object function = script.Globals["Stop"];
        if (function == null)
        {
            return;
        }
        script.Call(script.Globals["Stop"]);
    }
}