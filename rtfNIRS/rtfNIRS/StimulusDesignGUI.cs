using System;
using Gtk;
namespace rtfNIRS
{
    public partial class StimulusDesignGUI : Gtk.Dialog
    {
        public nirs.core.CSession session;
        public Gtk.ListStore ListStore;
        public StimulusDesignGUI()
        {
            this.Build();
            session= MainClass.win.Session;

            Gtk.TreeViewColumn column1 = new TreeViewColumn();
            column1.Title = "Condition";
            Gtk.CellRendererText column1text = new CellRendererText();
            column1.PackStart(column1text, true);
            column1.AddAttribute(column1text, "text", 0);

            Gtk.TreeViewColumn column2 = new TreeViewColumn();
            column2.Title = "Onset";
            Gtk.CellRendererText column2text = new CellRendererText();
            column2.PackStart(column2text, true);
            column2.AddAttribute(column2text, "text", 0);

            Gtk.TreeViewColumn column3 = new TreeViewColumn();
            column3.Title = "Duration";
            Gtk.CellRendererText column3text = new CellRendererText();
            column3.PackStart(column3text, true);
            column3.AddAttribute(column3text, "text", 0);

            Gtk.TreeViewColumn column4 = new TreeViewColumn();
            column4.Title = "Amplitude";
            Gtk.CellRendererText column4text = new CellRendererText();
            column4.PackStart(column4text, true);
            column4.AddAttribute(column4text, "text", 0);

            treeview1.AppendColumn(column1);
            treeview1.AppendColumn(column2);
            treeview1.AppendColumn(column3);
            treeview1.AppendColumn(column4);

            ListStore = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));
            treeview1.Model = ListStore;
            treeview1.ShowAll();


        }

    }
}
