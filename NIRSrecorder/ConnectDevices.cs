using System;
using System.Collections.Generic;
using System.Threading;

namespace NIRSrecorder
{
    public partial class ConnectDevices : Gtk.Dialog
    {
        List<string> ports;
        List<string> connected;

        public ConnectDevices()
        {
            this.Build();

            if (MainClass.win.settings.SYSTEM.Trim().ToLower().Equals("simulator"))
            {
                checkbutton_simMode.Active = true;
                vbox_devices.Sensitive = false;
                if ( MainClass.devices == null)
                {
                    spinbutton_numSimDevices.Value = 1;
                }
                else
                {
                    spinbutton_numSimDevices.Value = MainClass.devices.Length;
                }
            }else if (MainClass.win.settings.SYSTEM.Trim().ToLower().Equals("simulatorhyperscan"))
            {
                checkbutton_simMode.Active = true;
                vbox_devices.Sensitive = false;
                spinbutton_numSimDevices.Value = 2;
            }
            else
            {
                checkbutton_simMode.Active = false;
                vbox_devices.Sensitive = true;
                spinbutton_numSimDevices.Value = 1;
            }


            NIRSDAQ.Instrument.Devices.TechEn.BTnirs bTnirs = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
            ports = bTnirs.ListPorts();

            connected = new List<string>();
            if (MainClass.devices != null)
            {
                for (int i = 0; i < MainClass.devices.Length; i++)
                {

                    NIRSDAQ.info info = MainClass.devices[i].GetInfo();

                    connected.Add(info.PortName);

                    if (ports.Contains(info.PortName))
                    {
                        ports.Remove(info.PortName);
                    }

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

            if (combobox_connected.Active < 0) { return; }

            string port = combobox_connected.ActiveText;
            try
            {

                bool found = false;
                for (int i = 0; i < MainClass.devices.Length; i++)
                {
                    NIRSDAQ.info info = MainClass.devices[i].GetInfo();
                    if(port == info.PortName)
                    {
                        MainClass.devices[i].IDmode(true);
                        Thread.Sleep(3000);
                        MainClass.devices[i].IDmode(false);
                        found = true;
                    }
                }
                if(!found)
                {
                    NIRSDAQ.Instrument.Devices.TechEn.BTnirs bTnirs = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
                    bTnirs.Connect(port);
                    bTnirs.IDmode(true);
                    Thread.Sleep(3000);
                    bTnirs.IDmode(false);
                    bTnirs.Destroy();
                }
            }
            catch { }


        }

        protected void IDdiscont(object sender, EventArgs e)
        {

            if (combobox_avail.Active < 0) { return; }

            string port = combobox_avail.ActiveText;
            try
            {

                bool found = false;
                for (int i = 0; i < MainClass.devices.Length; i++)
                {
                    NIRSDAQ.info info = MainClass.devices[i].GetInfo();
                    if (port == info.PortName)
                    {
                        MainClass.devices[i].IDmode(true);
                        Thread.Sleep(3000);
                        MainClass.devices[i].IDmode(false);
                        found = true;
                    }
                }
                if (!found)
                {
                    NIRSDAQ.Instrument.Devices.TechEn.BTnirs bTnirs = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
                    bTnirs.Connect(port);
                    bTnirs.IDmode(true);
                    Thread.Sleep(3000);
                    bTnirs.IDmode(false);
                    bTnirs.Destroy();
                }
            }
            catch { }
        }

        protected void ClickedOK(object sender, EventArgs e)
        {

            if (checkbutton_simMode.Active)
            {

                List<string> simdev = new List<string>();
                MainClass.win.settings.SYSTEM = "Simulator";
                for (int i = 0; i < spinbutton_numSimDevices.Value; i++)
                {
                    simdev.Add(string.Format("{0}",i+1));
                }
                MainClass.win.SetupGUI(simdev);

            }
            else
            {
                MainClass.win.SetupGUI(connected);
            }

            if(MainClass.win.nirsdata.Count > 0)
            {
                MainClass.win.RegisterQuickStart(sender, e);
            }
            Dispose();
            Destroy();
        }



        protected void ToggleUseSim(object sender, EventArgs e)
        {
            if (checkbutton_simMode.Active)
            {
                vbox_devices.Sensitive = false;
            }
            else
            {
                vbox_devices.Sensitive = true;
            }

        }
    }
}