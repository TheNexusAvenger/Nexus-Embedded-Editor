/*
 * TheNexusAvenger
 *
 * Performs operation on Roblox Studio.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using NexusEmbeddedEditor.Project;
using TreeScope = System.Windows.Automation.TreeScope;
using WindowVisualState = System.Windows.Automation.WindowVisualState;

namespace NexusEmbeddedEditor.Window
{

#if DEBUG
    /*
     * An AutomationElement wrapper meant for debugging the Automation hierarchy.
     * This should NOT be used in production.
     * 
     * Usage:
        var debug = new DebugElement(this.Window.Window);
        
        Console.WriteLine("Debugging...");  // set breakpoint here

     * There should now be a tree in the Locals widget containing every element.
     */
    public class DebugElement
    {
        public AutomationElement Element;

        public DebugElement[] Children;
        public string Name;
        public string AutomationId;
        public string ClassName;
        public ControlType ControlType;
        public bool HasKeyboardFocus;

        public DebugElement(AutomationElement element)
        {
            this.Element = element;

            this.Name = element.Current.Name;
            this.AutomationId = element.Current.AutomationId;
            this.ClassName = element.Current.ClassName;
            this.ControlType = element.Current.ControlType;
            this.HasKeyboardFocus = element.Current.HasKeyboardFocus;

            List<DebugElement> debugElements = new List<DebugElement>();
            foreach (AutomationElement child in this.GetRawChildren())
            {
                debugElements.Add(new DebugElement(child));
            }
            this.Children = debugElements.ToArray();
        }

        private AutomationElementCollection GetRawChildren()
        {
            return this.Element.FindAll(
                TreeScope.Children,
                Condition.TrueCondition
            );
        }
    }
#endif

    public class RobloxStudioWindow
    {
        private static Condition MainViewCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "RibbonMainWindow.mainViewStackedWidget");
        private static Condition QStackedWidgetCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "QStackedWidget");
        private static Condition QWidgetCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "QWidget");
        private static Condition DocDockWidgetCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "DocDockWidget");

        public BaseWindow Window;
        private ProjectStructure Structure;
        private AutomationElement EditorWindow;
        private WindowPattern FocusPattern;
        private bool Active = true;
        private EditorWindow ExternalEditor;
        private string ScriptName;
        
        /*
         * Creates a Roblox Studio Window object.
         */
        private RobloxStudioWindow(BaseWindow window,ProjectStructure structure)
        {
            this.Window = window;
            this.Structure = structure;
            
            // Create the pattern.
            this.FocusPattern = (WindowPattern) this.Window.Window.GetCurrentPattern(WindowPattern.Pattern);
        }

        private AutomationElement GetDetachedEditorWindow()
        {
            AutomationElement detachedEditorWindow = null;

            if (this.ScriptName != null) {
                string automationId = $"RibbonMainWindow.{this.ScriptName}";

                // Get every detached script editor window.
                // They each have the ClassName "DocDockWidget".
                AutomationElementCollection subElements = this.Window.Window
                    .FindAll(TreeScope.Children, RobloxStudioWindow.DocDockWidgetCondition);

                // Get the child window with the targeted automation id if it exists.
                AutomationElement childWindow = null;
                foreach (AutomationElement element in subElements)
                {
                    try
                    {
                        if (element.Current.AutomationId.StartsWith(automationId))
                        {
                            childWindow = element;
                            break;
                        }
                    }
                    catch (ElementNotAvailableException) { }
                }

                if (childWindow != null)
                {
                    try
                    {
                        detachedEditorWindow = childWindow
                            .FindFirst(TreeScope.Children, RobloxStudioWindow.QWidgetCondition)
                            .FindFirst(TreeScope.Children, Condition.TrueCondition);
                    }
                    catch (NullReferenceException) { }
                }
            }

            return detachedEditorWindow;
        }

        private AutomationElement GetMainViewEditorWindow()
        {
            try
            {
                // Whenever Condition.TrueCondition is used, it's assumed there is only one child.
                return this.Window.Window
                    .FindFirst(TreeScope.Children, RobloxStudioWindow.MainViewCondition)
                    .FindFirst(TreeScope.Children, Condition.TrueCondition)
                    .FindFirst(TreeScope.Children, RobloxStudioWindow.QStackedWidgetCondition)
                    .FindFirst(TreeScope.Children, Condition.TrueCondition)
                    .FindFirst(TreeScope.Children, Condition.TrueCondition);
            } 
            catch (NullReferenceException)
            {
                return null;
            }
            
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
                    return !this.EditorWindow.Current.BoundingRectangle.IsEmpty;
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

            AutomationElement newEditorWindow = null;

            if (this.ScriptName != null)
            {
                // Check for a detached window first.
                // If a detached window was not found, assume it's in the main window.
                newEditorWindow = this.GetDetachedEditorWindow() ?? this.GetMainViewEditorWindow();
            }

            this.EditorWindow = newEditorWindow;
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
                var lastAttached = false;
                
                new Thread(() =>
                {
                    while (this.ExternalEditor == externalEditor && externalEditor.Active && this.Active)
                    {
                        try
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
                                            var editorBoundingSize = editorWindow.Current.BoundingRectangle;
                                            if ((selectedProcessId == externalEditorProcessId && selectedControlType != "ControlType.Pane") || (selectedProcessId == robloxProcessId && selectedControlType == "ControlType.Edit") && currentFocusedElement.BoundingRectangle.Height > 80)
                                            {
                                                if (editorBoundingSize != lastBoundingSize || selectedProcessId != lastProcessId || selectedControlType != lastControlType || lastAttached != externalEditor.Attached)
                                                {
                                                    // Move the window.
                                                    externalEditor.Move(editorBoundingSize);

                                                    // Set the last information.
                                                    lastBoundingSize = editorBoundingSize;
                                                    lastProcessId = selectedProcessId;
                                                    lastControlType = selectedControlType;
                                                    lastAttached = externalEditor.Attached;
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
                        } catch (ElementNotAvailableException)
                        {
                            // Set the window as inactive and close the editor (Thrown if Roblox Studio is closed).
                            this.Active = false;
                            externalEditor.Close();
                        }
                        
                        // Sleep the thread for a bit.
                        Thread.Sleep(50);
                    }
                }).Start();
            }
        }
        
        /*
         * Opens a script.
         * Returns if it is a temporary file.
         */
        public bool OpenScript(string scriptPath,string existingSource)
        {
            // Get the file location.
            string[] strings = scriptPath.Split('.');
            this.ScriptName = strings[strings.Length - 1];
            var fileLocation = this.Structure.GetFileLocation(scriptPath);
            var isTemporary = false;
            if (fileLocation == null)
            {
                isTemporary = true;
                fileLocation = this.Structure.TemporaryFiles.GetFileLocation(scriptPath);
                File.WriteAllText(fileLocation,existingSource);
            }
            
            // Open the file if the editor is still defined.
            if (this.ExternalEditor != null)
            {
                this.GetEditorWindow(true);
                this.ExternalEditor.OpenFile(fileLocation);
            }
            
            // Return if it was temporary.
            return isTemporary;
        }
        
        /*
         * Reads the source of a script.
         */
        public string ReadScript(string scriptPath)
        {
            return File.ReadAllText(this.Structure.TemporaryFiles.GetFileLocation(scriptPath));
        }
    }
}