--[[
TheNexusAvenger

Toggle button for connecting an external editor.
--]]

local NexusPluginFramework = require(script.Parent.Parent:WaitForChild("NexusPluginFramework"))

local ConnectToggleButton = NexusPluginFramework:GetResource("Plugin.NexusPluginButton"):Extend()
ConnectToggleButton:SetClassName("ConnectToggleButton")



--[[
Creates the toggle button.
--]]
function ConnectToggleButton:__new(EditorName,Toolbar,Session,Icon)
    self:InitializeSuper(Toolbar,"Connect "..EditorName,"Connects "..EditorName.." without attaching it to the Roblox Studio editor.",Icon)
    self.ClickableWhenViewportHidden = true
    
    --Set up connecting the button being active.
    Session:GetPropertyChangedSignal("Connected"):Connect(function()
        self.Active = Session.Connected
    end)

    --Set up the button being clicked.
    local DB = true
	self.Click:Connect(function()
        self:SetActive(Session.Attached)
        
		if DB then
			DB = false
            if Session.Connected then
                Session:DisconnectEditor()
            else
                Session:ConnectEditor()
                Session:UpdateOpenScript()
            end
			
			wait()
			DB = true
		end
	end)
end



return ConnectToggleButton