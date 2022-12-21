using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    [Serializable]
    public class Verification : IVerification
    {
        #region Constructor

        /// <summary>
        /// Create an VerificationModel Intance
        /// </summary>
        public Verification() { }

        public Verification(int idInspectionHistory, int idTrackingStatus, int orderNumber, string traceabilityNumber, string heatNumber, int idTracking, DateTime inspectionDateTime)
        {
            this.idInspectionHistory = idInspectionHistory;
            this.idTrackingStatus = idTrackingStatus;
            this.OrderNumber = orderNumber;
            this.TraceabilityNumber = traceabilityNumber;
            this.HeatNumber = heatNumber;
            this.idTracking = idTracking;
            this.InspectionDateTime = inspectionDateTime;
        }

        #endregion

        #region IVerificationModel Members

        public int idInspectionHistory { get; set; }

        public int idTrackingStatus { get; set; }

        public int OrderNumber { get; set; }

        public string TraceabilityNumber { get; set; }

        public string HeatNumber { get; set; }

        public int idTracking { get; set; }

        public DateTimeOffset InspectionDateTime { get; set; }

        #endregion
    }
}
