--[[
TheNexusAvenger

Toggle button for connecting an external editor.
--]]

local ConnectToggleButton = {}



--[[
Creates the toggle button.
--]]
function ConnectToggleButton.new(EditorName, Toolbar, Session, Icon)
    --Create the button.
    local Button = Toolbar:CreateButton("Connect "..EditorName, "Connects "..EditorName.." without attaching it to the Roblox Studio editor.", Icon)
    Button.ClickableWhenViewportHidden = true

    --Set up connecting the button being active.
    Session.ConnectedChanged:Connect(function()
        Button:SetActive(Session.Connected)
    end)

    --Set up the button being clicked.
    local DB = true
    Button.Click:Connect(function()
        Button:SetActive(Session.Attached)
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