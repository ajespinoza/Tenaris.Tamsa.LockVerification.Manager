using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter.Shared
{
	[Serializable]
	public class AdapterStateChangedArgs : EventArgs
	{
		public State State { get; private set; }

		public AdapterStateChangedArgs(State state)
		{
			State = state;
		}
	}
}
