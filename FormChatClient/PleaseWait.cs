
// Copyright (C) 2006 by Nikola Paljetak

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NikeSoftChat
{
    public partial class PleaseWaitDialog : Form
    {
        public PleaseWaitDialog()
        {
            InitializeComponent();
        }

        public void ShowError(string errMessage)
        {
            this.lblErrorMessage.Text = errMessage;
            this.pnlError.BringToFront();
            this.Text = "Connection error";
            pnlConnecting.Visible = false;
            pnlError.Visible = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}