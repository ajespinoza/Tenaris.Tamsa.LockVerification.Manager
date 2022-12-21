using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter.Shared
{
	[Serializable]
	public enum LockStatus
	{
		Locking,
		Locked,
		Unlocking,
		Unlocked
	}
}
