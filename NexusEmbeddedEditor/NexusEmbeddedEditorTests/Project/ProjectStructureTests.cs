/*
 * TheNexusAvenger
 *
 * Tests the ProjectStructure class.
 */

using System.Collections.Generic;
using NexusEmbeddedEditor.Project;
using NUnit.Framework;

namespace NexusEmbeddedEditorTests.Project
{
    [TestFixture]
    public class ProjectStructureTests
    {
        /*
         * Tests the GetFileLocation method in the RobloxFile class.
         */
        [Test]
        public void GetFileLocation()
        {
            // Create the components under testing.
            var CuT = new RobloxFile()
            {
                Name = "Test1.Test2",
                Location = "Test2.lua",
                Files = new List<RobloxFile>()
                {
                    new RobloxFile()
                    {
                        Name = "Test3",
                        Files = new List<RobloxFile>()
                        {
                            new RobloxFile()
                            {
                                Name = "Test4",
                                Location = "Test4.lua",
                            },
                        }
                    },
                    new RobloxFile()
                    {
                        Name = "Test5",
                        Location = "Test5.lua",
                    },
                }
            };
            
            // Test the correct files are returned.
            Assert.AreEqual(CuT.GetFileLocation("Test1"),null);
            Assert.AreEqual(CuT.GetFileLocation("Test2"),null);
            Assert.AreEqual(CuT.GetFileLocation("Test1.Test2"),"Test2.lua");
            Assert.AreEqual(CuT.GetFileLocation("Test1.Test2.Test3"),null);
            Assert.AreEqual(CuT.GetFileLocation("Test1.Test2.Test3.Test4"),"Test4.lua");
            Assert.AreEqual(CuT.GetFileLocation("Test1.Test2.Test4"),null);
            Assert.AreEqual(CuT.GetFileLocation("Test1.Test2.Test5"),"Test5.lua");
        }
    }
}