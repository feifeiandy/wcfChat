
// Copyright (C) 2006 by Nikola Paljetak

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceModel;


namespace NikeSoftChat
{
    public partial class ChatForm : Form, IChatCallback
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        //The WM_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar. 
        //This message is also sent to the owner of a vertical scroll bar control when a scroll event occurs in the control.
        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        private int lastSelectedIndex = -1;

        private ChatProxy proxy;
        private string myNick;

        private PleaseWaitDialog pwDlg;
        private delegate void HandleDelegate(string[] list);
        private delegate void HandleErrorDelegate();

        public ChatForm()
        {
            InitializeComponent();
            ShowConnectMenuItem(true);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstChatters.Items.Clear();
            NickDialog nickDlg = new NickDialog();
            if (nickDlg.ShowDialog() == DialogResult.OK)
            {
                myNick = nickDlg.txtNick.Text;
                nickDlg.Close();
            }

            txtMessage.Focus();
            Application.DoEvents();
            InstanceContext site = new InstanceContext(this);
            proxy = new ChatProxy(site);

            IAsyncResult iar = proxy.BeginJoin(myNick, new AsyncCallback((thisIar) =>
            {
                try
                {
                    string[] list = proxy.EndJoin(thisIar);

                    pwDlg.Invoke((Action)(() =>
                    {
                        if (list == null)
                        {
                            pwDlg.ShowError("Error: Username already exist!");
                            ExitChatSession();
                        }
                        else
                        {
                            pwDlg.Close();
                            ShowConnectMenuItem(false);
                            foreach (string name in list)
                            {
                                lstChatters.Items.Add(name);
                            }
                            AppendText("Connected at " + DateTime.Now.ToString() + " with user name " + myNick + Environment.NewLine);
                        }
                    }));

                }
                catch (Exception ex)
                {
                    pwDlg.Invoke((Action)(() =>
                    {
                        pwDlg.ShowError("Error: Cannot connect to chat!");
                        ExitChatSession();
                    }));
                }
            }), null);

            pwDlg = new PleaseWaitDialog();
            pwDlg.ShowDialog();

        }

        private void SayAndClear(string to, string msg, bool pvt)
        {
            if (msg != "")
            {
                try
                {
                    CommunicationState cs = proxy.State;
                    if (!pvt)
                        proxy.Say(msg);
                    else
                        proxy.Whisper(to, msg);

                    txtMessage.Text = "";
                }
                catch
                {
                    AbortProxyAndUpdateUI();
                    AppendText("Disconnected at " + DateTime.Now.ToString() + Environment.NewLine);
                    Error("Error: Connection to chat server lost!");
                }
            }
        }


        private void Error(string errMessage)
        {
            MessageBox.Show(errMessage, "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitChatSession();
        }

        private void btnSay_Click(object sender, EventArgs e)
        {
            SayAndClear("", txtMessage.Text, false);
            txtMessage.Focus();
        }

        private void btnWhisper_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text == "")
                return;

            object to = lstChatters.SelectedItem;
            if (to != null)
            {
                string receiverName = (string)to;
                AppendText("Whisper to " + receiverName + ": " + txtMessage.Text + Environment.NewLine);
                SayAndClear(receiverName, txtMessage.Text, true);
                txtMessage.Focus();
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitChatSession();
            btnWhisper.Enabled = false;
            AppendText("Disconnected at " + DateTime.Now.ToString() + Environment.NewLine);
        }

        public void Receive(string senderName, string message)
        {
            AppendText(senderName + ": " + message + Environment.NewLine);
        }

        public void ReceiveWhisper(string senderName, string message)
        {
            AppendText(senderName + " whisper: " + message + Environment.NewLine);
        }

        public void UserEnter(string name)
        {
            AppendText("User " + name + " enter at " + DateTime.Now.ToString() + Environment.NewLine);
            lstChatters.Items.Add(name);
        }

        public void UserLeave(string name)
        {
            AppendText("User " + name + " leave at " + DateTime.Now.ToString() + Environment.NewLine);
            lstChatters.Items.Remove(name);
            AdjustWhisperButton();
        }

        private void AppendText(string text)
        {
            txtChatText.Text += text;
            SendMessage(txtChatText.Handle, WM_VSCROLL, SB_BOTTOM, new IntPtr(0));
        }

        private void ShowConnectMenuItem(bool show)
        {
            connectToolStripMenuItem.Enabled = show;
            disconnectToolStripMenuItem.Enabled = btnSay.Enabled = !show;
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && btnSay.Enabled)
            {
                SayAndClear("", txtMessage.Text, false);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitChatSession();
            ExitApplication();
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ExitChatSession();
            ExitApplication();
        }

        private void ExitChatSession()
        {
            try
            {
                proxy.Leave();
            }
            catch { }
            finally
            {
                AbortProxyAndUpdateUI();
            }
        }

        private void AbortProxyAndUpdateUI()
        {
            if (proxy != null)
            {
                proxy.Abort();
                proxy.Close();
                proxy = null;
            }
            ShowConnectMenuItem(true);
        }


        private void ExitApplication()
        {
            Application.Exit();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                btnSay.PerformClick();
            }
        }

        private void lstChatters_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdjustWhisperButton();
        }

        private void AdjustWhisperButton()
        {
            if (lstChatters.SelectedIndex == lastSelectedIndex)
            {
                lstChatters.SelectedIndex = -1;
                lastSelectedIndex = -1;
                btnWhisper.Enabled = false;
            }
            else
            {
                btnWhisper.Enabled = true;
                lastSelectedIndex = lstChatters.SelectedIndex;
            }
            txtMessage.Focus();
        }

        private void ChatForm_Resize(object sender, EventArgs e)
        {
            SendMessage(txtChatText.Handle, WM_VSCROLL, SB_BOTTOM, new IntPtr(0));
        }


    }
}