using HuSe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuSe.Downloader
{
    public static class DownloadHanlderFactory
    {
        public static BaseDownloaderHanlder Create(long batchId, params MetaData[] datas)
        {
            if (batchId == 0)
            {
                return new SingleDownLoaderHandler(datas.First());
            }

            return new MutilDownLoadHanlder(batchId, datas);
        }
    }
}
