using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Library.Log;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    public static class Utility
    {
        public static string GetProxyUri(object proxy)
        {
            try
            {
                if (proxy is MarshalByRefObject)
                {
                    var objRef = System.Runtime.Remoting.RemotingServices.GetObjRefForProxy((MarshalByRefObject)proxy);
                    var channelDataStore = (System.Runtime.Remoting.Channels.ChannelDataStore)objRef.ChannelInfo.ChannelData.Where(data => data is System.Runtime.Remoting.Channels.ChannelDataStore).FirstOrDefault();
                    if (channelDataStore != null)
                    {
                        return String.Join(", ", channelDataStore.ChannelUris);
                    }
                }

                return String.Empty;
            }
            catch (Exception ex)
            {
                Trace.Warning("Exception getting proxy URI: {0}", ex.Message);
                return String.Empty;
            }
        }
    }
}
