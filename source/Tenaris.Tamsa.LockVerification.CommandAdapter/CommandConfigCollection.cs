using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Tenaris.Tamsa.LockVerification.CommandAdapter
{
    [ConfigurationCollection(typeof(CommandConfig), AddItemName = "CommandConfig")]
    public class CommandConfigCollection : ConfigurationElementCollection
    {
        private IDictionary<string, ICommandConfig> commands = (IDictionary<string, ICommandConfig>)null;

        protected override ConfigurationElement CreateNewElement() => (ConfigurationElement)new CommandConfig();

        protected override object GetElementKey(ConfigurationElement element) => (object)((CommandConfig)element).CommandCode;

        public CommandConfig this[int idx]
        {
            get => this.BaseGet(idx) as CommandConfig;
            set
            {
                if (this.BaseGet(idx) != null)
                    this.BaseRemoveAt(idx);
                this.BaseAdd(idx, (ConfigurationElement)value);
            }
        }

        public CommandConfig this[string key]
        {
            get => this.BaseGet((object)key) as CommandConfig;
            set
            {
                if (this.BaseGet((object)key) != null)
                    this.BaseRemove((object)key);
                this.BaseAdd(this.Count, (ConfigurationElement)value);
            }
        }

        public IDictionary<string, ICommandConfig> Commands
        {
            get
            {
                if (this.commands == null)
                {
                    this.commands = (IDictionary<string, ICommandConfig>)new Dictionary<string, ICommandConfig>();
                    foreach (CommandConfig commandConfig in (ConfigurationElementCollection)this)
                        this.commands.Add(commandConfig.CommandCode, (ICommandConfig)commandConfig);
                }
                else if (this.commands.Count != this.Count)
                {
                    this.commands.Clear();
                    foreach (CommandConfig commandConfig in (ConfigurationElementCollection)this)
                        this.commands.Add(commandConfig.CommandCode, (ICommandConfig)commandConfig);
                }
                return this.commands;
            }
        }
    }
}
