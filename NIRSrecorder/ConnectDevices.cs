using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net.NetworkInformation;

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