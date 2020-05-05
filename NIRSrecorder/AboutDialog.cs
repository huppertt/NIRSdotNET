using System;
namespace NIRSrecorder
{
    public partial class AboutDialog : Gtk.Dialog
    {
        public AboutDialog()
        {
            this.Build();
        }

        protected void CLoseDlg(object sender, EventArgs e)
        {
            Dispose();
            Destroy();
        }

        protected void ClickLink(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://www.nirsoptix.com/");
        }
    }
}
