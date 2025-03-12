using UnityEngine;
using Jint;
using System.IO;

public class JavascriptEngine
{
    private Engine jsEngine;

    public JavascriptEngine()
    {
        jsEngine = new Engine(cfg => cfg.AllowClr(typeof(GameObject).Assembly));

        jsEngine.SetValue("GameObject", typeof(GameObject));
        jsEngine.SetValue("Vector3", typeof(Vector3));
        jsEngine.SetValue("Color", typeof(Color));
        jsEngine.SetValue("ForceMode", typeof(ForceMode));
        jsEngine.SetValue("Shader", typeof(Shader));
        jsEngine.SetValue("Material", typeof(Material));
        jsEngine.SetValue("Physics", typeof(Physics));
        jsEngine.SetValue("RaycastHit", typeof(RaycastHit));
        jsEngine.SetValue("LineRenderer", typeof(LineRenderer));
        jsEngine.SetValue("Transform", typeof(Transform));
        jsEngine.SetValue("Debug", typeof(Debug));
    }

    public void LoadScript(string path)
    {
        string jsCode = File.ReadAllText(path);
        jsEngine.Execute(jsCode);
    }

    public object GetGlobal(string name)
    {
        return jsEngine.GetValue(name).ToObject();
    }

    public void Execute(string code)
    {
        jsEngine.Execute(code);
    }
}