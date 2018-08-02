using HuSe.Interface;

namespace HuSe.Models
{
    public class DefaultHuSeConfig : IHuSeConfig
    {
        private const int maxProccer = 1;

        private const string folderName = "temp";

        private const int headBearSeconds = 10;

        public int MaxProccer {
            get
            {
                return maxProccer;
            }
        }

        public string LocalFolder
        {
            get
            {
                return folderName;
            }
        }
        public int IdleMaxClearTime => headBearSeconds;

        public bool ReplaceExist => false;
    }
}
