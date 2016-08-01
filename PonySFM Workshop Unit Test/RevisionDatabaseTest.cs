﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace PonySFM_Desktop.Test
{
    [TestClass]
    public class RevisionDatabaseTest
    {
        private static string filepath;
        private static string stubfile;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            filepath = Path.Combine(Path.GetTempPath(), "ponysfmtest.xml");
            stubfile = Path.Combine(Path.GetTempPath(), "stubtest.xml");
        }

        [TestMethod]
        [TestCategory("RevisionDatabase")]
        public void CreateDefaultDB()
        {
            var fs = new MockFileSystem();
            var db = new RevisionDatabase(filepath, fs);
            Assert.IsTrue(fs.FileExists(filepath));

            var doc = fs.OpenXML(filepath);

            Assert.IsTrue(doc.HasChildNodes);
            Assert.IsTrue(doc.FirstChild.Name == "PonySFM");
            Assert.IsTrue(!doc.FirstChild.HasChildNodes);
        }

        [TestMethod]
        [TestCategory("RevisionDatabase")]
        public void PopulateData()
        {
            MockFileSystem fs = new MockFileSystem();
            fs.CreateFile(stubfile);
            RevisionDatabase db = new RevisionDatabase(filepath, fs);
            Assert.IsTrue(fs.FileExists(filepath));

            for (int i = 0; i < 5; i ++)
                db.Revisions.Add(CreateStubRevision(fs));

            db.WriteDBDisk();

            XmlDocument doc = fs.OpenXML(filepath);

            Assert.IsTrue(doc.HasChildNodes);
            Assert.IsTrue(doc.FirstChild.Name == "PonySFM");
            Assert.IsTrue(doc.FirstChild.HasChildNodes);
            Assert.IsTrue(doc.FirstChild.ChildNodes.Count == 5);

            foreach (XmlElement elem in doc.FirstChild.ChildNodes)
            {
                Assert.IsTrue(elem.HasAttribute("ID"));
                Assert.IsTrue(elem.HasAttribute("Test"));
                Assert.AreEqual("Cake", elem.GetAttribute("Test"));
                Assert.IsTrue(elem.HasChildNodes);

                foreach (XmlElement fileElem in elem.ChildNodes)
                {
                    Assert.IsTrue(fileElem.GetAttribute("Location") == stubfile);
                }
            }
        }

        private Revision CreateStubRevision(IFileSystem fs)
        {
            var r = new Random();
            int id = r.Next(1000);
            List<RevisionFileEntry> files = new List<RevisionFileEntry>();
            for(int i = 0; i < 5; i++)
            {
                files.Add(RevisionFileEntry.FromFile(stubfile, fs));
            }

            var rev = new Revision(id, files);
            rev.AdditionalData["Test"] = "Cake";

            return rev;
        }
    }
}