﻿using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CoreLib;
using CoreLib.Interface;

namespace CoreLib.Impl
{
    public class PonySFMAPIConnector : IAPIConnector
    {
        string _baseUrl = "https://ponysfm.com";
        static PonySFMAPIConnector singleton;

        public static PonySFMAPIConnector Instance
        {
            get
            {
                if (singleton == null)
                    singleton = new PonySFMAPIConnector();
                return singleton;
            }
        }

        private PonySFMAPIConnector()
        {
        }

        public async Task DownloadRevisionAdditionalInformation(Revision revision)
        {
            int id = revision.ID;
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var webClient = new CookedWebClient();
            webClient.Headers.Add("user-agent", "PSFM_ModManager-" + ModManager.Version);

            var ret = await webClient.DownloadStringTaskAsync(new Uri(string.Format("{0}/api/revision.json?id={1}", _baseUrl, id)));
            var revisionAPIObject = JsonConvert.DeserializeObject<RevisionAPIObject>(ret);

            ret = await webClient.DownloadStringTaskAsync(new Uri(string.Format("{0}/api/resource.json?id={1}", _baseUrl, revisionAPIObject.resource_id)));
            var resourceAPIObject = JsonConvert.DeserializeObject<ResourceAPIObject>(ret, settings);

            revision.AdditionalData["ResourceName"] = resourceAPIObject.name;

            if(resourceAPIObject.user_id != 0)
            {
                ret = await webClient.DownloadStringTaskAsync(new Uri(string.Format("{0}/api/user.json?id={1}", _baseUrl, resourceAPIObject.user_id)));
                var userAPIObject = JsonConvert.DeserializeObject<UserAPIObject>(ret);
                revision.AdditionalData["UserName"] = userAPIObject.name;
            }
            else
            {
                revision.AdditionalData["UserName"] = "None";
            }
        }

        public async Task DownloadRevisionZIP(int id, string filepath, IProgress<int> progress)
        {
            var webClient = new CookedWebClient();
            webClient.Headers.Add("user-agent", "PSFM_ModManager-" + ModManager.Version);

            webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
            {
                progress.Report(e.ProgressPercentage);
            };

            await webClient.DownloadFileTaskAsync(new Uri(string.Format("{0}/rev/{1}/internal_download_redirect", _baseUrl, id)),
                filepath);
        }

        public string GetRevisionURL(Revision revision)
        {
            return string.Format("{0}/rev/{1}", _baseUrl, revision.ID);
        }

        public int FetchCurrentVersion()
        {
            throw new NotImplementedException();
        }
    }
}