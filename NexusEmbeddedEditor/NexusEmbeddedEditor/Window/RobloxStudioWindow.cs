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
    public class RobloxStudioWindow
    {
        public BaseWindow Window;
        private ProjectStructure Structure;
        private AutomationElement EditorWindow;
        private Condition ExplorerWidgetCondition;
        private Condition DirectChildCondition;
        private WindowPattern FocusPattern;
        public AutomationElement EditorParent;
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
            
            // Create the conditions.
            this.ExplorerWidgetCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "RibbonMainWindow.objectExplorer");
            this.DirectChildCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "");

            AutomationElement mainView = this.Window.Window.FindFirst(
                TreeScope.Descendants, 
                new PropertyCondition(AutomationElement.AutomationIdProperty, "RibbonMainWindow.mainViewStackedWidget")
            );

            AutomationElementCollection allViews = mainView.FindFirst(TreeScope.Children, this.DirectChildCondition)
                .FindAll(TreeScope.Children, Condition.TrueCondition);

            // Get the parents.
            this.EditorParent = this.Window.Window
                .FindFirst(TreeScope.Children, this.ExplorerWidgetCondition)
                .FindFirst(TreeScope.Children, this.DirectChildCondition);
        }

        private List<AutomationElement> GetPath(AutomationElement ancestor, AutomationElement element)
        {
            TreeWalker finder = new TreeWalker(Condition.TrueCondition);
            List<AutomationElement> path = new List<AutomationElement>();
            AutomationElement currentElement = element;
            while (currentElement != ancestor && currentElement != null)
            {
                path.Add(currentElement);
                currentElement = finder.GetParent(currentElement);
            }
            path.Add(ancestor);
            return path;
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

            AutomationElement newEditor = null;

            if (this.ScriptName != null)
            {
                string automationId = $"RibbonMainWindow.{this.ScriptName}";

                AutomationElementCollection editors = this.Window.Window.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "StudioScriptEditor")
                );
                foreach (AutomationElement editor in editors)
                {
                    var path = this.GetPath(this.Window.Window, editor);
                    var window = path[path.Count - 2];
                    if (window.Current.AutomationId.StartsWith(automationId))
                    {
                        newEditor = editor;
                        break;
                    }
                }

                if (newEditor == null)
                {
                    var mainWindow = this.Window.Window.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.AutomationIdProperty, "RibbonMainWindow.mainViewStackedWidget")
                    );
                    newEditor = mainWindow.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ClassNameProperty, "StudioScriptEditor")
                    );
                }
            }

            this.EditorWindow = newEditor;
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