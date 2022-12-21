using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    public interface ILockVerificationHistory
    {
        int idLockVerificationHistory { get; set; }

        LockVerificationStatus idStatusLockVerification { get; set; }

        string NameStatusLockVerification { get; set; }

        int idLastInspectionHistory { get; set; }

        DateTimeOffset LastInspectionDate { get; set; }

        int? idUserAuthorization { get; set; }

        string UserAuthorization { get; set; }

        string Comments { get; set; }

        int idUserLoggedIn { get; set; }

        string UserLoggedIn { get; set; }

        DateTimeOffset OpenDateTime { get; set; }

        DateTimeOffset? AcceptDateTime { get; set; }

        int? idTrackingLock { get; set; }

        int? idItemStatusLock { get; set; }

        DateTimeOffset InsertDateTime { get; set; }        
    }
}
