using System;
namespace NIRSrecorder
{
    public partial class ConnectDevices : Gtk.Dialog
    {
        public ConnectDevices()
        {
            this.Build();
        }

        protected void CloseDlg(object sender, EventArgs e)
        {
            Dispose();
            Destroy();
        }
    }
}
