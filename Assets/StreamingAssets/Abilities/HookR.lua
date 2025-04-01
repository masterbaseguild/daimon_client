---@diagnostic disable: undefined-global

local userObject
local userCamera
local userRigidBody
local physicsMultiplier
local ground
local hookRange = 125
local hookPower = 50

local lineRenderer1
local lineRenderer2
local isHooked = false
local hookPos

local script = user:GetComponent("MainUser")
userObject = user.transform
userCamera = script.playerCamera.transform
userRigidBody = user:GetComponent("Rigidbody")
physicsMultiplier = script.physicsMultiplier
ground = script.ground

local Hook1 = CreateGameObject("Hook1")
local Hook2 = CreateGameObject("Hook2")
Hook1.transform.parent = user.transform
Hook2.transform.parent = user.transform

lineRenderer1 = AddLineRenderer(Hook1)
lineRenderer2 = AddLineRenderer(Hook2)

lineRenderer1.startWidth = 0.1
lineRenderer1.endWidth = 0.1
lineRenderer2.startWidth = 0.1
lineRenderer2.endWidth = 0.1
lineRenderer1.material = CreateMaterial(Shader.Find("Sprites/Default"))
lineRenderer2.material = CreateMaterial(Shader.Find("Sprites/Default"))
lineRenderer1.startColor = Color.blue
lineRenderer1.endColor = Color.blue
lineRenderer2.startColor = Color.blue
lineRenderer2.endColor = Color.blue
lineRenderer1.enabled = false
lineRenderer2.enabled = false

function Start()
    if RaycastCheck(userCamera.position, userCamera.forward, hookRange, ground) then
        local hit = RaycastValue(userCamera.position, userCamera.forward, hookRange, ground)
        hookPos = hit.point
        isHooked = true
        local startForce = Normalize(hookPos - userObject.position) * hookPower * physicsMultiplier
        if startForce.y < physicsMultiplier * hookPower * 0.5 then
            startForce = startForce + Vector3.up * (physicsMultiplier * hookPower * 0.5 - startForce.y)
        end
        if hookPos.y >= userObject.position.y then
            startForce = startForce + Vector3.up * (hookPos.y - userObject.position.y) * physicsMultiplier
        end
        AddForce(userRigidBody, startForce, "Force")
        lineRenderer1.enabled = true
        lineRenderer1:SetPosition(0, userObject.position + userObject.right * 0.5 + userObject.up * 0.5)
        lineRenderer1:SetPosition(1, hookPos)
        lineRenderer2.enabled = true
        lineRenderer2:SetPosition(0, userObject.position + userObject.right * 0.5 - userObject.up * 0.5)
        lineRenderer2:SetPosition(1, hookPos)
    end
end

function Frame()
    if isHooked then
        lineRenderer1:SetPosition(0, userObject.position + userObject.right * 0.5 + userObject.up * 0.5)
        lineRenderer2:SetPosition(0, userObject.position + userObject.right * 0.5 - userObject.up * 0.5)
    end
end

function Tick()
    if isHooked then
        local direction = Normalize(hookPos - userObject.position)
        local force = direction * hookPower * physicsMultiplier
        AddForce(userRigidBody, force, "Force")
        if Vector3.Angle(userRigidBody.velocity, force) > 90 then
            AddForce(userRigidBody, -userRigidBody.velocity * physicsMultiplier, "Force")
        end
    end
end

function Stop()
    isHooked = false
    lineRenderer1.enabled = false
    lineRenderer2.enabled = false
end