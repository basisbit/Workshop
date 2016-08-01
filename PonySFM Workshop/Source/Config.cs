﻿using System;
using System.IO;

namespace PonySFM_Workshop
{
    public class Config
    {
        static string ConfigLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PonySFM";
        static string ConfigFileLocation = ConfigLocation + "\\config.xml";

        static bool CheckConfigExists()
        {
            return Directory.Exists(ConfigLocation) && File.Exists(ConfigFileLocation);
        }
    }
}