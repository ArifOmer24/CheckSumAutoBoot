using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Linq;
using System.Drawing;

namespace ChecSumAutoBoot
{
    public partial class CheckSumAutoBoot : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public CheckSumAutoBoot()
        {
            InitializeComponent();
            this.TopMost = true;
            this.KeyPreview = true; // Tuş vuruşlarını formda yakala
            this.WindowState = FormWindowState.Maximized; // Formu tam ekran yap
            this.FormBorderStyle = FormBorderStyle.None; // Kenarlıkları kaldır
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#f44336");
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
                if (vkCode == (int)Keys.LWin || vkCode == (int)Keys.RWin) // Disable Windows key
                {
                    return (IntPtr)1; // Block the key press
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
        private void Form1_Resize(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.Location = new Point((this.ClientSize.Width - panel1.Width) / 2, (this.ClientSize.Height - panel1.Height) / 2);
            this.Focus();
            StartProcess();

        }



        private async void StartProcess()
        {
            await Task.Run(() => RunProcess(progressBar1, "RegEdit Install", 1000));
            await Task.Run(() => RunProcess(progressBar2, "DataLine Check", 1500));
            await Task.Run(() => RunProcess(progressBar3, "Internet Check", 2000));
            CheckAndLogInternet();
            // Başka bir uygulamayı çalıştır
            //Process.Start("C:\\Program Files (x86)\\Electrum");

            Screen screenForm = new Screen();
            screenForm.ShowDialog(); // Formu göster
            this.Hide(); // CheckSumAutoBoot formunu gizle

        }



        private void RunProcess(ProgressBar progressBar, string labelText, int duration)
        {
            // Label metnini güncelle
            Invoke((MethodInvoker)delegate
            {
                label5.Text = labelText;
            });

            for (int i = 0; i <= 100; i++)
            {
                // Progress bar değerini güncelle
                Invoke((MethodInvoker)delegate
                {
                    progressBar.Value = i;
                });

                // Bir süre bekleyerek ilerlemeyi simüle et
                Thread.Sleep(duration / 100);
            }

            // İşlem tamamlandıktan sonra label'ı güncelle
            Invoke((MethodInvoker)delegate
            {
                label5.Text = labelText + " done!";
            });
        }







        void DisableStartMenu()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            key.SetValue("NoStartMenu", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void HideSystemFolders()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            key.SetValue("NoDesktop", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void DisableRightClick()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            key.SetValue("NoViewContextMenu", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void DisableTaskManager()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void DisableControlPanel()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            key.SetValue("NoControlPanel", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void DisableRecycleBin()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            key.SetValue("NoRecycleBin", 1, RegistryValueKind.DWord);
            key.Close();
        }

        void RestartExplorer()
        {
            Process[] explorerProcesses = Process.GetProcessesByName("explorer");
            foreach (var process in explorerProcesses)
            {
                process.Kill();
            }

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {

                EnableTaskManager();
                EnableControlPanel();
                EnableRecycleBin();
                ShowSystemFolders();
                EnableRightClick();
                RestartExplorer();  // Değişikliklerin etkili olması için Explorer'ı yeniden başlatın
                MessageBox.Show("Tüm değişiklikler geri alındı.");
                this.Close(); // Close the form
                Application.Exit(); // Uygulamayı tamamen kapat
            }

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID); // Remove the hook
            base.OnFormClosing(e);
        }

        void EnableTaskManager()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
            if (key != null)
            {
                key.DeleteValue("DisableTaskMgr", true);
                key.Close();
            }
        }

        void EnableControlPanel()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
            if (key != null)
            {
                key.DeleteValue("NoControlPanel", true);
                key.Close();
            }
        }

        void EnableRecycleBin()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
            if (key != null)
            {
                key.DeleteValue("NoRecycleBin", true);
                key.Close();
            }
        }

        void ShowSystemFolders()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
            if (key != null)
            {
                key.DeleteValue("NoDesktop", true);
                key.Close();
            }
        }

        void EnableRightClick()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
            if (key != null)
            {
                key.DeleteValue("NoViewContextMenu", true);
                key.Close();
            }
        }
        void RestoreStartMenu()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
            if (key != null)
            {
                key.DeleteValue("NoStartMenu", true);
                key.Close();
            }
        }







        // İnternet işlemleri


        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        private void DisableActiveNetwork()
        {
            var activeNetwork = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                     (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                      n.NetworkInterfaceType == NetworkInterfaceType.Ethernet));

            if (activeNetwork != null)
            {
                // Ağ bağlantısını devre dışı bırak
                DisableNetwork(activeNetwork.Name);
            }
        }
        private void EnableActiveNetwork()
        {
            var activeNetwork = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Down &&
                                     (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                      n.NetworkInterfaceType == NetworkInterfaceType.Ethernet));

            if (activeNetwork == null)
            {
                // Ağ bağlantısını etkinleştir
                EnableNetwork(activeNetwork.Name);
            }
        }


        public static void DisableNetwork(string networkName)
        {
            Process.Start("netsh", $"interface set interface \"{networkName}\" admin=disabled");
        }

        public static void EnableNetwork(string networkName)
        {
            Process.Start("netsh", $"interface set interface \"{networkName}\" admin=enabled");
        }

        public static void LogStatus(int status)
        {
            string logFilePath = GetLogFilePath(); // Log dosyası yolunu al

            // Log dosyasına yaz
            File.AppendAllText(logFilePath, status.ToString() + Environment.NewLine);

        }

        private static string GetLogFilePath()
        {
            string logFileName = "Checksum.txt"; // Log dosyasının adı
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
        }
        private void CheckAndLogInternet()
        {
            int status = CheckInternetConnection() ? 1 : 0;
            LogStatus(status);



            //if (status == 1)
            //    DisableActiveNetwork();
        }


        //güncel 14.10.2024
    }


}
