﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreLib.Interface;

namespace CoreLib
{
    public enum InstallationResult
    {
        Cancelled,
        Success
    }

    public class RevisionManager
    {
        ConfigFile _configFile;
        IFileSystem _fs;
        string _path;
        RevisionDatabase _db;
        SFMDirectoryParser _dirParser;

        public delegate void FileExistsHandler(object sender, DirectoryCopierFileExistsEventArgs e);
        public event FileExistsHandler OnFileExists;

        public RevisionDatabase Database => _db;

        public RevisionManager(ConfigFile configFile, IFileSystem fs)
        {
            _configFile = configFile;
            _fs = fs;
            _path = configFile.SFMDirectoryPath;
            _dirParser = new SFMDirectoryParser(_path, fs);
            CreateDataFolder();
            _db = new RevisionDatabase(Path.Combine(_dirParser.InstallationPath, "ponysfm.xml"), _fs);
        }

        public void CreateDataFolder()
        {
            if (!_fs.DirectoryExists(_path))
                _fs.CreateDirectory(_path);

            _dirParser.CreateDirectories();
        }

        public async Task<InstallationResult> InstallRevision(Revision revision, string topDir, IProgress<int> progress, CancellationToken cancel = default(CancellationToken))
        {
            /* Copy files and blahblah */
            var directoryCopier = new DirectoryCopier(_fs, topDir, _dirParser.InstallationPath, true);

            directoryCopier.OnProgress += (s, e) =>
                progress?.Report(e.Progress);

            directoryCopier.OnFileExists += (s, e) =>
                OnFileExists(s, e);

            try
            {
                await directoryCopier.Execute(cancel);
            }
            catch(OperationCanceledException)
            {
                return InstallationResult.Cancelled;
            }

            revision.ChangeTopDirectory(topDir, _dirParser.InstallationPath);
            revision.AdditionalData["InstallationTime"] = DateTime.Now.ToString();
            _db.AddToDB(revision);
            _db.WriteDBDisk();

            return InstallationResult.Success;
        }

        public async Task UninstallRevision(int id, IProgress<int> progress)
        {
            Revision revision = _db.Revisions.Find(r => r.ID == id);

            if (revision == null)
                return;

            int totalCount = revision.Files.Count;
            int i = 0;

            await Task.Factory.StartNew(() =>
            {
                _db.RemoveRevision(id);

                /* FIXME: totally guranteed to be sorted by directory! */
                foreach (var file in revision.Files)
                {
                    i++;

                    if (_fs.FileExists(file.Path))
                        _fs.DeleteFile(file.Path);
                    else if(_fs.DirectoryExists(file.Path))
                        _fs.DeleteDirectory(file.Path);

                    progress?.Report(i / totalCount * 100);
                }

                _db.WriteDBDisk();
            });
        }

        public bool VerifyInstalled(Revision revision, IProgress<int> progress)
        {
            int i = 0;
            foreach (var file in revision.Files)
            {
                if (!_fs.FileExists(file.Path))
                    return false;

                if (_fs.GetChecksum(file.Path) != file.Sha512)
                    return false;

                double p = i / (double)revision.Files.Count * 100;
                progress?.Report((int)p);
                i++;
            }

            return true;
        }

        public bool VerifyInstalled(int id, IProgress<int> progress)
        {
            Revision revision = _db.Revisions.Find(x => x.ID == id);

            if (revision == null)
                return false;

            return VerifyInstalled(revision, progress);
        }

        public bool IsInstalled(int id)
        {
            return _db.Revisions.Find(x => x.ID == id) != null;
        }
    }
}
