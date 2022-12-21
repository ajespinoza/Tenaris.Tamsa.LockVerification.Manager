using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tenaris.Tamsa.LockVerification.Manager
{
    [Serializable]
    public class ManagerConfiguration : ConfigurationSection, IXmlSerializable
    {
        /// <summary>
        /// 
        /// </summary>
        public static ManagerConfiguration Settings
        {
            get
            {
                return (ManagerConfiguration)ConfigurationManager.GetSection("ManagerConfiguration");
            }
            set
            {
                ConfigurationManager.RefreshSection("ManagerConfiguration");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("warningTime", IsRequired = true, DefaultValue = "3:30")]
        public string WarningTime
        {
            get { return base["warningTime"].ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("alarmTime", IsRequired = true, DefaultValue = "4:10")]
        public string AlarmTime
        {
            get { return base["alarmTime"].ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("blockTime", IsRequired = true, DefaultValue = "4:25")]
        public string BlockTime
        {
            get { return base["blockTime"].ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("toleranceTime", IsRequired = true, DefaultValue = "4:30")]
        public string ToleranceTime
        {
            get { return base["toleranceTime"].ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("verificationCount", IsRequired = true, DefaultValue = 4)]
        public int VerificationCount
        {
            get { return Convert.ToInt32(base["verificationCount"].ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("idTrackingStatus", IsRequired = true, DefaultValue = 5)]
        public int IdTrackingStatus
        {
            get { return Convert.ToInt32(base["idTrackingStatus"].ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("iterationsVerification", IsRequired = true, DefaultValue = "1000")]
        public int IterationsVerification
        {
            get { return Convert.ToInt32(base["iterationsVerification"].ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("applicationCode", IsRequired = true)]
        public string ApplicationCode
        {
            get
            {
                return base["applicationCode"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("applicationCommand", IsRequired = true)]
        public string ApplicationCommand
        {
            get
            {
                return base["applicationCommand"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("userN2", IsRequired = true)]
        public string UserN2
        {
            get
            {
                return base["userN2"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("mailAddressesTo", IsRequired = true)]
        public string MailAddressesTo
        {
            get
            {
                return base["mailAddressesTo"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("mailAddressesFrom", IsRequired = true)]
        public string MailAddressesFrom
        {
            get
            {
                return base["mailAddressesFrom"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("mailSubject", IsRequired = true)]
        public string MailSubject
        {
            get
            {
                return base["mailSubject"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("nameArea", IsRequired = true)]
        public string NameArea
        {
            get
            {
                return base["nameArea"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("adapterDependencies", IsRequired = true)]
        public string AdapterDependencies
        {
            get
            {
                return base["adapterDependencies"].ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("commandAdapterAssemblyFullName", IsRequired = true)]
        public string CommandAdapterAssemblyFullName
        {
            get
            {
                return base["commandAdapterAssemblyFullName"].ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("commandAdapterClassName", IsRequired = true)]
        public string CommandAdapterClassName
        {
            get
            {
                return base["commandAdapterClassName"].ToString();
            }
        }

        #region IXmlSerializable members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            DeserializeElement(reader, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        { }
        #endregion

    }
}
