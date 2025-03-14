using UnityEngine;

public class HookR
{
    
    // parameters
    Transform user;
    Transform userCamera;
    Rigidbody userRigidBody;
    float physicsMultiplier;
    LayerMask ground;
    float hookRange = 125f;
    float hookPower = 50f;

    // state
    LineRenderer lineRenderer1;
    LineRenderer lineRenderer2;
    bool isHooked = false;
    Vector3 hookPos;

    public HookR(GameObject user)
    {
        MainUser script = user.GetComponent<MainUser>();
        this.user = user.transform;
        this.userCamera = script.playerCamera.transform;
        this.userRigidBody = user.GetComponent<Rigidbody>();
        this.physicsMultiplier = script.physicsMultiplier;
        this.ground = script.ground;

        GameObject Hook1 = new GameObject("HookR1");
        GameObject Hook2 = new GameObject("HookR2");
        Hook1.transform.parent = user.transform;
        Hook2.transform.parent = user.transform;
        lineRenderer1 = Hook1.AddComponent<LineRenderer>();
        lineRenderer2 = Hook2.AddComponent<LineRenderer>();

        lineRenderer1.startWidth = 0.1f;
        lineRenderer1.endWidth = 0.1f;
        lineRenderer2.startWidth = 0.1f;
        lineRenderer2.endWidth = 0.1f;
        lineRenderer1.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer1.startColor = Color.blue;
        lineRenderer1.endColor = Color.blue;
        lineRenderer2.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer2.startColor = Color.blue;
        lineRenderer2.endColor = Color.blue;
    }

    public void Start()
    {
        RaycastHit hit;
        if(Physics.Raycast(userCamera.position, userCamera.forward, out hit, hookRange, layerMask: ground))
        {
            hookPos = hit.point;
            Debug.Log("Hook Right: from "+userCamera.position+" to "+hookPos+"");
            isHooked = true;
            Vector3 startForce = (hookPos - user.position).normalized * hookPower * physicsMultiplier;
            if (startForce.y < physicsMultiplier * hookPower * 0.5f) startForce += Vector3.up * (physicsMultiplier * hookPower * 0.5f - startForce.y);
            if (hookPos.y >= user.position.y) startForce += Vector3.up * (hookPos.y - user.position.y) * physicsMultiplier;
            userRigidBody.AddForce(startForce, ForceMode.Force);
            lineRenderer1.enabled = true;
            lineRenderer1.SetPosition(0, user.position + user.right * 0.5f + user.up * 0.5f);
            lineRenderer1.SetPosition(1, hookPos);
            lineRenderer2.enabled = true;
            lineRenderer2.SetPosition(0, user.position + user.right * 0.5f - user.up * 0.5f);
            lineRenderer2.SetPosition(1, hookPos);
        }
    }

    public void Frame()
    {
        if (isHooked)
        {
            lineRenderer1.SetPosition(0, user.position + user.right * 0.5f + user.up * 0.5f);
            lineRenderer2.SetPosition(0, user.position + user.right * 0.5f - user.up * 0.5f);
        }
    }

    public void Tick()
    {
        if (isHooked)
        {
            Vector3 direction = (hookPos - user.position).normalized;
            Vector3 force = direction * hookPower * physicsMultiplier;
            userRigidBody.AddForce(force, ForceMode.Force);
            if (Vector3.Angle(userRigidBody.velocity, force) > 90f) userRigidBody.AddForce(-userRigidBody.velocity * physicsMultiplier);
        }
    }

    public void Stop()
    {
        isHooked = false;
        lineRenderer1.enabled = false;
        lineRenderer2.enabled = false;
    }
}