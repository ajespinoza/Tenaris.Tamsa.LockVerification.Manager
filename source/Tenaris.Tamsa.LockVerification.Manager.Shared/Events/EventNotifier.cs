using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tenaris.Library.Framework.Remoting;
using Tenaris.Library.Log;

namespace Tenaris.Tamsa.LockVerification.Manager.Shared
{
    public interface IEventNotifier<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Gets RemoteEvent.
        /// </summary>
        RemoteEvent<TEventArgs> RemoteEvent { get; }

        /// <summary>
        /// Enqueue event to be sent asynchronously
        /// </summary>
        /// <param name="argument">
        /// The event args.
        /// </param>
        void Notify(TEventArgs argument);
    }

    /// <summary>
    /// Asynchronous notifier for remote events
    /// </summary>
    /// <typeparam name="TEventArgs">
    /// Event args type
    /// </typeparam>
    public class EventNotifier<TEventArgs> : IEventNotifier<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Utility remote event
        /// </summary>
        private readonly RemoteEvent<TEventArgs> remoteEvent = new RemoteEvent<TEventArgs>(true);

        /// <summary>
        /// Queue for event args
        /// </summary>
        private readonly Queue<TEventArgs> eventArgsQueue = new Queue<TEventArgs>(50);

        /// <summary>
        /// Utility object used for queue synchronization
        /// </summary>
        private readonly object lockQueue = new object();

        /// <summary>
        /// Name of event to handled
        /// </summary>
        private readonly string eventName;

        /// <summary>
        /// Sender of events to be notified
        /// </summary>
        private readonly object sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventNotifier{TEventArgs}"/> class.
        /// </summary>
        /// <param name="eventName">
        /// The event Name.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public EventNotifier(string eventName, object sender)
        {
            if (String.IsNullOrEmpty(eventName))
            {
                throw new ArgumentNullException("eventName");
            }

            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            this.eventName = eventName;
            this.sender = sender;

            this.remoteEvent.ErrorDetected += this.OnErrorDetected;
        }

        /// <summary>
        /// Gets RemoteEvent.
        /// </summary>
        public RemoteEvent<TEventArgs> RemoteEvent
        {
            get
            {
                return this.remoteEvent;
            }
        }

        /// <summary>
        /// Enqueue event to be sent asynchronously
        /// </summary>
        /// <param name="argument">
        /// The event args.
        /// </param>
        public void Notify(TEventArgs argument)
        {
            lock (this.lockQueue)
            {
                this.eventArgsQueue.Enqueue(argument);
            }

            // Schedule worker to process the event asynchronoulsy
            if (!ThreadPool.QueueUserWorkItem(this.RaiseEvent))
            {
                var eventRaiser = new Thread(this.RaiseEvent) { IsBackground = true };
                eventRaiser.Start();
            }
        }

        /// <summary>
        /// Dequeue event args and raise event
        /// </summary>
        /// <param name="foo">
        /// Placeholder required for ThreadPool.QueueUserWorkItem.
        /// </param>
        private void RaiseEvent(object foo)
        {
            try
            {
                TEventArgs argument = null;

                lock (this.lockQueue)
                {
                    if (this.eventArgsQueue.Count > 0)
                    {
                        argument = this.eventArgsQueue.Dequeue();
                    }
                }

                if (argument != null && this.remoteEvent != null)
                {
                    this.remoteEvent.InvokeEventAsync(this.sender, argument);
                    Trace.Message("Event {0} raised with data {1}", this.eventName, argument);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Trace.Exception(ex, "EXCEPTION raising event {0} : {1}", this.eventName, ex.Message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Handler for ErrorDetected event
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="disconnect">
        /// The disconnect.
        /// </param>
        private void OnErrorDetected(object sender, Exception error, Delegate client, ref bool disconnect)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Trace.Message("Error dispatching event {0}, status {1}: {2}", this.eventName, disconnect ? "Disconnect" : "Connect", error.Message);
            Console.ResetColor();
        }
    }
}
