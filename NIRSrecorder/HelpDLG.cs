using System;
using Gtk;
namespace NIRSrecorder
{
    public partial class HelpDLG : Window
    {
        public HelpDLG() :
                base(WindowType.Toplevel)
        {
            Build();

        }

        protected void ClickLink(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://www.nirsoptix.com/");
        }

        protected void CloseDLG(object sender, EventArgs e)
        {
            Dispose();
            Destroy();
        }
    }
}
