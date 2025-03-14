HookL = {}
HookL.__index = HookL

function HookL.new()
    local self = setmetatable({}, HookL)
    self.user = user

    self.Start = HookL.Start
    self.Frame = HookL.Frame
    self.Tick = HookL.Tick
    self.Stop = HookL.Stop
    return self
end

function HookL:Start()
    Debug.Log("HookL started!")
    Debug.Log("User position: " .. tostring(self.user.transform.position))
end

function HookL:Frame()
    Debug.Log("HookL running every frame")
end

function HookL:Tick()
    Debug.Log("HookL physics update")
end

function HookL:Stop()
    Debug.Log("HookL stopped!")
end

return HookL.new()