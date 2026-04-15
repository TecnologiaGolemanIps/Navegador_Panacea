using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace Panacea
{
    [ComVisible(true)]
    public partial class Form1 : Form
    {
        private string urlPanacea = "http://181.51.196.194/panacea";
        private string urlDescarga = "https://rossana-philhellenic-unpredictably.ngrok-free.dev";

        public Form1()
        {
            FijarEmulacionNavegador();
            InitializeComponent();

            webBrowser1.ScriptErrorsSuppressed = true;

            // 🔥 REGISTRO SEGURO DEL EVENTO
            this.Load += (s, e) => {
                try
                {
                    // Obtenemos la instancia ActiveX
                    SHDocVw.WebBrowser ax = (SHDocVw.WebBrowser)webBrowser1.ActiveXInstance;

                    // IMPORTANTE: Usamos el constructor del delegado oficial para evitar la línea roja
                    ax.NewWindow2 += new SHDocVw.DWebBrowserEvents2_NewWindow2EventHandler(Ax_NewWindow2);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error ActiveX: " + ex.Message);
                }
            };
        }

        // 🔥 MÉTODO DEL EVENTO (Separado para que compile con tipos incrustados)
        private void Ax_NewWindow2(ref object ppDisp, ref bool Cancel)
        {
            Form1 nueva = new Form1();
            nueva.Tag = "Secundaria";
            nueva.Show();

            // Pasamos el motor de navegación a la nueva ventana
            ppDisp = nueva.webBrowser1.ActiveXInstance;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.Tag?.ToString() != "Secundaria")
            {
                VerificarYArrancar();
            }
        }

        private void VerificarYArrancar()
        {
            if (!EstaSilverlightInstalado())
            {
                if (MessageBox.Show("¿Instalar soporte de Silverlight?", "Soporte Tempest", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    DescargarEInstalarSilencioso();
            }
            else { webBrowser1.Navigate(urlPanacea); }
        }

        private void FijarEmulacionNavegador()
        {
            try
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    // Forzamos IE7 (7000) para el diseño de Panacea
                    key.SetValue(appName, 7000, RegistryValueKind.DWord);
                    key.SetValue(appName.Replace(".exe", ".vshost.exe"), 7000, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        // Métodos de apoyo
        private bool EstaSilverlightInstalado()
        {
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Silverlight")) return k != null;
        }

        private void DescargarEInstalarSilencioso()
        {
            try
            {
                string temp = Path.Combine(Path.GetTempPath(), "Silverlight_Setup.exe");
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Headers.Add("ngrok-skip-browser-warning", "true");
                    client.DownloadFile(urlDescarga, temp);
                }
                Process.Start(temp, "/q").WaitForExit();
                Application.Restart();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void WebBrowser1_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Vacío por diseño
        }
    }
}