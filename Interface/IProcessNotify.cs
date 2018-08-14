using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuSe.Models;

namespace HuSe.Interface
{
    public interface IProcessNotify
    {
        void BatchSucceed(long batchId, IEnumerable<MetaData> datas);

        void Succeed(MetaData wrapperData);

        void Error(Exception exception, MetaData wrapperData);

        void Progress(MetaData userState, long bytesReceived, long totalBytesToReceive, int progressPercentage);
    }
}
