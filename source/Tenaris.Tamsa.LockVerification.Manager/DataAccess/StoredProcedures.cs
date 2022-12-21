using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    public class StoredProcedures
    {
        #region Get Stored Procedures

        public const string SpGetLastVerification = "[Ndt_Tamsa].[GetLastVerification]";

        public const string SpGetLastLockVerificationHistory = "[Ndt_Tamsa].[GetLastLockVerificationHistory]";

        public const string SpInsLockVerificationHistory = "[Ndt_Tamsa].[InsLockVerificationHistory]";

        public const string SpUpdLockVerificationHistoryAuthorization = "[Ndt_Tamsa].[UpdLockVerificationHistoryAuthorization]";

        public const string SpUpdLockVerificationHistoryAccept = "[Ndt_Tamsa].[UpdLockVerificationHistoryAccept]";


        #endregion

        #region Parameters

        public static class Parameters
        {
            /// <summary>
            /// 
            /// </summary>
            public const string idTrackingStatus = "@idTrackingStatus";

            /// <summary>
            /// 
            /// </summary>
            public const string VerificationCount = "@VerificationCount";

            /// <summary>
            /// 
            /// </summary>
            public const string idStatusLockVerification = "@idStatusLockVerification";

            /// <summary>
            /// 
            /// </summary>
            public const string idLastInspectionHistory = "@idLastInspectionHistory";

            /// <summary>
            /// 
            /// </summary>
            public const string OpenDateTime = "@OpenDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string idLockVerificationHistory = "@idLockVerificationHistory";

            /// <summary>
            /// 
            /// </summary>
            public const string User = "@User";

            /// <summary>
            /// 
            /// </summary>
            public const string Password = "@Password";

            /// <summary>
            /// 
            /// </summary>
            public const string Comments = "@Comments";

            /// <summary>
            /// 
            /// </summary>
            public const string AcceptDateTime = "@AcceptDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string ApplicationCode = "@ApplicationCode";

            /// <summary>
            /// 
            /// </summary>
            public const string ApplicationCommand = "@ApplicationCommand";
        }

        #endregion

        #region Fields

        public static class Fields
        {
            /// <summary>
            /// 
            /// </summary>
            public const string idInspectionHistory = "idInspectionHistory";

            /// <summary>
            /// 
            /// </summary>
            public const string idTrackingStatus = "idTrackingStatus";

            /// <summary>
            /// 
            /// </summary>
            public const string OrderNumber = "OrderNumber";

            /// <summary>
            /// 
            /// </summary>
            public const string TraceabilityNumber = "TraceabilityNumber";

            /// <summary>
            /// 
            /// </summary>
            public const string HeatNumber = "HeatNumber";

            /// <summary>
            /// 
            /// </summary>
            public const string idTracking = "idTracking";

            /// <summary>
            /// 
            /// </summary>
            public const string EndDateTime = "EndDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string idLockVerificationHistory = "idLockVerificationHistory";

            /// <summary>
            /// 
            /// </summary>
            public const string idStatusLockVerification = "idStatusLockVerification";

            /// <summary>
            /// 
            /// </summary>
            public const string idLastInspectionHistory = "idLastInspectionHistory";

            /// <summary>
            /// 
            /// </summary>
            public const string idUserAuthorization = "idUserAuthorization";

            /// <summary>
            /// 
            /// </summary>
            public const string idUserLoggedIn = "idUserLoggedIn";

            /// <summary>
            /// 
            /// </summary>
            public const string OpenDateTime = "OpenDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string AcceptDateTime = "AcceptDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string idTrackingLock = "idTrackingLock";

            /// <summary>
            /// 
            /// </summary>
            public const string idItemStatusLock = "idItemStatusLock";

            /// <summary>
            /// 
            /// </summary>
            public const string InsDateTime = "InsDateTime";

            /// <summary>
            /// 
            /// </summary>
            public const string Code = "Code";

            /// <summary>
            /// 
            /// </summary>
            public const string Message = "Message";

        }

        #endregion
    }
}
