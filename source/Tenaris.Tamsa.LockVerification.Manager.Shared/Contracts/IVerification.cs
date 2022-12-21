using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    public interface IVerification
    {
        int idInspectionHistory { get; set; }

        int idTrackingStatus { get; set; }

        int OrderNumber { get; set; }

        string TraceabilityNumber { get; set; }

        string HeatNumber { get; set; }

        int idTracking { get; set; }

        DateTimeOffset InspectionDateTime { get; set; }
    }
}
