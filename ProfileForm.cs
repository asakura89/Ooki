using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aku;

namespace Ooki
{
    public class ProfileForm : AkuForm
    {
        readonly Action<String> SelectedChangedAction;

        public ProfileForm()
        {
            InitializeComponent();
        }

        public ProfileForm(IList<Size> profiles, Action<String> selectedChangedAction) : this()
        {
            SelectedChangedAction = selectedChangedAction;

            lstProfile.SelectedValueChanged -= lstProfileOnSelectedValueChanged;
            lstProfile.DisplayMember = "Key";
            lstProfile.ValueMember = "Value";
            lstProfile.DataSource = profiles.Select(p => new KeyValuePair<String, String>(p.Width + Constant.XString + p.Height, p.Width + Constant.XString + p.Height)).ToList();
            lstProfile.SelectedValueChanged += lstProfileOnSelectedValueChanged;
        }

        void lstProfileOnSelectedValueChanged(object sender, EventArgs e)
        {
            String selected = lstProfile.SelectedValue.ToString();
            if (!String.IsNullOrEmpty(selected))
            {
                SelectedChangedAction(selected);
                Close();
            }
        }

        #region Windows Form Designer generated code

        private IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private ListBox lstProfile;
        private void InitializeComponent()
        {
            lstProfile = new ListBox();
            SuspendLayout();

            lstProfile.BorderStyle = BorderStyle.None;
            lstProfile.Dock = DockStyle.Fill;
            lstProfile.Name = "lstProfile";

            Controls.Add(lstProfile);
            Name = "ProfileForm";
            ShowInTaskbar = false;
            Text = Constant.AppName;
            ResumeLayout(false);
        }


        #endregion
    }
}
