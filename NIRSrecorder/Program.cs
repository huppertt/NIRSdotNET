using System;
using Gtk;
using System.Windows;
using System.Collections.Generic;

namespace NIRSrecorder
{
    class MainClass
    {

     
        public static MainWindow win;
        public static Splash obj_Splash;
        public static List<NIRSDAQ.Instrument.instrument> devices;


        public static void Main(string[] args)
        {
            Application.Init();

            obj_Splash = new Splash();
            obj_Splash.Show();
            obj_Splash.Opacity = 8;
            obj_Splash.Decorated = false;

            
            GLib.Timeout.Add(5000, delegate
            {
                win = new MainWindow();
                win.ShowAll();
                obj_Splash.Hide();
                return false;
            });

        Application.Run();
        }
    }
}
