namespace com.peec.itemupgrader
{
	partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.pgSettings = new System.Windows.Forms.PropertyGrid();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Impact", 17F);
            this.label1.Location = new System.Drawing.Point(4, 558);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "ItemUpgrader";
            // 
            // pgSettings
            // 
            this.pgSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.pgSettings.Location = new System.Drawing.Point(0, 0);
            this.pgSettings.Margin = new System.Windows.Forms.Padding(4);
            this.pgSettings.Name = "pgSettings";
            this.pgSettings.Size = new System.Drawing.Size(436, 556);
            this.pgSettings.TabIndex = 1;
            this.pgSettings.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgSettings_PropertyValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 613);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(237, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Upgrades armor with item upgrades.";
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 659);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pgSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(19, 40);
            this.Name = "FormSettings";
            this.Text = "ItemUpgrader";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PropertyGrid pgSettings;
        private System.Windows.Forms.Label label2;
    }
}