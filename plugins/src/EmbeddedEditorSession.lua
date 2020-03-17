--[[
TheNexusAvenger

Session for Nexus Embedded Editor.
--]]

local NexusPluginFramework = require(script.Parent.Parent:WaitForChild("NexusPluginFramework"))
local NexusInstance = NexusPluginFramework:GetResource("NexusInstance.NexusInstance")

local EmbeddedEditorSession = NexusInstance:Extend()
EmbeddedEditorSession:SetClassName("EmbeddedEditorSession")

local HttpService = game:GetService("HttpService")



--[[
Creates an Embedded Editor Session object.
--]]
function EmbeddedEditorSession:__new(Port)
    self:InitializeSuper()
    
    self.SessionId = HttpService:GenerateGUID()
    self.Connected = false
    self.Attached = false
    self.DB = true
    self.Port = Port
end

--[[
Updates the state based on the current status.
Returns if there was an error for calls where this is called first.
--]]
function EmbeddedEditorSession:UpdateState(SurpressWarnings)
    --Send the session request.
    local Worked,Return = pcall(function()
        return HttpService:GetAsync("http://localhost:"..tostring(self.Port).."/session?session="..self.SessionId)
    end)

    --Update the state if the request was successful and got a response.
    if Worked then
        self.Connected = true
        self.Attached = HttpService:JSONDecode(Return).attached
        return false
    end

    --Output a warning message and return if the error was connection related.
    self.Connected = false
    self.Attached = false
    if Return == "Http requests are not enabled. Enable via game settings" then
        if not SurpressWarnings then
            warn("The HttpService is disabled. Nexus Embedded Editor requires it to be enabled to communicate with the editor server.")
        end
        return true
    elseif Return == "HttpError: ConnectFail" then
        if not SurpressWarnings then
            warn("Nexus Embedded Editor is unreachable. Is it active?")
        end
        return true
    elseif Return == "HTTP 400 (Bad Request)" then
        return false
    else
        if not SurpressWarnings then
            warn("Error occured getting status: "..tostring(Return))
        end
        return true
    end
end

--[[
Connects the editor.
--]]
function EmbeddedEditorSession:ConnectEditor()
    if not self.DB then return end
    self.DB = false

    --Update the current state in case something changed.
    if self:UpdateState() then self.DB = true return end

    if not self.Connected then
        --Create the script to connect.
        local ConnectScript = Instance.new("Script")
        ConnectScript.Name = self.SessionId
        ConnectScript.Source = "Nexus Embedded Editor is attemtping detection.\nDo not close or unfocus this script."
        ConnectScript.Archivable = false
        ConnectScript.Parent = game:GetService("ServerScriptService")
        NexusPluginFramework:GetPlugin():OpenScript(ConnectScript)

        --Send the connect request.
        local Worked,Return = pcall(function()
            return HttpService:PostAsync("http://localhost:"..tostring(self.Port).."/connect?session="..self.SessionId,"")
        end)

        --Warn that the detection failed.
        if not Worked then
            warn("Nexus Embedded Editor server failed to detect. Did you unfocus the script?")
        end

        --Destroy the connect script.
        ConnectScript:Destroy()
    end

    --Update the state.
    self:UpdateState()
    self.DB = true
end

--[[
Disconnects the editor.
--]]
function EmbeddedEditorSession:DisconnectEditor()
    if not self.DB then return end
    self.DB = false

    --Update the current state in case something changed.
    if self:UpdateState() then self.DB = true return end

    if self.Connected then
        --Send the disconnect request.
        local Worked,Return = pcall(function()
            return HttpService:PostAsync("http://localhost:"..tostring(self.Port).."/disconnect?session="..self.SessionId,"")
        end)

        --Warn that the disconnect failed.
        if not Worked then
            warn("Nexus Embedded Editor server failed to disconnect because "..tostring(Return))
        end
    end

    --Update the state.
    self:UpdateState()
    self.DB = true
end

--[[
Attaches the editor.
--]]
function EmbeddedEditorSession:AttachEditor()
    if not self.DB then return end
    self.DB = false

    --Update the current state in case something changed.
    if self:UpdateState() then self.DB = true return end

    if not self.Attached then
        --Send the attach request.
        local Worked,Return = pcall(function()
            return HttpService:PostAsync("http://localhost:"..tostring(self.Port).."/attach?session="..self.SessionId,"")
        end)

        --Warn that the attach failed.
        if not Worked then
            warn("Nexus Embedded Editor server failed to attach because "..tostring(Return))
        end
    end

    --Update the state.
    self:UpdateState()
    self.DB = true
end

--[[
Detaches the editor.
--]]
function EmbeddedEditorSession:DetachEditor()
    if not self.DB then return end
    self.DB = false

    --Update the current state in case something changed.
    if self:UpdateState() then self.DB = true return end

    if self.Attached then
        --Send the detach request.
        local Worked,Return = pcall(function()
            return HttpService:PostAsync("http://localhost:"..tostring(self.Port).."/detach?session="..self.SessionId,"")
        end)

        --Warn that the detach failed.
        if not Worked then
            warn("Nexus Embedded Editor server failed to detach because "..tostring(Return))
        end
    end

    --Update the state.
    self:UpdateState()
    self.DB = true
end

--[[
Starts updating the state continously in the background.
--]]
function EmbeddedEditorSession:StartContinousUpdates()
    spawn(function()
        while true do
            self:UpdateState(true)
            wait(1)
        end
    end)
end


return EmbeddedEditorSession