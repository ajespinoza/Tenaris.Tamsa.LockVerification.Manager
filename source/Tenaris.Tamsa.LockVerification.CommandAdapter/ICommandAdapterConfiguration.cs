using System.Collections.Generic;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    public class ICommandAdapterConfiguration
    {
        IDictionary<string, ICommandConfig> CommandConfiguration { get; }
    }
}
