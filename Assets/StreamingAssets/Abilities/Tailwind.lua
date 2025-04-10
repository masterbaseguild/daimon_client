---@diagnostic disable: undefined-global

Prefix = 3

local tailwindPower = 500

local script = user:GetComponent("MainUser")
local userCamera = script.playerCamera.transform
local userRigidBody = user:GetComponent("Rigidbody")
local physicsMultiplier = script.physicsMultiplier

function Start()
    local horizontalDirection = Normalize(CreateVector3(userCamera.forward.x, 0, userCamera.forward.z))
    local startForce = horizontalDirection * tailwindPower * physicsMultiplier
    AddForce(userRigidBody, startForce, "Force")
end