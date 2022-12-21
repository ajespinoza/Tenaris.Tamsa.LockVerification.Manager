
namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    public interface ICommandConfig
    {
        string CommandCode { get; }

        string MachineCode { get; }

        bool IsAutoUnlock { get; }

        bool CheckStatusToLock { get; }

        string LockingStatus { get; }

        string LockedStatus { get; }

        string UnlockingStatus { get; }

        string UnlockedStatus { get; }
    }
}
