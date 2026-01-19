using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTool.Base.Common
{
    public class SimpleLogInfo
    {
        public string header;
        public string[] data;

        public SimpleLogInfo(string header)
        {
            this.header = header;
        }
    }
}
