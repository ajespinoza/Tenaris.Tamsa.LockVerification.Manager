using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Library.Framework.Factory;
using Tenaris.Library.Log;
using Tenaris.Tamsa.LockVerification.Manager.Shared;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public class Factory : Factory<ILockVerificationManager>
    {
        #region Fields
        /// <summary>
        /// 
        /// </summary>
        private readonly object sync = new object();
        /// <summary>
        /// 
        /// </summary>
        private LockVerificationManager instance;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the lf manager configuration.
        /// </summary>
        public ManagerConfiguration ServiceConfiguration { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ILockVerificationManager Create()
        {
            try
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        if (instance == null)
                        {
                            instance = new LockVerificationManager();
                        }
                    }
                }

                return instance;
            }
            catch (Exception ex)
            {
                Trace.Error("Exception at manager factory creating Profile manager: {0}", ex.Message);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configString"></param>
        /// <returns></returns>
        protected override object DeserializeConfiguration(string configString)
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void DoConfigure()
        { }
    }
}
