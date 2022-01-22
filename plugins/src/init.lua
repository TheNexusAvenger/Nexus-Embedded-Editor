--[[
TheNexusAvenger

Common script for initializing the plugin.
--]]

local EmbeddedEditorSession = require(script:WaitForChild("EmbeddedEditorSession"))
local ConnectToggleButton = require(script:WaitForChild("ConnectToggleButton"))
local AttachToggleButton = require(script:WaitForChild("AttachToggleButton"))



return function(Plugin, Port, EditorName, EditorConnectIcon, EditorAttachIcon)
    --Initialize the session.
    local Session = EmbeddedEditorSession.new(Port, Plugin)
    Session:StartContinousUpdates()

    --Create the toolbar and buttons.
    local NexusEmbeddedEditorToolbar = Plugin:CreateToolbar("Embedded "..EditorName)
    ConnectToggleButton.new(EditorName, NexusEmbeddedEditorToolbar, Session, EditorConnectIcon)
    AttachToggleButton.new(EditorName, NexusEmbeddedEditorToolbar, Session, EditorAttachIcon)

    --Create the plugin actions.
    local DB = true
    Plugin:CreatePluginAction("NexusEmbedded"..EditorName.."_Connect", "Connect "..EditorName, "Connects the external editor "..EditorName.." to Roblox Studio.\nPart of Nexus Embedded "..EditorName..".", EditorConnectIcon).Triggered:Connect(function()
        if DB then
            DB = false
            Session:ConnectEditor()
            wait()
            DB = true
        end
    end)
    Plugin:CreatePluginAction("NexusEmbedded"..EditorName.."_Disconnect", "Disonnect "..EditorName, "Disconnects the external editor "..EditorName.." to Roblox Studio.\nPart of Nexus Embedded "..EditorName..".", EditorConnectIcon).Triggered:Connect(function()
        if DB then
            DB = false
            Session:DisconnectEditor()
            wait()
            DB = true
        end
    end)
    Plugin:CreatePluginAction("NexusEmbedded"..EditorName.."_Attach", "Attach "..EditorName, "Attaches the external editor "..EditorName.." to Roblox Studio.\nPart of Nexus Embedded "..EditorName..".", EditorAttachIcon).Triggered:Connect(function()
        if DB then
            DB = false
            Session:AttachEditor()
            wait()
            DB = true
        end
    end)
    Plugin:CreatePluginAction("NexusEmbedded"..EditorName.."_Detach", "Attach "..EditorName, "Detaches the external editor "..EditorName.." to Roblox Studio.\nPart of Nexus Embedded "..EditorName..".", EditorAttachIcon).Triggered:Connect(function()
        if DB then
            DB = false
            Session:DetachEditor()
            wait()
            DB = true
        end
    end)
end