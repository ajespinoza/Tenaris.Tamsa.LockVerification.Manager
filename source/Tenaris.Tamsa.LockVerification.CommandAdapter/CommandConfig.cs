using System;
using System.Configuration;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    [Serializable]
    public class CommandConfig : ConfigurationElement, ICommandConfig
    {
        [ConfigurationProperty("MachineCode", IsRequired = true)]
        public string MachineCode => (string)this[nameof(MachineCode)];

        [ConfigurationProperty("CommandCode", IsRequired = true)]
        public string CommandCode => (string)this[nameof(CommandCode)];

        [ConfigurationProperty("IsAutoUnlock", IsRequired = true)]
        public bool IsAutoUnlock
        {
            get => (bool)this[nameof(IsAutoUnlock)];
            set => this[nameof(IsAutoUnlock)] = value ? (object)"true" : (object)"false";
        }

        [ConfigurationProperty("CheckStatusToLock", DefaultValue = false, IsRequired = false)]
        public bool CheckStatusToLock
        {
            get => (bool)this[nameof(CheckStatusToLock)];
            set => this[nameof(CheckStatusToLock)] = value ? (object)"true" : (object)"false";
        }

        [ConfigurationProperty("LockingStatus", IsRequired = false)]
        public string LockingStatus => (string)this[nameof(LockingStatus)];

        [ConfigurationProperty("LockedStatus", IsRequired = true)]
        public string LockedStatus => (string)this[nameof(LockedStatus)];

        [ConfigurationProperty("UnlockingStatus", IsRequired = false)]
        public string UnlockingStatus => (string)this[nameof(UnlockingStatus)];

        [ConfigurationProperty("UnlockedStatus", IsRequired = true)]
        public string UnlockedStatus => (string)this[nameof(UnlockedStatus)];
    }
}
