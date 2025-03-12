var HookL = {
    user: null,
    userCamera: null,
    userRigidBody: null,
    physicsMultiplier: 1.0,
    ground: null,
    hookRange: 125.0,
    hookPower: 50.0,
    isHooked: false,
    hookPos: new Vector3(0, 0, 0),
    lineRenderer1: null,
    lineRenderer2: null,

    Init: function(user) {
        Debug.Log("HookL initialized");
        this.user = user.transform;
        var userScript = user.MainUser;
        this.userCamera = userScript.playerCamera.transform;
        this.userRigidBody = user.RigidBody;
        this.physicsMultiplier = userScript.physicsMultiplier;
        this.ground = userScript.ground;

        var Hook1 = new GameObject("HookL1");
        var Hook2 = new GameObject("HookL2");
        Hook1.transform.parent = this.user;
        Hook2.transform.parent = this.user;

        this.lineRenderer1 = Hook1.AddComponent("LineRenderer");
        this.lineRenderer2 = Hook2.AddComponent("LineRenderer");

        this.lineRenderer1.startWidth = 0.1;
        this.lineRenderer1.endWidth = 0.1;
        this.lineRenderer2.startWidth = 0.1;
        this.lineRenderer2.endWidth = 0.1;
        
        var mat = new Material(Shader.Find("Sprites/Default"));
        this.lineRenderer1.material = mat;
        this.lineRenderer2.material = mat;

        this.lineRenderer1.startColor = Color.blue;
        this.lineRenderer1.endColor = Color.blue;
        this.lineRenderer2.startColor = Color.blue;
        this.lineRenderer2.endColor = Color.blue;
    },

    Start: function() {
        Debug.Log("HookL started");
        var hit = new RaycastHit();
        if (Physics.Raycast(this.userCamera.position, this.userCamera.forward, hit, this.hookRange, this.ground)) {
            this.hookPos = hit.point;
            print("Hook Left: from " + this.userCamera.position + " to " + this.hookPos);
            this.isHooked = true;

            var startForce = (this.hookPos - this.user.position).normalized * this.hookPower * this.physicsMultiplier;
            if (startForce.y < this.physicsMultiplier * this.hookPower * 0.5)
                startForce = startForce + Vector3.up * (this.physicsMultiplier * this.hookPower * 0.5 - startForce.y);
            if (this.hookPos.y >= this.user.position.y)
                startForce = startForce + Vector3.up * (this.hookPos.y - this.user.position.y) * this.physicsMultiplier;

            this.userRigidBody.AddForce(startForce, ForceMode.Force);

            this.lineRenderer1.enabled = true;
            this.lineRenderer1.SetPosition(0, this.user.position - this.user.right * 0.5 + this.user.up * 0.5);
            this.lineRenderer1.SetPosition(1, this.hookPos);

            this.lineRenderer2.enabled = true;
            this.lineRenderer2.SetPosition(0, this.user.position - this.user.right * 0.5 - this.user.up * 0.5);
            this.lineRenderer2.SetPosition(1, this.hookPos);
        }
    },

    Test: function() {
        Debug.Log("HookL test!");
    },

    Frame: function() {
        if (this.isHooked) {
            this.lineRenderer1.SetPosition(0, this.user.position - this.user.right * 0.5 + this.user.up * 0.5);
            this.lineRenderer2.SetPosition(0, this.user.position - this.user.right * 0.5 - this.user.up * 0.5);
        }
    },

    Tick: function() {
        if (this.isHooked) {
            var direction = (this.hookPos - this.user.position).normalized;
            var force = direction * this.hookPower * this.physicsMultiplier;
            this.userRigidBody.AddForce(force, ForceMode.Force);
            if (Vector3.Angle(this.userRigidBody.velocity, force) > 90)
                this.userRigidBody.AddForce(-this.userRigidBody.velocity * this.physicsMultiplier);
        }
    },

    Stop: function() {
        Debug.Log("HookL stopped");
        this.isHooked = false;
        this.lineRenderer1.enabled = false;
        this.lineRenderer2.enabled = false;
    }
};