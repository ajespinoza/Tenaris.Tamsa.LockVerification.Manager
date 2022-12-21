using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    [Serializable]
    public class LastVerificationChangeEventArgs : EventArgs
    {
        public IVerification LastVerification { get; set; }

        public string Chronometer { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
