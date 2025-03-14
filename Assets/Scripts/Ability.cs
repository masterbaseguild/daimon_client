using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class Ability
{
    private Script script;
    private DynValue scriptObject;

    public Ability(string path, GameObject user)
    {
        // init script
        script = new Script();

        // register unity namespace
        RegisterUnityEngineTypes();

        // register and pass Debug
        UserData.RegisterType<Debug>();
        script.Globals["Debug"] = DynValue.FromObject(script, new Debug());

        // pass user
        script.Globals["user"] = DynValue.FromObject(script, user);

        // set scriptObject
        scriptObject = script.DoString(File.ReadAllText(path));
    }

    private void RegisterUnityEngineTypes()
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
        Debug.Log("Ability started");
        // run start method of scriptObject
        var startMethod = scriptObject.Table.Get("Start").Function;
        script.Call(startMethod);
    }

    public void Frame()
    {
        // run frame method of scriptObject
    }

    public void Tick()
    {
        // run tick method of scriptObject
    }

    public void Stop()
    {
        Debug.Log("Ability stopped");
        // run stop method of scriptObject
    }
}