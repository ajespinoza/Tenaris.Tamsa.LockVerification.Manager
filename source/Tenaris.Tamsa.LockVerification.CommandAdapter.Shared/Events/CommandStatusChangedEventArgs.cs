using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Events
{
	[Serializable]
	public class CommandStatusChangedEventArgs : EventArgs
	{
		private string commandCode;

		private LockStatus lockStatus;

		public string CommandCode => commandCode;

		public LockStatus LockStatus => lockStatus;

		public CommandStatusChangedEventArgs(string commandCode, LockStatus lockStatus)
		{
			this.commandCode = commandCode;
			this.lockStatus = lockStatus;
		}
	}

}
