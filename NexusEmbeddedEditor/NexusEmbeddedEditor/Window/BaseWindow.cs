/*
 * TheNexusAvenger
 *
 * Wraps windows for automation.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using static System.Windows.Automation.AutomationElement;

namespace NexusEmbeddedEditor.Window
{
    public class BaseWindow
    {
        // The default timeout for getting windows.
        public const int DEFAULT_TIMEOUT_SECONDS = 3;
        
        private static List<AutomationElement> WindowsInUse = new List<AutomationElement>();
        public AutomationElement Window;
        
        /*
         * Creates a window.
         */
        private BaseWindow(AutomationElement window)
        {
            this.Window = window;
        }
        
        /*
         * Fetches a window.
         */
        public static BaseWindow GetWindow(string windowName,int timeout = DEFAULT_TIMEOUT_SECONDS)
        {
            // Get the window depending on if a search is being done.
            var searchTime = new Stopwatch();
            searchTime.Start();
            while (searchTime.Elapsed.Seconds < timeout)
            {
                // Iterate through the windows and find the matches.
                var elementCollectionControl = RootElement.FindAll(TreeScope.Children,Automation.ControlViewCondition);
                foreach (AutomationElement autoElement in elementCollectionControl)
                {
                    var searchString = autoElement.GetCurrentPropertyValue(NameProperty).ToString();
                    if ((bool) autoElement.GetCurrentPropertyValue(IsEnabledProperty) && !WindowsInUse.Contains(autoElement) && searchString.IndexOf(windowName,StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        WindowsInUse.Append(autoElement);
                        return new BaseWindow(autoElement);
                    }
                }
            }
            searchTime.Stop();
            
            // Throw an exception if no windows were found.
            throw new ElementNotAvailableException("Could not find unused window with title: " + windowName);
        }
        
        /*
         * Releases the window.
         */
        public void Release()
        {
            WindowsInUse.Remove(this.Window);
        }
    }
}