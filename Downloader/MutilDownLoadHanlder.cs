using HuSe.Interface;
using HuSe.Models;
using System.Linq;

namespace HuSe.Downloader
{
    public class MutilDownLoadHanlder : BaseDownloaderHanlder
    {
        private MetaData[] datas;

        private long batchId;

        public MutilDownLoadHanlder(long batchId, MetaData[] datas)
        {
            this.datas = datas;
            this.batchId = batchId;
        }

        protected override void FinshedNotify(IProcessNotify processNotify)
        {
           processNotify?.BatchSucceed(batchId, datas);
        }

        protected override MetaData GetMetaData(int index)
        {
            return datas[index];
        }

        protected override bool StepOver(int index, MetaData metaData)
        {
           return index >= datas.Count() - 1;
        }
    }
}
