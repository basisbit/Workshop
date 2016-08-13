﻿using System.Xml;

namespace CoreLib
{
    /* TODO: instead of just accepting one string as param this class should hold a list of attributes with a dictionary */
    // There is nothing wrong with holding a path with a string type.
    public class ConfigFile
    {
        public string SFMDirectoryPath { get; set; }

        public ConfigFile(string sfmDirPath)
        {
            SFMDirectoryPath = sfmDirPath;
        }

        public static ConfigFile FromXML(XmlElement elem)
        {
            return new ConfigFile(elem.InnerText);
        }

        public XmlElement ToXML(XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("SFMDirectoryPath");
            elem.InnerText = SFMDirectoryPath;
            return elem;
        }

        public bool Equals(ConfigFile other)
        {
            return other.SFMDirectoryPath == SFMDirectoryPath;
        }
    }
}