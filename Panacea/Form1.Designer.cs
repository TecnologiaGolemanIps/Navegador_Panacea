namespace Panacea
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1200, 800);

            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.webBrowser1);
            this.Name = "Form1";
            this.Text = "Panacea IPS - Sistema Seguro";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized; // Para que se vea como Edge
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}