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

namespace ChecSumAutoBoot
{
    public partial class CheckSumAutoBoot : Form
    {

        public CheckSumAutoBoot()
        {
            InitializeComponent();
            this.KeyPreview = true; // Tuş vuruşlarını formda yakala
            this.WindowState = FormWindowState.Maximized; // Formu tam ekran yap
            this.FormBorderStyle = FormBorderStyle.None; // Kenarlıkları kaldır

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartProcess();
            DisableStartMenu();
            HideSystemFolders();
            DisableRightClick();
            DisableTaskManager();
            DisableControlPanel();
            DisableRecycleBin();
            RestartExplorer();

        }



        private async void StartProcess()
        {
            await Task.Run(() => RunProcess(progressBar1, "RegEdit Install", 1000));
            await Task.Run(() => RunProcess(progressBar2, "DataLine Check", 1500));
            await Task.Run(() => RunProcess(progressBar3, "Internet Check", 2000));
            CheckAndLogInternet();
            // Başka bir uygulamayı çalıştır
            Process.Start("C:\\Program Files (x86)\\Electrum");

            Application.Exit();

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
                RestoreStartMenu();
                ShowSystemFolders();
                EnableRightClick();
                RestartExplorer();  // Değişikliklerin etkili olması için Explorer'ı yeniden başlatın
                MessageBox.Show("Tüm değişiklikler geri alındı.");
            }

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
            string logFilePath = @"C:\Users\Arif\source\repos\ChecSumAutoBoot\ChecSumAutoBoot\Checksum.txt";
            File.AppendAllText(logFilePath, status.ToString() + Environment.NewLine);
        }

        private void CheckAndLogInternet()
        {
            int status = CheckInternetConnection() ? 1 : 0;
            LogStatus(status);

            if (status == 1)
                DisableActiveNetwork();
        }
    }


}
