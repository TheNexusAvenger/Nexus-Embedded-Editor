/*
 * TheNexusAvenger
 *
 * Performs operation on Roblox Studio.
 */

using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using NexusEmbeddedEditor.Project;
using TreeScope = System.Windows.Automation.TreeScope;
using WindowVisualState = System.Windows.Automation.WindowVisualState;

namespace NexusEmbeddedEditor.Window
{
    public class RobloxStudioWindow
    {
        public BaseWindow Window;
        private ProjectStructure Structure;
        private AutomationElement EditorWindow;
        private Condition PaneCondition;
        private Condition EditorCondition;
        private Condition TabCondition;
        private WindowPattern FocusPattern;
        private AutomationElement EditorParent;
        private AutomationElement TabsParent;
        private bool Active = true;
        private EditorWindow ExternalEditor;
        
        /*
         * Creates a Roblox Studio Window object.
         */
        private RobloxStudioWindow(BaseWindow window,ProjectStructure structure)
        {
            this.Window = window;
            this.Structure = structure;
            
            // Create the pattern.
            this.FocusPattern = (WindowPattern) this.Window.Window.GetCurrentPattern(WindowPattern.Pattern);
            
            // Create the conditions.
            this.PaneCondition = new AndCondition(new List<Condition>
            {
                new PropertyCondition(AutomationElement.ClassNameProperty, "Qt5QWindowIcon"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane)
            }.ToArray());
            this.EditorCondition = new AndCondition(new List<Condition>
            {
                new PropertyCondition(AutomationElement.ClassNameProperty, "Qt5QWindowIcon"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
            }.ToArray());
            this.TabCondition = new AndCondition(new List<Condition>
            {
                new PropertyCondition(AutomationElement.ClassNameProperty, "Qt5QWindowIcon"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab)
            }.ToArray());
            
            // Get the parents.
            this.EditorParent = this.Window.Window.FindFirst(TreeScope.Children,this.PaneCondition).FindFirst(TreeScope.Children,this.PaneCondition).FindAll(TreeScope.Children,this.PaneCondition)[1];
            this.TabsParent = this.Window.Window.FindFirst(TreeScope.Children,this.PaneCondition).FindFirst(TreeScope.Children,this.PaneCondition);
        }
        
        /*
         * Fetches a Roblox Studio window.
         */
        public static RobloxStudioWindow GetWindow(string name,ProjectStructure structure)
        {
            return new RobloxStudioWindow(BaseWindow.GetWindow(name),structure);
        }
        
        /*
         * Returns if the editor window is available.
         */
        public bool IsEditorWindowAvailable()
        {
            // Return if the name can be fetched.
            if (this.EditorWindow != null)
            {
                try
                {
                    var name = this.EditorWindow.Current.Name;
                    return true;
                }
                catch (ElementNotAvailableException)
                {
                    this.EditorWindow = null;
                }
            }
            
            // Return false (error or not available).
            return false;
        }
        
        /*
         * Returns the editor window.
         */
        public AutomationElement GetEditorWindow(bool forceUpdate = false)
        {
            // Return the existing window if one exists.
            if (this.IsEditorWindowAvailable() && (this.EditorWindow.Current.HasKeyboardFocus || !forceUpdate))
            {
                return this.EditorWindow;
            }
            
            // Search selected for the editor.
            foreach (AutomationElement editWindow in this.EditorParent.FindAll(TreeScope.Children,this.EditorCondition))
            {
                if (editWindow.Current.HasKeyboardFocus) {
                    this.EditorWindow = editWindow;
                    break;
                }
            }
            
            // Return the editor.
            return this.EditorWindow;
        }
        
        /*
         * Returns if Roblox is minimized.
         */
        public bool IsMinimized()
        {
            return this.FocusPattern.Current.WindowVisualState == WindowVisualState.Minimized;
        }
        
        /*
         * Sets the external editor window.
         */
        public void SetExternalEditor(EditorWindow externalEditor)
        {
            this.ExternalEditor = externalEditor;
            

            // Create the update loop.
            if (externalEditor != null)
            {
                // Get the process ids.
                var robloxProcessId = this.Window.Window.Current.ProcessId;
                var externalEditorProcessId = externalEditor.Window.Window.Current.ProcessId;
                var lastBoundingSize = Rect.Empty;
                var lastProcessId = 0;
                var lastControlType = "";
                
                new Thread(() =>
                {
                    while (this.ExternalEditor == externalEditor && externalEditor.Active)
                    {
                        if (this.IsMinimized())
                        {
                            // Minimize the editor if Roblox Studio is minimized.
                            externalEditor.Minimize();
                        }
                        else
                        {
                            try
                            {
                                // Move the window;
                                var focusedElement = AutomationElement.FocusedElement;
                                if (focusedElement != null)
                                {
                                    var currentFocusedElement = focusedElement.Current;
                                    var selectedProcessId = currentFocusedElement.ProcessId;
                                    var selectedControlType = currentFocusedElement.ControlType.ProgrammaticName;
                                    var editorWindow = this.GetEditorWindow(selectedProcessId == robloxProcessId && selectedProcessId != lastProcessId);

                                    if (editorWindow != null)
                                    {
                                        // Move the window if it is valid and has changed.
                                        if ((selectedProcessId == externalEditorProcessId && selectedControlType != "ControlType.Pane") || (selectedProcessId == robloxProcessId && selectedControlType == "ControlType.Edit"))
                                        {
                                            var editorBoundingSize = editorWindow.Current.BoundingRectangle;
                                            if (editorBoundingSize != lastBoundingSize || selectedProcessId != lastProcessId || selectedControlType != lastControlType)
                                            {
                                                // Update the opened script if the script editor is focused.
                                                if (editorWindow.Current.HasKeyboardFocus)
                                                {
                                                    this.UpdateOpenScript();
                                                }

                                                // Move the window.
                                                externalEditor.Move(editorBoundingSize);

                                                // Set the last information.
                                                lastBoundingSize = editorBoundingSize;
                                                lastProcessId = selectedProcessId;
                                                lastControlType = selectedControlType;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Minimize the editor if no editor is open.
                                        externalEditor.Minimize();
                                    }
                                }
                            } catch (ElementNotAvailableException) {
                                // Thrown if the focused element is closed while reading (ex: using and then closing another application).
                            }
                        }

                        // Sleep the thread for a bit.
                        Thread.Sleep(50);
                    }
                }).Start();
            }
        }
        
        /*
         * Updates the opened script.
         */
        public void UpdateOpenScript()
        {
            new Thread(() =>
            {
                // Invoke the event if the opened script was changed.
                var editor = this.GetEditorWindow();
                if (editor != null)
                {
                    // Determine the file to open.
                    var currentScriptName = this.TabsParent.FindFirst(TreeScope.Children,this.TabCondition).Current.Name;
                    var scriptSource = ((ValuePattern) this.GetEditorWindow(true).GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
                    var fileLocation = this.Structure.GetFileLocation(currentScriptName,scriptSource);
                    
                    // Open the file if the editor is still defined (automation code has a delay).
                    if (this.ExternalEditor != null && fileLocation != null)
                    {
                        this.ExternalEditor.OpenFile(fileLocation);
                    }
                }
            }).Start();
        }
    }
}