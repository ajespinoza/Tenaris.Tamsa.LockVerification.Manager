using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    public enum LockVerificationStatus
    {
        /// <summary>
        /// Wait state
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Warning state
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Alarm state
        /// </summary>
        Alarm = 3,
        
        /// <summary>
        /// Alarm state
        /// </summary>
        Stop = 4

    }
}
