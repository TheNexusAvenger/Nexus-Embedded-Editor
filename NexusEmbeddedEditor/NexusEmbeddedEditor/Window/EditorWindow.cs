/*
 * TheNexusAvenger
 *
 * Abstract class for an editor window.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;

namespace NexusEmbeddedEditor.Window
{
    public abstract class EditorWindow
    {
        public BaseWindow Window;
        public WindowPattern FocusPattern;
        public TransformPattern SizePattern;
        public bool Attached = false;
        public bool Active = true;
        
        /*
         * Creates the editor window.
         */
        public EditorWindow(BaseWindow editorWindow)
        {
            this.Window = editorWindow;
            
            // Create the patterns.
            this.FocusPattern = (WindowPattern) this.Window.Window.GetCurrentPattern(WindowPattern.Pattern);
            this.SizePattern = (TransformPattern) this.Window.Window.GetCurrentPattern(TransformPattern.Pattern);
        }
        
        /*
         * Returns the location of a file in either a hard-coded path
         * or in the system's PATH.
         */
        public static string GetFileLocation(string fileName,IEnumerable<string> hardCodedDirectories = null)
        {
            // Iterate through the hard coded paths and return if the file exists.
            if (hardCodedDirectories != null)
            {
                foreach (var directoryPath in hardCodedDirectories)
                {
                    var filePath = Path.Combine(directoryPath,fileName);
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
            }
            
            // Iterate through the PATH environment variable paths and return if the file exists.
            foreach (var directoryPath in Environment.GetEnvironmentVariable("PATH").Split(';'))
            {
                var filePath = Path.Combine(directoryPath,fileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
            
            // Return null (not found).
            return null;
        }
        
        /*
         * Moves the editor window to the given rectangle.
         */
        public virtual void Move(Rect editorBoundingSize)
        {
            if (this.Attached && this.Active)
            {
                try
                {
                    this.FocusPattern.SetWindowVisualState(WindowVisualState.Normal);
                    this.SizePattern.Move(editorBoundingSize.X, editorBoundingSize.Y);
                    this.SizePattern.Resize(editorBoundingSize.Width, editorBoundingSize.Height);
                }
                catch (InvalidOperationException)
                {
                    
                }
            }
        }
        
        /*
         * Minimizes the window.
         */
        public virtual void Minimize()
        {
            
            if (this.Attached && this.Active && this.FocusPattern.Current.WindowVisualState != WindowVisualState.Minimized)
            {
                try
                {
                    this.FocusPattern.SetWindowVisualState(WindowVisualState.Minimized);
                }
                catch (InvalidOperationException)
                {
                    
                }
            }
        }
        
        /*
         * Closes the editor.
         */
        public virtual void Close()
        {
            this.Active = false;
            this.Window.Release();
        }
        
        /*
         * Opens a file at a given location.
         */
        public abstract void OpenFile(string fileLocation);
    }
}