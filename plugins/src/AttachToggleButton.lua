--[[
TheNexusAvenger

Toggle button for attaching an external editor.
--]]

local NexusPluginFramework = require(script.Parent.Parent:WaitForChild("NexusPluginFramework"))

local AttachToggleButton = NexusPluginFramework:GetResource("Plugin.NexusPluginButton"):Extend()
AttachToggleButton:SetClassName("AttachToggleButton")



--[[
Creates the toggle button.
--]]
function AttachToggleButton:__new(EditorName,Toolbar,Session,Icon)
    self:InitializeSuper(Toolbar,"Attach "..EditorName,"Attaches "..EditorName.." to the Roblox Studio editor. Connects the editor if not done so already.",Icon)
    self.ClickableWhenViewportHidden = true
    
    --Set up connecting the button being active.
    Session:GetPropertyChangedSignal("Attached"):Connect(function()
        self.Active = Session.Attached
    end)

    --Set up the button being clicked.
    local DB = true
    self.Click:Connect(function()
        self.Active = Session.Attached
        
		if DB then
			DB = false
            if Session.Attached then
                Session:DetachEditor()
            else
                Session:ConnectEditor()
                Session:AttachEditor()
                Session:UpdateOpenScript()
            end
			
			wait()
			DB = true
		end
	end)
end



return AttachToggleButton