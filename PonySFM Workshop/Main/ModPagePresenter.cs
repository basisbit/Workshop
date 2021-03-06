﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLib;
using PonySFM_Workshop.Base;

namespace PonySFM_Workshop.Main
{
    class ModPagePresenter : BasePresenter
    {
        public Revision Revision { get; set; }
        public string ResourceName => Revision.GetMetadataValue("ResourceName");
        public string UserName => Revision.GetMetadataValue("UserName");
        public string InstallationTime => Revision.GetMetadataValue("InstallationTime");
        public string Size => FileUtil.GetHumanReadableFileSize(Convert.ToInt64(Revision.GetMetadataValue("Size")));

        public string FileList
        {
            get
            {
                return Revision.Files.Aggregate("", (current, file) => current + (file.Path + "\n"));
            }
        }

        public ModPagePresenter(Revision revision)
        {
            Revision = revision;
        }
    }
}
