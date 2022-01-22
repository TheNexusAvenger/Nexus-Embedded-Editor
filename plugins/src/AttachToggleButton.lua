--[[
TheNexusAvenger

Toggle button for attaching an external editor.
--]]

local AttachToggleButton = {}



--[[
Creates the toggle button.
--]]
function AttachToggleButton.new(EditorName, Toolbar, Session, Icon)
    --Create the button.
    local Button = Toolbar:CreateButton("Attach "..EditorName, "Attaches "..EditorName.." to the Roblox Studio editor. Connects the editor if not done so already.", Icon)
    Button.ClickableWhenViewportHidden = true

    --Set up connecting the button being active.
    Session.AttachedChanged:Connect(function()
        Button:SetActive(Session.Attached)
    end)

    --Set up the button being clicked.
    local DB = true
    Button.Click:Connect(function()
        Button:SetActive(Session.Attached)
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