

function Start()
    Debug.Log("HookL started!")
    Debug.Log("User position: " .. tostring(user.transform.position))
end

function Frame()
    Debug.Log("HookL running every frame")
end

function Tick()
    Debug.Log("HookL physics update")
end

function Stop()
    Debug.Log("HookL stopped!")
end