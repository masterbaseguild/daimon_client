---@diagnostic disable: undefined-global

local updraftPower = 500

local script = user:GetComponent("MainUser")
local userRigidBody = user:GetComponent("Rigidbody")
local physicsMultiplier = script.physicsMultiplier

function Start()
    local startForce = Vector3.up * updraftPower * physicsMultiplier
    AddForce(userRigidBody, startForce, "Force")
end