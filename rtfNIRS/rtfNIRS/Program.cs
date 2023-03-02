using System;
using Gtk;

namespace rtfNIRS
{
    class MainClass
    {
        static public MainWindow win;
        public static void Main(string[] args)
        {
            Application.Init();
            win = new MainWindow();
            win.Show();
            win.initialize_GUI();
            Application.Run();
        }
    }
}
