using System;
using Gtk;
namespace NIRSrecorder
{
    public partial class HelpDLG : Gtk.Window
    {
        public HelpDLG() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();

        }

        protected void ClickLink(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://www.nirsoptix.com/");
        }

        protected void CloseDLG(object sender, EventArgs e)
        {
            this.Dispose();
            
        }
    }
}
