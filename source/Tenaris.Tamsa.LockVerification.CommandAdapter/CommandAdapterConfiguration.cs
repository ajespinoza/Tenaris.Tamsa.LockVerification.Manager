using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    [Serializable]
    public class CommandAdapterConfiguration : ConfigurationSection, IXmlSerializable
    {
        [ConfigurationProperty("dependenciesPath")]
        public string DependenciesPath
        {
            get => (string)this["dependenciesPath"];
            set => this["dependenciesPath"] = (object)value;
        }

        [ConfigurationProperty("CommandConfigurations", IsDefaultCollection = true)]
        public CommandConfigCollection CommandConfigurations
        {
            get => (CommandConfigCollection)this[nameof(CommandConfigurations)];
            set => this[nameof(CommandConfigurations)] = (object)value;
        }

        public XmlSchema GetSchema() => (XmlSchema)null;

        public void ReadXml(XmlReader reader) => this.DeserializeElement(reader, false);

        public void WriteXml(XmlWriter writer)
        {
        }
    }
}
