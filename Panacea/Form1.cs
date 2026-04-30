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
    public partial class Form1 : Form, IMessageFilter
    {
        // --- VARIABLES DE ACTUALIZACIÓN Y CONTROL ---
        private string versionLocal = "1.1";
        private string urlVersionGit = "https://gist.githubusercontent.com/TecnologiaGolemanIps/b3da2a70053ad080ac4e68458e40e829/raw/3b29f811dace96ccb79fe277943b5b4628df4375/version.txt";
        private string urlDescargaNueva = "https://github.com/TecnologiaGolemanIps/Navegador_Panacea/releases/latest";

        private int currentZoom = 100;
        private Label lblPercent;
        private TrackBar tbZoom;
        private object axInstance;
        private SHDocVw.DWebBrowserEvents2_NewWindow2EventHandler newWindow2Handler;
        private volatile bool isShuttingDown = false;
        private string urlPanacea = "http://181.51.196.194/panacea";
        private string urlSilver32 = "https://github.com/TecnologiaGolemanIps/Silver_panacea/raw/main/Silverlight_x32.exe";
        private string urlSilver64 = "https://github.com/TecnologiaGolemanIps/Silver_panacea/raw/main/Silverlight_x64-5.1.50918.0.exe";

        public Form1()
        {
            FijarEmulacionNavegador();
            InitializeComponent();
            CrearBarraHerramientasTempest();

            this.Text = "Panacea - Ips Goleman";
            webBrowser1.ScriptErrorsSuppressed = true;

            // REGISTRAMOS EL FILTRO PARA CAPTURAR TECLAS EN TODA LA APP
            Application.AddMessageFilter(this);

            this.Load += (s, e) => {
                try
                {
                    axInstance = webBrowser1.ActiveXInstance;
                    if (axInstance != null)
                    {
                        newWindow2Handler = new SHDocVw.DWebBrowserEvents2_NewWindow2EventHandler(Ax_NewWindow2);
                        ((SHDocVw.WebBrowser)axInstance).NewWindow2 += newWindow2Handler;
                    }
                    this.FormClosed += Form1_FormClosed;
                }
                catch { }
            };
        }

        // --- 🔥 FILTRO GLOBAL DE TECLADO (COMANDOS DE EMERGENCIA) ---
        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            if (m.Msg == WM_KEYDOWN)
            {
                Keys keyCode = (Keys)m.WParam & Keys.KeyCode;
                bool control = (ModifierKeys & Keys.Control) == Keys.Control;
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

                // CTRL + R: Reinicio Forzado Manual (Evita el cierre sin reinicio)
                if (control && keyCode == Keys.R)
                {
                    ReiniciarAplicacionManual();
                    return true;
                }
                // CTRL + SHIFT + X: Cierre Atómico
                if (control && shift && keyCode == Keys.X)
                {
                    TerminarAppLimpio(false);
                    return true;
                }
            }
            return false;
        }

        // MÉTODO PARA REINICIAR MANUALMENTE (Más robusto que Application.Restart)
        private void ReiniciarAplicacionManual()
        {
            try
            {
                // Lanzamos una nueva instancia del ejecutable actual
                Process.Start(Application.ExecutablePath);
                // Cerramos la instancia actual de raíz
                TerminarAppLimpio(false);
            }
            catch
            {
                Application.Restart(); // Fallback por si falla el inicio manual
            }
        }

        private void TerminarAppLimpio(bool restart)
        {
            if (isShuttingDown) return;
            isShuttingDown = true;

            Application.RemoveMessageFilter(this);

            try
            {
                if (webBrowser1 != null)
                {
                    webBrowser1.Stop();
                    webBrowser1.Navigate("about:blank");
                    webBrowser1.Dispose();
                }
            }
            catch { }

            if (restart) Application.Restart();
            else Process.GetCurrentProcess().Kill();
        }

        private void Ax_NewWindow2(ref object ppDisp, ref bool Cancel)
        {
            object newDisp = null;
            try
            {
                this.Invoke((Action)(() => {
                    Form1 nueva = new Form1();
                    nueva.Tag = "Secundaria";
                    nueva.Show();
                    try { newDisp = nueva.webBrowser1.ActiveXInstance; } catch { newDisp = null; }
                }));

                if (newDisp != null) ppDisp = newDisp;
            }
            catch
            {
                Cancel = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isShuttingDown) TerminarAppLimpio(false);
        }

        private void CrearBarraHerramientasTempest()
        {
            Panel panelZoom = new Panel();
            panelZoom.Dock = DockStyle.Bottom;
            panelZoom.Height = 45;
            panelZoom.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
            panelZoom.Padding = new Padding(10, 5, 10, 5);
            panelZoom.AutoSize = false;
            panelZoom.BorderStyle = BorderStyle.FixedSingle;

            int xPos = 10;
            int yPos = 5;
            int btnHeight = 30;

            Label lblZoomLabel = new Label();
            lblZoomLabel.Text = "📏 Zoom:";
            lblZoomLabel.ForeColor = System.Drawing.Color.Cyan;
            lblZoomLabel.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            lblZoomLabel.AutoSize = true;
            lblZoomLabel.Location = new System.Drawing.Point(xPos, yPos);
            lblZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelZoom.Controls.Add(lblZoomLabel);
            xPos += lblZoomLabel.Width + 10;

            Button bMenos = new Button();
            bMenos.Text = "−";
            bMenos.Width = 35;
            bMenos.Height = btnHeight;
            bMenos.Location = new System.Drawing.Point(xPos, yPos);
            bMenos.BackColor = System.Drawing.Color.FromArgb(60, 90, 150);
            bMenos.ForeColor = System.Drawing.Color.White;
            bMenos.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            bMenos.FlatStyle = FlatStyle.Flat;
            bMenos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 120, 200);
            bMenos.Click += (s, e) => { SetZoom(Math.Max(10, currentZoom - 10)); };
            panelZoom.Controls.Add(bMenos);
            xPos += 40;

            tbZoom = new TrackBar();
            tbZoom.Minimum = 10;
            tbZoom.Maximum = 500;
            tbZoom.Value = currentZoom;
            tbZoom.TickStyle = TickStyle.None;
            tbZoom.Width = 180;
            tbZoom.Height = btnHeight;
            tbZoom.Location = new System.Drawing.Point(xPos, yPos);
            tbZoom.Scroll += (s, e) => { SetZoom(tbZoom.Value); };
            panelZoom.Controls.Add(tbZoom);
            xPos += 185;

            Button bMas = new Button();
            bMas.Text = "+";
            bMas.Width = 35;
            bMas.Height = btnHeight;
            bMas.Location = new System.Drawing.Point(xPos, yPos);
            bMas.BackColor = System.Drawing.Color.FromArgb(60, 90, 150);
            bMas.ForeColor = System.Drawing.Color.White;
            bMas.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            bMas.FlatStyle = FlatStyle.Flat;
            bMas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 120, 200);
            bMas.Click += (s, e) => { SetZoom(Math.Min(500, currentZoom + 10)); };
            panelZoom.Controls.Add(bMas);
            xPos += 40;

            Button b100 = new Button();
            b100.Text = "100%";
            b100.Width = 50;
            b100.Height = btnHeight;
            b100.Location = new System.Drawing.Point(xPos, yPos);
            b100.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            b100.ForeColor = System.Drawing.Color.White;
            b100.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            b100.FlatStyle = FlatStyle.Flat;
            b100.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(150, 150, 150);
            b100.Click += (s, e) => { SetZoom(100); };
            panelZoom.Controls.Add(b100);
            xPos += 55;

            lblPercent = new Label();
            lblPercent.Text = $"{currentZoom}%";
            lblPercent.ForeColor = System.Drawing.Color.LimeGreen;
            lblPercent.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            lblPercent.AutoSize = true;
            lblPercent.Location = new System.Drawing.Point(xPos, yPos + 5);
            lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            panelZoom.Controls.Add(lblPercent);
            xPos += 50;

            Label separator = new Label();
            separator.Text = "|";
            separator.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            separator.Font = new System.Drawing.Font("Segoe UI", 12);
            separator.AutoSize = true;
            separator.Location = new System.Drawing.Point(xPos + 5, yPos);
            panelZoom.Controls.Add(separator);
            xPos += 20;

            Button btnLimpia = new Button();
            btnLimpia.Text = "🔄 Limpiar Caché";
            btnLimpia.Width = 140;
            btnLimpia.Height = btnHeight;
            btnLimpia.Location = new System.Drawing.Point(xPos, yPos);
            btnLimpia.BackColor = System.Drawing.Color.FromArgb(150, 120, 0);
            btnLimpia.ForeColor = System.Drawing.Color.White;
            btnLimpia.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            btnLimpia.FlatStyle = FlatStyle.Flat;
            btnLimpia.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 180, 0);
            btnLimpia.Click += (s, e) => LimpiarCacheIE();
            panelZoom.Controls.Add(btnLimpia);

            this.Controls.Add(panelZoom);
            SetZoom(currentZoom);
        }

        private void SetZoom(int percent)
        {
            if (isShuttingDown) return;
            currentZoom = Math.Max(10, Math.Min(500, percent));
            try
            {
                if (lblPercent != null) lblPercent.Text = $"{currentZoom}%";
                if (tbZoom != null && tbZoom.Value != currentZoom) tbZoom.Value = currentZoom;

                try
                {
                    var ax = webBrowser1?.ActiveXInstance as SHDocVw.WebBrowser;
                    if (ax != null)
                    {
                        object pvaIn = currentZoom;
                        ax.ExecWB((SHDocVw.OLECMDID)63, (SHDocVw.OLECMDEXECOPT)2, ref pvaIn, IntPtr.Zero);
                        return;
                    }
                }
                catch { }
            }
            catch { }
        }

        private void LimpiarCacheIE()
        {
            if (MessageBox.Show("¿Limpiar temporales y reiniciar?", "Tempest", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    Process.Start("RunDll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 255");
                    ReiniciarAplicacionManual();
                }
                catch { }
            }
        }

        private void FijarEmulacionNavegador()
        {
            try
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                    key.SetValue(appName, 7000, RegistryValueKind.DWord);

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Styles"))
                    key.SetValue("MaxScriptStatements", -1, RegistryValueKind.DWord);
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.Tag?.ToString() != "Secundaria") VerificarYArrancar();
        }

        private void VerificarYArrancar()
        {
            ChequearVersionForzosa();
            if (!EstaSilverlightInstalado())
            {
                if (MessageBox.Show("Instalando soporte Tempest...", "Soporte", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    DescargarEInstalarSilencioso();
            }
            else webBrowser1.Navigate(urlPanacea);
        }

        private void ChequearVersionForzosa()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    string versionRemota = client.DownloadString(urlVersionGit).Trim();

                    if (versionRemota != versionLocal)
                    {
                        string msg = "⚠️ ACTUALIZACIÓN REQUERIDA (v" + versionRemota + ")\n\nDebes instalar la versión más reciente para continuar.";
                        if (MessageBox.Show(msg, "Tempest Security", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            Process.Start(urlDescargaNueva);
                            TerminarAppLimpio(false);
                        }
                    }
                }
            }
            catch { }
        }

        private bool EstaSilverlightInstalado()
        {
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Silverlight")) return k != null;
        }

        private void DescargarEInstalarSilencioso()
        {
            try
            {
                string urlFinal = Environment.Is64BitOperatingSystem ? urlSilver64 : urlSilver32;
                string temp = Path.Combine(Path.GetTempPath(), "Silverlight_Setup.exe");
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.DownloadFile(urlFinal, temp);
                }
                Process.Start(temp, "/q").WaitForExit();
                ReiniciarAplicacionManual();
            }
            catch { }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (!isShuttingDown && webBrowser1 != null && e.Url == webBrowser1.Url) SetZoom(currentZoom);
            }
            catch { }
        }
    }
}