using HuSe.Interface;
using HuSe.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace HuSe
{
    internal class WebClientEx : IDisposable
    {
        private WebClient webClient;

        internal volatile bool IsBusy;

        private IProcessNotify ProcessNotify { get; set; }

        public Action Idle;
        
        private IEnumerable<MetaData> Datas { get; set; }

        private int Index = 0;

        private string localFolder;

        internal WebClientEx()
        {
            webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var metaData = (MetaData)e.UserState;
            Succeed(metaData);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProcessNotify?.Progress((MetaData)e.UserState, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        internal void Down(MetaData metaData, IProcessNotify notify, string localFolder)
        {
            this.IsBusy = true;
            this.localFolder = localFolder;
            this.ProcessNotify = notify;
            Down(metaData);
        }

        internal void Down(IEnumerable<MetaData> datas, IProcessNotify notify, string localFolder)
        {
            this.IsBusy = true;
            Index = 0;
            this.Datas = datas.ToArray();
            this.ProcessNotify = notify;
            this.localFolder = localFolder;
            Down(Datas.First());
        }

        private void Succeed(MetaData metaData)
        {
            metaData.ProcessNotify = null;
            if (Datas != null)
            {
                if (Index < Datas.Count() - 1)
                {
                    Index++;
                    Down(Datas.ElementAt(Index));
                    return;
                }
                else {
                    try
                    {
                        ProcessNotify?.BatchSucceed(metaData.BatchId, Datas);
                        return;
                    }
                    finally
                    {
                        Datas = null;
                        this.IsBusy = false;
                        Idle?.Invoke();
                    }
                }
            }

            try
            {
                ProcessNotify?.Succeed(metaData);
            }
            finally {
                this.IsBusy = false;
                Idle?.Invoke();
            }
        }

        private void Down(MetaData metaData)
        {
            string fullPath = Path.Combine(localFolder, metaData.LocalFileName);
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }

            metaData.FullPath = fullPath;
            if (File.Exists(fullPath))
            {
                if (metaData.ReplaceExist)
                {
                    File.Delete(fullPath);
                }
                else
                {
                    Succeed(metaData);
                    return;
                }
            }

            try
            {
                webClient.DownloadFileAsync(new Uri(metaData.Url), fullPath, metaData);
            }
            catch
            {
                try
                {
                    ProcessNotify.Error(metaData);
                }
                finally
                {
                    this.IsBusy = false;
                    metaData.ProcessNotify = null;
                    Datas = null;
                    Idle?.Invoke();
                }
            }
        }

        public void Dispose()
        {
            if (IsBusy == false)
            {
                this.webClient?.Dispose();
            }
        }

        ~WebClientEx()
        {
            this.webClient?.Dispose();
        }
    }
}
