using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CRSoftware
{
    public partial class MainWindow : Window
    {
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), "CRSoftware_Temp");
        private string selectedLanguage = "it"; // Default: Italiano

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
        }

        private async void BtnInstallWin11_Click(object sender, RoutedEventArgs e)
        {
            string win11Url = selectedLanguage switch
            {
                "it" => "https://software-download.microsoft.com/db/Win11_23H2_Italian_x64.iso",
                "en" => "https://software-download.microsoft.com/db/Win11_23H2_English_x64.iso",
                "es" => "https://software-download.microsoft.com/db/Win11_23H2_Spanish_x64.iso",
                "fr" => "https://software-download.microsoft.com/db/Win11_23H2_French_x64.iso",
                "de" => "https://software-download.microsoft.com/db/Win11_23H2_German_x64.iso",
                _ => "https://software-download.microsoft.com/db/Win11_23H2_Italian_x64.iso"
            };

            string langName = selectedLanguage switch
            {
                "it" => "Italiano",
                "en" => "Inglese",
                "es" => "Spagnolo",
                "fr" => "Francese",
                "de" => "Tedesco",
                _ => "Italiano"
            };

            await StartInstallation($"Windows 11 ({langName})", win11Url);
        }

        private async void BtnInstallLinux_Click(object sender, RoutedEventArgs e)
        {
            var selected = CmbLinuxDistro.SelectedItem as ComboBoxItem;
            if (selected == null)
            {
                MessageBox.Show("Seleziona una distribuzione Linux.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string distro = (selected.Tag as string) ?? "ubuntu";
            string url = distro switch
            {
                "ubuntu" => "https://releases.ubuntu.com/24.04/ubuntu-24.04-desktop-amd64.iso",
                "kubuntu" => "https://cdimage.ubuntu.com/kubuntu/releases/24.04/release/kubuntu-24.04-desktop-amd64.iso",
                "xubuntu" => "https://cdimage.ubuntu.com/xubuntu/releases/24.04/release/xubuntu-24.04-desktop-amd64.iso",
                "linuxmint" => "https://mirrors.layeronline.com/linuxmint/stable/21.3/linuxmint-21.3-cinnamon-64bit.iso",
                "fedora" => "https://download.fedoraproject.org/pub/fedora/linux/releases/40/Workstation/x86_64/iso/Fedora-Workstation-Live-x86_64-40-1.14.iso",
                "debian" => "https://cdimage.debian.org/debian-cd/current/amd64/iso-dvd/debian-12.6.0-amd64-DVD-1.iso",
                _ => "https://releases.ubuntu.com/24.04/ubuntu-24.04-desktop-amd64.iso"
            };

            string langNote = "\n\nℹ️ L'ISO è in inglese, ma potrai selezionare l'italiano durante l'installazione.";
            await StartInstallation($"Linux: {selected.Content}{langNote}", url);
        }

        private async void BtnInstallChromeOS_Click(object sender, RoutedEventArgs e)
        {
            string note = "\n\nℹ️ La lingua verrà impostata automaticamente al primo avvio.";
            await StartInstallation($"ChromeOS Flex{note}", "https://dl.google.com/chromeos/flashbench/ChromeOSFlex.iso");
        }

        private void BtnCreateUSB_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funzionalità USB Multi-Boot in preparazione.\nPer ora, usa Rufus o Ventoy manualmente.", "CRSoftware", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task StartInstallation(string name, string url)
        {
            try
            {
                MessageBox.Show($"Download di {name} in corso...\nQuesto può richiedere diversi minuti.", "CRSoftware", MessageBoxButton.OK, MessageBoxImage.Information);

                string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                string isoPath = Path.Combine(tempDir, $"{safeName}.iso");

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(isoPath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                Process.Start("explorer.exe", tempDir);
                MessageBox.Show($"ISO salvata in:\n{isoPath}", "CRSoftware", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("L'ISO per questa lingua non è disponibile.\nProva con un'altra lingua.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}