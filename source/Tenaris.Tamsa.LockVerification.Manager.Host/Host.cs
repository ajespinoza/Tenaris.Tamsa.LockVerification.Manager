using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Library.Framework.Factory;
using Tenaris.Library.Log;
using Tenaris.Manager.Forum.Shared;
using Tenaris.Tamsa.LockVerification.Manager.Shared;

namespace Tenaris.Tamsa.LockVerification.Manager.Host
{
    public class Host
    {
        /// <summary>
        /// 
        /// </summary>
        private static ILockVerificationManager lockVerificationManager;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
                System.Runtime.Remoting.RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);
                var factory = FactoryProvider.Instance.CreateFactory<ILockVerificationManager>("LockVerificationConfiguration");
                lockVerificationManager = factory.Create();

                Trace.Message(" ========================================");
                Trace.Message(" === Manager LockVerification Created ===");
                Trace.Message(" ========================================");

                Trace.Message("Manager LockVerification Initialized...");
                lockVerificationManager.Initialize();
                

                Trace.Message("Manager LockVerification Activated...");
                lockVerificationManager.Activate();
                

                System.Runtime.Remoting.RemotingServices.Marshal((MarshalByRefObject)lockVerificationManager, "Tenaris.Tamsa.LockVerification.Manager.soap");
                
                Trace.Warning("Manager LockVerification Published");
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Failed to initialize Manager LockVerification: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            try
            {
                IManager manager = (IManager)lockVerificationManager;

                Trace.Message("Manager LockVerification Deactivate...");
                manager.Deactivate();

                Trace.Message("Manager LockVerification Uninitialize...");
                manager.Uninitialize();

                System.Runtime.Remoting.RemotingServices.Disconnect((MarshalByRefObject)lockVerificationManager);
                manager = null;
                lockVerificationManager = null;
                
                Trace.Warning("Manager LockVerification Stoped...");
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Failed to Uninitialize Sensors Service: {0}", ex.Message);
            }
        }
    }
}
