using System;
using System.Collections.Generic;


namespace NIRSrecorder
{
    public partial class ConnectDevices : Gtk.Dialog
    {
        List<string> ports;
        List<string> connected;

        public ConnectDevices()
        {
            this.Build();

            NIRSDAQ.Instrument.Devices.TechEn.BTnirs bTnirs = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
            ports = bTnirs.ListPorts();

            connected = new List<string>();
            for(int i=0;i<MainClass.devices.Length; i++)
            {

                NIRSDAQ.info info = MainClass.devices[i].GetInfo();

                connected.Add(MainClass.devices[i].devicename);

                if (ports.Contains(info.PortName)){
                    ports.Remove(info.PortName);
                }

            }

            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            combobox_connected.Model=ClearList;
            foreach (string s in connected)
            {
                combobox_connected.AppendText(s);
            }
           
            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            combobox_avail.Model = ClearList2;
            foreach (string s in ports)
            {
                combobox_avail.AppendText(s);
            }
            combobox_avail.Active = ports.Count - 1;
            combobox_connected.Active = connected.Count - 1;
        }



        protected void CloseDlg(object sender, EventArgs e)
        {
            Dispose();
            Destroy();
        }

        protected void Discont(object sender, EventArgs e)
        {
            if (combobox_connected.Active > -1)
            {
                string str = combobox_connected.ActiveText;
                ports.Add(str);
                connected.Remove(str);

            }
            else
            {
                return;
            }
            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            combobox_connected.Model = ClearList;
            foreach (string s in connected)
            {
                combobox_connected.AppendText(s);
            }
            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            combobox_avail.Model = ClearList2;
            foreach (string s in ports)
            {
                combobox_avail.AppendText(s);
            }
            combobox_avail.Active = ports.Count - 1;
            combobox_connected.Active = connected.Count - 1;
            ShowAll();

        }

        protected void Cont(object sender, EventArgs e)
        {
            if (combobox_avail.Active > -1)
            {
                string str = combobox_avail.ActiveText;
                ports.Remove(str);
                connected.Add(str);

            }
            else
            {
                return;
            }
            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            combobox_connected.Model = ClearList;
            foreach (string s in connected)
            {
                combobox_connected.AppendText(s);
            }
            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            combobox_avail.Model = ClearList2;
            foreach (string s in ports)
            {
                combobox_avail.AppendText(s);
            }
            combobox_avail.Active = ports.Count - 1;
            combobox_connected.Active = connected.Count - 1;
            ShowAll();

        }

        protected void IDcont(object sender, EventArgs e)
        {
        }

        protected void IDdiscont(object sender, EventArgs e)
        {
        }

        protected void ClickedOK(object sender, EventArgs e)
        {
            MainClass.win.SetupGUI(connected);

            Dispose();
            Destroy();
        }
    }
}