using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using Tenaris.Library.ConnectionMonitor;
using Tenaris.Library.Log;
using Tenaris.Library.System.Factory;
using Tenaris.Library.System.Remoting;
using Tenaris.Manager.Command.Common;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Adapter;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Contracts;
using Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.Events;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    public class CommandAdapter : MarshalByRefObject, ICommandAdapter, IAdapter, IDisposable
    {
        private Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State state = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Uninitialized;
        private Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.ProcessEvent processEvent = (Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.ProcessEvent)null;
        private static readonly object mutex = new object();
        private readonly object connectionmutex = new object();
        private static ICommandAdapter adapter = (ICommandAdapter)null;
        private bool isCommandManagerConnected = false;
        private readonly Dictionary<string, Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.CommandSubscription> subscriptions = new Dictionary<string, Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.CommandSubscription>();
        private readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
        private Dictionary<string, IMachine> commandMachine = new Dictionary<string, IMachine>();
        private CommandAdapterConfiguration Configuration;

        internal static ICommandManager manager { get; private set; }

        public CommandAdapter() => this.Configuration = (CommandAdapterConfiguration)ConfigurationManager.GetSection("Tenaris.Tamsa.LockVerification.CommandAdapter");

        public event EventHandler<CommandStatusChangedEventArgs> CommandStatusChanged;

        public void SendLock(string command)
        {
            if (!Monitor.TryEnter(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex))
                return;
            try
            {
                bool flag = false;
                int num = 0;
                while (num <= 3 && !flag)
                {
                    try
                    {
                        this.commands[command].Send();
                        flag = true;
                    }
                    catch
                    {
                        ++num;
                        Trace.Error("Error sending command {0}, starting retry #{1}", (object)command, (object)num);
                        Thread.Sleep(300);
                    }
                }
            }
            finally
            {
                Monitor.Exit(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex);
            }
        }

        public void SendUnlock(string command)
        {
            if (!Monitor.TryEnter(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex))
                return;
            try
            {
                if (this.commands != null && this.commands.Count > 0 && this.commands.ContainsKey(command))
                    this.commands[command].SendReset();
            }
            finally
            {
                Monitor.Exit(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex);
            }
        }

        public bool IsLocked(string command)
        {
            bool flag = false;
            if (this.Configuration.CommandConfigurations.Commands[command].CheckStatusToLock)
            {
                if (Monitor.TryEnter(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex))
                {
                    try
                    {
                        if (this.commands != null && this.commands.Count > 0 && this.commands.ContainsKey(command))
                            flag = !(this.commands[command].Status.Name == this.Configuration.CommandConfigurations.Commands[command].UnlockedStatus);
                    }
                    finally
                    {
                        Monitor.Exit(Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.mutex);
                    }
                }
            }
            return flag;
        }

        public event EventHandler<AdapterStateChangedArgs> AdapterStateChanged;

        public Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State State
        {
            get => this.state;
            set
            {
                this.state = value;
                if (this.AdapterStateChanged == null)
                    return;
                this.AdapterStateChanged((object)this, new AdapterStateChangedArgs(this.state));
            }
        }

        public void Initialize()
        {
            try
            {
                //Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager = FactoryProvider.Instance.CreateEntity<ICommandManager>("Tenaris.Manager.Command.CommandManager").Create();
                //ProxyConfiguration.ConfigureRemoting();
                IFactory<ICommandManager> factory2 = FactoryProvider.Instance.CreateFactory<ICommandManager>("Tenaris.Manager.Command.CommandManager");
                Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager = factory2.Create();

                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.StateChanged -= new EventHandler<StateChangeEventArgs>(this.Instance_StateChanged);
                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.StateChanged += new EventHandler<StateChangeEventArgs>(this.Instance_StateChanged);
                this.SubscribeCommands();
                Trace.Message("!Created Singleton source Command Adapter instance was Successful");
                this.State = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Initialized;
            }
            catch (Exception ex)
            {
                Trace.Error("Exception in connection to Command Manager : {0}", ex.Message);
            }
        }

        public void Activate()
        {
            if(this.State == Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Initialized)
            {
                this.State = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Activated;
            }
            else
            {
                string empty = string.Empty;
                if (this.State == Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Activated)
                    throw new Exception("Adapter must be Initialized First!");
            }
        }

        public void Deactivate() => this.Uninitialize();

        public void Uninitialize()
        {
            try
            {
                if (Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager == null)
                    return;
                this.UnsubscribeCommands();
                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.StateChanged -= new EventHandler<StateChangeEventArgs>(this.Instance_StateChanged);
                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.Stop();
                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.Abort();
                Tenaris.Library.ConnectionMonitor.ConnectionMonitor.Instance.Dispose();
                Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager = (ICommandManager)null;
                this.State = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Uninitialized;
            }
            catch (Exception ex)
            {
                Trace.Error("Exception on command manager connection: {0}", (object)ex.Message);
            }
        }

        private void SubscribeCommands()
        {
            if (Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager == null)
                return;
            foreach (KeyValuePair<string, ICommandConfig> command1 in (IEnumerable<KeyValuePair<string, ICommandConfig>>)this.Configuration.CommandConfigurations.Commands)
            {
                if (!this.commandMachine.ContainsKey(command1.Value.MachineCode))
                    this.commandMachine.Add(command1.Value.MachineCode, Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager.GetMachine(command1.Value.MachineCode));
                foreach (ICommand command2 in this.commandMachine[command1.Value.MachineCode].Commands.Values)
                {
                    if (command2.Code == command1.Value.CommandCode)
                    {
                        if (!this.subscriptions.ContainsKey(command1.Key))
                            this.subscriptions.Add(command1.Value.CommandCode, new Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.CommandSubscription());
                        if (!this.commands.ContainsKey(command1.Key))
                            this.commands.Add(command1.Key, command2);
                        this.commands[command1.Key].StatusChanged -= new EventHandler<StatusChangedEventArgs>(this.subscriptions[command1.Value.CommandCode].StatusChanged.Handler);
                        this.commands[command1.Key].StatusChanged += new EventHandler<StatusChangedEventArgs>(this.subscriptions[command1.Value.CommandCode].StatusChanged.Handler);
                        this.subscriptions[command1.Value.CommandCode].StatusChanged.Event -= new EventHandler<StatusChangedEventArgs>(this.OnCommandStatusChanged);
                        this.subscriptions[command1.Value.CommandCode].StatusChanged.Event += new EventHandler<StatusChangedEventArgs>(this.OnCommandStatusChanged);
                    }
                }
            }
        }

        private void UnsubscribeCommands()
        {
            if (Tenaris.Tamsa.LockVerification.CommandAdapter.CommandAdapter.manager == null)
                return;
            foreach (KeyValuePair<string, ICommandConfig> command1 in (IEnumerable<KeyValuePair<string, ICommandConfig>>)this.Configuration.CommandConfigurations.Commands)
            {
                try
                {
                    foreach (ICommand command2 in this.commands.Values)
                    {
                        if (command2.Code == command1.Value.CommandCode)
                        {
                            command2.StatusChanged -= new EventHandler<StatusChangedEventArgs>(this.subscriptions[command1.Value.CommandCode].StatusChanged.Handler);
                            this.subscriptions[command1.Value.CommandCode].StatusChanged.Event -= new EventHandler<StatusChangedEventArgs>(this.OnCommandStatusChanged);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Exception(ex, "FAILED Unsubscribing command {0} - ", (object)command1.Value.CommandCode);
                }
            }
            this.subscriptions.Clear();
            this.commands.Clear();
        }

        private void OnCommandStatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (sender == null)
                return;
            Trace.Debug("Command Status Changed - Command: {0}, Status: {1}, Read: {2}, Write: {3}", (object)((ICommand)sender).Code, (object)e.Status.Name, (object)e.Status.ReadValue, (object)e.Status.WriteValue);
            LockStatus lockStatus = LockStatus.Unlocked;
            string code1 = ((ICommand)sender).Definition.Code;
            string code2 = ((ICommand)sender).Code;
            foreach (ICommandConfig commandConfig in (IEnumerable<ICommandConfig>)this.Configuration.CommandConfigurations.Commands.Values)
            {
                if (commandConfig.CommandCode == code2 && commandConfig.CheckStatusToLock)
                {
                    Trace.Debug("Command: {0}. Locked status: {1}.", (object)commandConfig.CommandCode, (object)e.Status.Name);
                    if (e.Status.Name == commandConfig.LockingStatus)
                        lockStatus = LockStatus.Locking;
                    else if (e.Status.Name == commandConfig.LockedStatus)
                        lockStatus = LockStatus.Locked;
                    else if (e.Status.Name == commandConfig.UnlockingStatus)
                        lockStatus = LockStatus.Unlocking;
                    else if (e.Status.Name == commandConfig.UnlockedStatus)
                        lockStatus = LockStatus.Unlocked;
                    if (lockStatus == LockStatus.Locked && commandConfig.IsAutoUnlock)
                        this.SendUnlock(commandConfig.CommandCode);
                }
            }
            CommandStatusChangedEventArgs e1 = new CommandStatusChangedEventArgs(((ICommand)sender).Code, lockStatus);
            if (this.CommandStatusChanged != null)
                this.CommandStatusChanged((object)this, e1);
        }

        private void Reconnect()
        {
            this.UnsubscribeCommands();
            this.SubscribeCommands();
            this.State = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Activated;
        }

        public void Instance_StateChanged(object sender, StateChangeEventArgs e)
        {
            lock (this.connectionmutex)
            {
                try
                {
                    bool isConnected;
                    if (e.IsConnected)
                    {
                        if (e.Connection is ICommandManager)
                        {
                            this.isCommandManagerConnected = true;
                            Console.ForegroundColor = ConsoleColor.Green;
                            object[] objArray1 = new object[1];
                            object[] objArray2 = objArray1;
                            isConnected = e.IsConnected;
                            string str = isConnected.ToString();
                            objArray2[0] = (object)str;
                            Trace.Message("Command connection state changed: [{0}]", objArray1);
                            Console.ResetColor();
                            this.Reconnect();
                        }
                    }
                    else if (e.Connection is ICommandManager)
                    {
                        this.isCommandManagerConnected = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        object[] objArray3 = new object[1];
                        object[] objArray4 = objArray3;
                        isConnected = e.IsConnected;
                        string str = isConnected.ToString();
                        objArray4[0] = (object)str;
                        Trace.Message("Command connection state changed: [{0}]", objArray3);
                        Console.ResetColor();
                        this.UnsubscribeCommands();
                        this.State = Tenaris.Tamsa.LockVerification.CommandAdapter.Shared.State.Initialized;
                    }
                    object[] objArray5 = new object[1];
                    object[] objArray6 = objArray5;
                    isConnected = e.IsConnected;
                    string str1 = isConnected.ToString();
                    objArray6[0] = (object)str1;
                    Trace.Message("Command connection state: [{0}]", objArray5);
                }
                catch (Exception ex)
                {
                    Trace.Exception(ex, "Exception on connection monitor callback: [{0}]", (object)ex.Message);
                }
            }
        }

        public void Dispose() => throw new NotImplementedException();

        public override object InitializeLifetimeService() => (object)null;

        private delegate void ProcessEvent(StatusChangedEventArgs e);

        internal class CommandSubscription : IDisposable
        {
            public readonly RemotableEvent<StatusChangedEventArgs> StatusChanged = new RemotableEvent<StatusChangedEventArgs>();

            public void Dispose()
            {
            }
        }
        internal class RemotableEvent<TEventArgs> : MarshalByRefObject where TEventArgs : EventArgs
        {
            private readonly object eventLock = new object();

            /// <summary>
            /// The event to connect from the client side.
            /// </summary>
            public event EventHandler<TEventArgs> Event
            {
                add
                {
                    lock (eventLock)
                    {
                        EventTarget += value;
                    }
                }
                remove
                {
                    lock (eventLock)
                    {
                        EventTarget -= value;
                    }
                }
            }

            private event EventHandler<TEventArgs> EventTarget;

            /// <summary>
            /// The entry point for the server-side class to invoke events remotly.
            /// </summary>
            /// <param name="sender">
            /// The event sender.
            /// </param>
            /// <param name="args">
            /// The event args.
            /// </param>
            public void Handler(object sender, TEventArgs args)
            {
                lock (eventLock)
                {
                    if (this.EventTarget != null)
                    {
                        this.EventTarget(sender, args);
                    }
                }
            }

            /// <summary>
            /// Overrides the lifetime service initializer so the object is never dipose by GC.
            /// </summary>
            /// <returns>
            /// Always null.
            /// </returns>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }
    }
}
