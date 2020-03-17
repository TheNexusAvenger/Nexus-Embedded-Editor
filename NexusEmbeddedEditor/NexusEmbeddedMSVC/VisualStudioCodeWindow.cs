/*
 * TheNexusAvenger
 *
 * Window control for Microsoft Visual Studio Code
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Automation;
using NexusEmbeddedEditor.Project;
using NexusEmbeddedEditor.Window;

namespace NexusEmbeddedMSVC
{
    public class VisualStudioCodeWindow : EditorWindow
    {
        protected Process MainProcess;
        
        /*
         * Creates a Visual Studio Code Window object.
         */
        public VisualStudioCodeWindow(BaseWindow editorWindow) : base(editorWindow)
        {
            
        }
        
        /*
         * Returns the executable location for Microsoft Visual Studio Code.
         */
        public static string GetExecutableLocation()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"),"Programs","Microsoft VS Code","Code.exe");
        }
        
        /*
         * Opens a new Visual Studio Code window.
         */
        public static VisualStudioCodeWindow OpenWindow(ProjectStructure structure)
        {
            // Create and start the process.
            var process = new Process();
            process.StartInfo.FileName = GetExecutableLocation();
            process.StartInfo.Arguments = structure.GetProjectDirectory().Replace(" ","\\ ") + " -n";;
            process.Start();

            // Create the window.
            var directoryName = Path.GetFileName(structure.GetProjectDirectory());
            var window = new VisualStudioCodeWindow(BaseWindow.GetWindow(directoryName + " - Visual Studio Code"));
            window.MainProcess = process;
            
            // Connect closing the window.
            new Thread(() =>
            {
                process.WaitForExit();
                window.Attached = false;
                window.Active = false;
            }).Start();

            // Return the new window.
            return window;
        }
        
        /*
         * Closes the editor.
         */
        public override void Close()
        {
            base.Close();
            
            // Stop the editor process.
            if (this.MainProcess != null && !this.MainProcess.HasExited)
            {
                this.MainProcess.Kill();
            }
        }

        /*
         * Opens a file at a given location.
         */
        public override void OpenFile(string fileLocation)
        {
            // Create and start the process.
            var process = new Process();
            process.StartInfo.FileName = GetExecutableLocation();
            process.StartInfo.Arguments = fileLocation.Replace(" ","\\ ");
            process.Start();
            
            // Focus the window after opening.
            process.WaitForExit();
            this.FocusPattern.SetWindowVisualState(WindowVisualState.Normal);
        }
    }
}