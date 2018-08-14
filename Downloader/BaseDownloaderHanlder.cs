using HuSe.Interface;
using HuSe.Models;
using System;
using System.IO;
using System.Net;

namespace HuSe.Downloader
{
    public abstract class BaseDownloaderHanlder
    {
        private int index = 0;

        public Action<BaseDownloaderHanlder> Finshed { get; internal set; }

        protected abstract MetaData GetMetaData(int index);

        internal void StartDown(IProcessNotify processNotify, string localFolder)
        {
             var metaData = GetMetaData(index);
             DoDownload(metaData, processNotify, localFolder);
        }

        private void DoDownload(MetaData metaData, IProcessNotify processNotify, string localFolder)
        {
            var fullPath = Path.Combine(localFolder, metaData.LocalFileName);
            metaData.FullPath = fullPath;
            var isExist = CheckIfExist(fullPath, metaData);
            if (isExist)
            {
                TryGoToNext(metaData, processNotify, localFolder);
                return;
            }

            DoWebDown(metaData, processNotify, localFolder, fullPath);
        }

        private void TryGoToNext(MetaData metaData, IProcessNotify processNotify, string localFolder)
        {
            var isFinshed = StepOver(index, metaData);
            if (isFinshed)
            {
                FinshedNotify(processNotify);
                Finshed?.Invoke(this);
                return;
            }
            else
            {
                TakeNext(processNotify, localFolder);
                return;
            }
        }

        protected void TakeNext(IProcessNotify processNotify, string localFolder)
        {
            index++;
            var metaData = GetMetaData(index);
            DoDownload(metaData, processNotify, localFolder);
        }

        private bool CheckIfExist(string fullPath, MetaData metaData)
        {
            if (!File.Exists(fullPath))
            {
                return false;
            }

            if (metaData.ReplaceExist)
            {
                File.Delete(fullPath);
                return false;
            }
            else
            {
                return true;
            }
        }
        
        private void EndResponse(WebResponse response, MetaData metaData, IProcessNotify processNotify, string loaclPath, string fullPath)
        {
            var stream = response.GetResponseStream();
            var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            var totalLength = response.ContentLength;
            byte[] bytes = new byte[1024 * 512];
            var accpectLegnth = 0;
            Action beginReadAction = null;
            Action<IAsyncResult> endReadAction = (ar) =>
            {
                var readCount = stream.EndRead(ar);
                if (readCount == 0)
                {
                    stream.Dispose();
                    fs.Flush();
                    fs.Dispose();
                    response.Close();
                    TryGoToNext(metaData, processNotify, loaclPath);
                }
                else
                {
                    accpectLegnth += readCount;
                    processNotify?.Progress(metaData, accpectLegnth, totalLength, (int)(accpectLegnth / totalLength));
                    fs.Write(bytes, 0, readCount);
                    fs.Flush();
                    beginReadAction.Invoke();
                }
            };

            beginReadAction = () =>
            {
                stream.BeginRead(bytes, 0, bytes.Length, (ar) => {
                    endReadAction.Invoke(ar);
                }, null);
            };

            beginReadAction.Invoke();
        }

        private void DoWebDown(MetaData metaData, IProcessNotify processNotify, string loaclPath, string fullPath)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(metaData.Url);
                request.BeginGetResponse(ar=> {
                   var response = request.EndGetResponse(ar);
                   EndResponse(response, metaData, processNotify, loaclPath, fullPath);
                }, null);
            }
            catch(Exception ex)
            {
                try
                {
                    processNotify.Error(ex, metaData);
                }
                finally
                {
                }
            }
        }

        protected abstract void FinshedNotify(IProcessNotify processNotify);

        protected abstract bool StepOver(int index, MetaData metaData);
    }
}
