using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Styx.Helpers;
using Styx.Common;
using Styx;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace com.peec.itemupgrader
{
    class ItemUpgraderSettings : Settings
    {
        private static ItemUpgraderSettings _instance;
        public static ItemUpgraderSettings Instance { get { return _instance ?? (_instance = new ItemUpgraderSettings()); } }

        public ItemUpgraderSettings()
            : base(Path.Combine(Path.Combine(Styx.Helpers.GlobalSettings.SettingsDirectory, "Settings"), string.Format("ItemUpgraderSettings_{0}.xml", StyxWoW.Me.Name)))
        {

        }

        

        [Setting]
        [Category("Upgrades")]
        [DefaultValue(true)]
        [DisplayName("Honor points")]
        [Description("Use Honor on upgrades.")]
        public Boolean enableHonor { get; set; }

        [Setting]
        [Category("Upgrades")]
        [DefaultValue(false)]
        [DisplayName("Conquest points")]
        [Description("Use Conquest on upgrades.")]
        public Boolean enableConquest { get; set; }


        [Setting]
        [Category("Upgrades")]
        [DefaultValue(false)]
        [DisplayName("Justice points")]
        [Description("Use Justice on upgrades.")]
        public Boolean enableJustice { get; set; }


        [Setting]
        [Category("Upgrades")]
        [DefaultValue(false)]
        [DisplayName("Valor points")]
        [Description("Use Valor on upgrades.")]
        public Boolean enableValor { get; set; }


        [Setting]
        [Category("Developers")]
        [DefaultValue(false)]
        [DisplayName("Debug")]
        [Description("Enable debug mode.")]
        public Boolean enableDebug { get; set; }
    }
}
