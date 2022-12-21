using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    [Serializable]
    public class TransactionResult
    {
        public int Code { get; set; }

        public string Tag { get; set; }

        public string Message { get; set; }
    }
}
