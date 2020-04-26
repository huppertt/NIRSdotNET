using System;
using System.IO.Ports;
using NIRSDAQ;
using System.Collections.Generic;

namespace NIRSrecorder
{
    public partial class PrefrencesWindow : Gtk.Window
    {
        NIRSDAQ.Instrument.Devices.TechEn.BTnirs device;
        public PrefrencesWindow() :
                base(Gtk.WindowType.Toplevel)
        {

            this.Build();
            device = new Instrument.Devices.TechEn.BTnirs();
            // Get a list of serial port names.
            List<string> ports = device.ListPorts();

            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            combobox_selectSerial.Model = ClearList;
             foreach (string s in ports)
                {
                   combobox_selectSerial.AppendText(s);
                }
                combobox_selectSerial.Active = 0;

         
        }

        protected void ClickedTestCOM(object sender, EventArgs e)
        {

            // Create a new SerialPort object with default settings.
            bool status = device.Connect(combobox_selectSerial.ActiveText);
            if (status)
            {
                textview_Debug.Buffer.Text += "\nOpened Port";
            }else
            {
                textview_Debug.Buffer.Text += "\nError.  Unable to open port";
            }




        }

      


        protected void EditComboChanged(object sender, EventArgs e)
        {
            device.FlushBuffer();

            string cmd = comboboxentry_debug.ActiveText;
            if(cmd.Contains("Lasers All On"))
            {
                device.AllOn();
            }
            else if (cmd.Contains("Lasers All Off"))
            {
                device.AllOff();
            }
            else if (cmd.Contains("Laser 1 On"))
            {
                device.SetLaserState(0,true);
                device.SetLaserState(1, true);
            }
            else if (cmd.Contains("Laser 1 Off"))
            {
                device.SetLaserState(0, false);
                device.SetLaserState(1, false);
            }
            else if (cmd.Contains("Laser 2 On"))
            {
                device.SetLaserState(2, true);
                device.SetLaserState(3, true);
            }
            else if (cmd.Contains("Laser 2 Off"))
            {
                device.SetLaserState(2, false);
                device.SetLaserState(3, false);
            }
            else if (cmd.Contains("Laser 3 On"))
            {
                device.SetLaserState(4, true);
                device.SetLaserState(5, true);
            }
            else if (cmd.Contains("Laser 3 Off"))
            {
                device.SetLaserState(4, false);
                device.SetLaserState(5, false);
            }
            else if (cmd.Contains("Laser 4 On"))
            {
                device.SetLaserState(6, true);
                device.SetLaserState(7, true);
            }
            else if (cmd.Contains("Laser 4 Off"))
            {
                device.SetLaserState(6, false);
                device.SetLaserState(7, false);
            }
            else
            {
                device.SendCommMsg(cmd);
            }

            string msg = device.ReadCommMsg();

            textview_Debug.Buffer.Text += "\nMSG Sent " + cmd;
            if (msg != null)
            {
                textview_Debug.Buffer.Text += "\n" + msg;
            }
            



        }
    }
}
