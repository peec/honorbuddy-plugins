using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace com.peec.itemupgrader
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }


        private void FormSettings_Load(object sender, EventArgs e)
        {
            pgSettings.SelectedObject = ItemUpgraderSettings.Instance;
        }

        private void pgSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (pgSettings.SelectedObject != null && pgSettings.SelectedObject is ItemUpgraderSettings)
                ((ItemUpgraderSettings)pgSettings.SelectedObject).Save();
        }



    }
}
