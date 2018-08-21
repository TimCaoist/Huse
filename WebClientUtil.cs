using HuSe.Downloader;
using HuSe.Interface;
using HuSe.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace HuSe
{
    public static class WebClientUtil
    {
        private static readonly ICollection<BaseDownloaderHanlder> Proccers = new List<BaseDownloaderHanlder>();

        private static readonly List<MetaData> datas = new List<MetaData>();

        private static IHuSeConfig Config;

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private static Thread CustomerThread = new Thread(Cusomer);


        static WebClientUtil()
        {
            ResetConfig(new DefaultHuSeConfig());
            CustomerThread.Start();
        }

        public static bool CheckUrlExist(string url)
        {
            var path = GetDefaultPath(url);
            return File.Exists(path);
        }

        public static string GetDefaultPath(string url)
        {
            var path = Path.Combine(Config.LocalFolder, Config.GetDefaultFile(url));
            return path;
        }

        private static void Cusomer()
        {
            while (true)
            {
                if (Proccers.Count() < Config.MaxProccer)
                {
                    WebClientIdle();
                }
                
                autoResetEvent.WaitOne();
            }
        }

        public static void ResetConfig(IHuSeConfig config)
        {
            if (config == null)
            {
                throw new ArgumentException("配置不能为空！");
            }

            if (config.MaxProccer < 1)
            {
                throw new ArgumentException("最大任务处理个数不能小于1！");
            }

            Config = config;
            if (!Directory.Exists(Config.LocalFolder))
            {
                Directory.CreateDirectory(Config.LocalFolder);
            }
        }

        public static void DownloadFile(string url, string localName, long batchId, IProcessNotify processNotify = null, bool existReplace = false)
        {
            var metaData = new MetaData
            {
                BatchId = batchId,
                Url = url,
                LocalFileName = localName
            };

            DownloadFile(metaData, processNotify, existReplace);
        }

        public static void DownloadFile(MetaData metaData, IProcessNotify processNotify = null, bool existReplace = false)
        {
            if (string.IsNullOrEmpty(metaData.LocalFileName))
            {
                metaData.LocalFileName = Config.GetDefaultFile(metaData.Url);
            }

            metaData.ProcessNotify = processNotify;
            metaData.ReplaceExist = existReplace;
            datas.Add(metaData);
            autoResetEvent.Set();
        }

        private static void Notify()
        {
            autoResetEvent.Set();
        }

        private static void WebClientIdle()
        {
            MetaData meta = datas.FirstOrDefault();
            if (meta == null)
            {
                return;
            }

            MetaData[] batchDatas;
            if (meta.BatchId == 0)
            {
                batchDatas = new MetaData[] { meta };
                datas.Remove(meta);
            }
            else {
                var c = datas.Count();
                batchDatas = datas.Where(d => d.BatchId == meta.BatchId).ToArray();
                datas.RemoveAll(d => d.BatchId == meta.BatchId);
            }

            var baseHanlder = DownloadHanlderFactory.Create(meta.BatchId, batchDatas);
            Proccers.Add(baseHanlder);
            baseHanlder.Finshed = (hanlder) =>
            {
                Proccers.Remove(hanlder);
                autoResetEvent.Set();
            };

            baseHanlder.StartDown(meta.ProcessNotify, Config.LocalFolder);
        }

        public static void DownloadFile(string url, long id, IProcessNotify processNotify)
        {
            DownloadFile(url, string.Empty, id, processNotify, Config.ReplaceExist);
        }
        
        public static void DownloadFile(string url, long id = 0)
        {
            DownloadFile(url, string.Empty, id, null, Config.ReplaceExist);
        }

        public static void DownloadFiles(IEnumerable<string> urls, IProcessNotify processNotify = null, long batchId = 0)
        {
            var metaDatas = urls.Select(u => new MetaData
            {
                Url = u
            }).ToArray();

            DownloadFiles(metaDatas, batchId, processNotify, Config.ReplaceExist);
        }

        public static void DownloadFiles(IEnumerable<MetaData> metaDatas, long batchId = 0, IProcessNotify processNotify = null, bool existReplace = false)
        {
            foreach (var metaData in metaDatas)
            {
                if (string.IsNullOrEmpty(metaData.LocalFileName))
                {
                    metaData.LocalFileName = Config.GetDefaultFile(metaData.Url);
                }

                metaData.BatchId = batchId;
                metaData.ProcessNotify = processNotify;
                metaData.ReplaceExist = existReplace;
                datas.Add(metaData);
            }

            autoResetEvent.Set();
        }
    }
}
