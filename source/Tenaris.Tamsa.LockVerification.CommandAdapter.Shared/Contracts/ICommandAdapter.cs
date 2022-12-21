using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Contracts;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Events;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Adapter
{
	public interface ICommandAdapter : IAdapter
	{
		event EventHandler<CommandStatusChangedEventArgs> CommandStatusChanged;

		void SendLock(string command);

		void SendUnlock(string command);

		bool IsLocked(string command);
	}
}
