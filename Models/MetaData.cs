using HuSe.Interface;

namespace HuSe.Models
{
    public class MetaData
    {
        public string Url { get; set; }

        public string LocalFileName { get; set; }

        internal long BatchId { get; set; }

        public string FullPath { get; internal set; }

        internal IProcessNotify ProcessNotify { get; set; }

        internal bool ReplaceExist { get; set; }

        public object Data { get; set; }
    }
}
