using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuSe.Interface
{
    public interface IHuSeConfig
    {
        int MaxProccer { get; }

        string LocalFolder { get; }

        bool ReplaceExist { get;  }

        string GetDefaultFile(string url);
    }
}
