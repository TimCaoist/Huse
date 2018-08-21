using HuSe.Interface;

namespace HuSe.Models
{
    public class DefaultHuSeConfig : IHuSeConfig
    {
        private const int maxProccer = 5;

        private const string folderName = "temp";

        public virtual int MaxProccer {
            get
            {
                return maxProccer;
            }
        }

        public virtual string LocalFolder
        {
            get
            {
                return folderName;
            }
        }

        public bool ReplaceExist => false;

        public virtual string GetDefaultFile(string url)
        {
            return url.GetHashCode().ToString();
        }
    }
}
