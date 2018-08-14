using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuSe.Interface;
using HuSe.Models;

namespace HuSe.Downloader
{
    public class SingleDownLoaderHandler : BaseDownloaderHanlder
    {
        private MetaData metaData;

        public SingleDownLoaderHandler(MetaData metaData)
        {
            this.metaData = metaData;
        }

        protected override void FinshedNotify(IProcessNotify processNotify)
        {
            processNotify?.Succeed(metaData);
        }

        protected override MetaData GetMetaData(int index)
        {
            return metaData;
        }

        protected override bool StepOver(int index, MetaData metaData)
        {
            return true;
        }
    }
}
