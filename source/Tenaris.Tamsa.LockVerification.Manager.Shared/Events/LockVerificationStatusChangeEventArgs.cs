using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    [Serializable]
    public class LockVerificationStatusChangeEventArgs : EventArgs
    {
        public int idLockVerificationHistory { get; set; }

        public LockVerificationStatus Status { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
