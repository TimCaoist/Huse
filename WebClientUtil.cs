using HuSe.Interface;
using HuSe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HuSe
{
    public static class WebClientUtil
    {
        private static readonly ICollection<WebClientEx> Proccers = new List<WebClientEx>();

        private static readonly List<MetaData> datas = new List<MetaData>();

        private static IHuSeConfig Config;

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private static Thread CustomerThread = new Thread(Cusomer);

        private static volatile bool IsIdle = false;

        static WebClientUtil()
        {
            ResetConfig(new DefaultHuSeConfig());
            CustomerThread.Start();
        }

        private static void Cusomer()
        {
            while (true)
            {
                if (Config.MaxProccer <= Proccers.Count)
                {
                    var clientEx = Proccers.FirstOrDefault(p => p.IsBusy == false);
                    if (clientEx == null)
                    {
                        continue;
                    }
                    
                    WebClientIdle(clientEx);
                }
                else {
                    var webClient = NewClient();
                    Proccers.Add(webClient);
                    WebClientIdle(webClient);
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
            IsIdle = false;
            metaData.ProcessNotify = processNotify;
            metaData.ReplaceExist = existReplace;
            datas.Add(metaData);
            autoResetEvent.Set();
        }

        private static WebClientEx NewClient()
        {
            var webClient = new WebClientEx
            {
                Idle = Notify
            };
            return webClient;
        }

        private static void Notify()
        {
            autoResetEvent.Set();
        }

        private static void Clear()
        {
            if (IsIdle == true)
            {
                return;
            }

            IsIdle = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(Config.IdleMaxClearTime * 1000);
                if (IsIdle == false)
                {
                    return;
                }

                var c = Proccers.Count();
                for (var i = c - 1; i >= 0; i--)
                {
                    var p = Proccers.ElementAt(i);
                    p.Idle -= Notify;
                    p.Dispose();
                    Proccers.Remove(p);
                }
            });
        }

        private static void WebClientIdle(WebClientEx clientEx)
        {
            MetaData meta = datas.FirstOrDefault();
            if (meta == null)
            {
                Clear();
                return;
            }
            
            if (meta.BatchId == 0)
            {
                datas.Remove(meta);
                clientEx.Down(meta, meta.ProcessNotify, Config.LocalFolder);
            }
            else {
                var c = datas.Count();
                IEnumerable<MetaData> batchDatas = datas.Where(d => d.BatchId == meta.BatchId).ToArray();
                datas.RemoveAll(d => d.BatchId == meta.BatchId);
                clientEx.Down(batchDatas, meta.ProcessNotify, Config.LocalFolder);
            }
        }

        public static void DownloadFile(string url, long id, IProcessNotify processNotify)
        {
            DownloadFile(url, url.GetHashCode().ToString(), id, processNotify, Config.ReplaceExist);
        }
        
        public static void DownloadFile(string url, long id = 0)
        {
            DownloadFile(url, url.GetHashCode().ToString(), id, null, Config.ReplaceExist);
        }

        public static void DownloadFiles(IEnumerable<string> urls, IProcessNotify processNotify = null, long batchId = 0)
        {
            var metaDatas = urls.Select(u => new MetaData
            {
                Url = u,
                LocalFileName = u.GetHashCode().ToString(),
            }).ToArray();

            DownloadFiles(metaDatas, batchId, processNotify, Config.ReplaceExist);
        }

        public static void DownloadFiles(IEnumerable<MetaData> metaDatas, long batchId = 0, IProcessNotify processNotify = null, bool existReplace = false)
        {
            IsIdle = false;
            foreach (var metaData in metaDatas)
            {
                metaData.BatchId = batchId;
                metaData.ProcessNotify = processNotify;
                metaData.ReplaceExist = existReplace;
                datas.Add(metaData);
            }

            autoResetEvent.Set();
        }
    }
}
