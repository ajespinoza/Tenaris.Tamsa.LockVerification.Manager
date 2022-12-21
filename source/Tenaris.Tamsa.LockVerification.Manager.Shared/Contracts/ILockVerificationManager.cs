using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Manager.Forum.Shared;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    public interface ILockVerificationManager : IManager, IDisposable
    {
        #region Attributes

        int idLockVerificationHistory { get; set; }

        IVerification LastVerification { get; set; }

        ILockVerificationHistory LastLockVerificationHistory { get; set; }

        LockVerificationStatus CurrentStatus { get; set; }

        #endregion

        #region Events

        event EventHandler<LockVerificationStatusChangeEventArgs> OnLockVerificationStatusChange;

        event EventHandler<LastVerificationChangeEventArgs> OnLastVerificationChange;

        #endregion

        #region Methods

        TransactionResult SendAuthorization(string user, string password, string comment);

        TransactionResult SaveAccept();

        Dictionary<int,string> GetTimeConfiguration();

        #endregion

    }
}
