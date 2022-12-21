using System;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Contracts
{
	public interface IAdapter
	{
		State State { get; set; }

		event EventHandler<AdapterStateChangedArgs> AdapterStateChanged;

		void Initialize();

		void Activate();

		void Deactivate();

		void Uninitialize();
	}
}
