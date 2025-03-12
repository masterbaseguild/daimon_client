using UnityEngine;

public class Ability
{
    public Ability(GameObject user)
    {
    }

    public virtual void Start()
    {
        Debug.Log("Ability started");
    }

    public virtual void Frame()
    {
        Debug.Log("Ability frame");
    }

    public virtual void Tick()
    {
        Debug.Log("Ability tick");
    }

    public virtual void Stop()
    {
        Debug.Log("Ability stopped");
    }
}