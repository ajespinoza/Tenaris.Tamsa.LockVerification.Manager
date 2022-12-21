using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    [Serializable]
    public class LockVerificationHistory : ILockVerificationHistory
    {
        #region Constructor

        /// <summary>
        /// Create an LockVerificationHistory Intance
        /// </summary>
        public LockVerificationHistory() { }

        #endregion

        #region ILockVerificationHistory Members

        public int idLockVerificationHistory { get; set; }

        public LockVerificationStatus idStatusLockVerification { get; set; }

        public string NameStatusLockVerification { get; set; }

        public int idLastInspectionHistory { get; set; }

        public DateTimeOffset LastInspectionDate { get; set; }

        public int? idUserAuthorization { get; set; }

        public string UserAuthorization { get; set; }

        public string Comments { get; set; }

        public int idUserLoggedIn { get; set; }

        public string UserLoggedIn { get; set; }

        public DateTimeOffset OpenDateTime { get; set; }

        public DateTimeOffset? AcceptDateTime { get; set; }

        public int? idTrackingLock { get; set; }

        public int? idItemStatusLock { get; set; }

        public DateTimeOffset InsertDateTime { get; set; }

        #endregion
    }
}
