using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using System.Linq;

namespace NIRSrecorder
{
    public partial class RegisterSubject : Gtk.Window
    {

        public static nirs.core.Probe probe;

        public RegisterSubject() :
                base(Gtk.WindowType.Toplevel)
        {

            this.Build();
            this.drawingarea1.ExposeEvent += sdgdraw;
            probe = new nirs.core.Probe();

            //this.previewSDG.ExposeEvent += sdgdraw;
            //  DeleteEvent += delegate { this.Dispose(); };

            // Populate the investigator/study/subjid folders
            string rootfolder = MainClass.win.settings.DATADIR;
            string[] investigators = Directory.GetDirectories(rootfolder);
            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));

            combobox_investigators.Model = ClearList;
            foreach (string s in investigators)
            {
                string[] s2 = s.Split(System.IO.Path.DirectorySeparatorChar);
                combobox_investigators.AppendText(s2[s2.Length - 1]);
            }
            combobox_investigators.Active = 0;

            if (MainClass.devices.Count > 1)
            {
                this.checkbutton_hyperscan.Active = true;
                Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
                this.combobox_hyperscanning.Model = ClearList2;
                for(int i=0; i<MainClass.devices.Count; i++)
                {
                    this.combobox_hyperscanning.AppendText(MainClass.devices[i].devicename);
                }
            }
            else
            {
                this.checkbutton_hyperscan.Destroy();
                this.combobox_hyperscanning.Destroy();
                this.label_hypersacan.Destroy();
                this.combobox_hyperscanning.Destroy();
            }


            ShowAll();
        }

        protected void sdgdraw(object sender, EventArgs e)
        {
            if (probe.isregistered)
            {
                probe.draw1020(this.drawingarea1.GdkWindow);
            }
            else
            {
                probe.draw(this.drawingarea1.GdkWindow);
            }
            return;
        }


        protected void InvestigatorChanged(object sender, EventArgs e)
        {
            string rootfolder = MainClass.win.settings.DATADIR;
            string[] investigators = Directory.GetDirectories(rootfolder);
            int i = combobox_investigators.Active;

            if (i > -1 & i < investigators.Length)
            {
                string[] studies = Directory.GetDirectories(investigators[i]);

                Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
                combobox_studies.Model = ClearList;
                if (studies.Length > 0)
                {
                    //this.investigatorlist.Clear ();
                    foreach (string s in studies)
                    {
                        string[] s2 = s.Split(System.IO.Path.DirectorySeparatorChar);
                        combobox_studies.AppendText(s2[s2.Length - 1]);
                    }
                    combobox_studies.Active = 0;
                }
            }
            ShowAll();
        }

        protected void StudyChanged(object sender, EventArgs e)
        {

            string rootfolder = MainClass.win.settings.DATADIR;
            string[] investigators = Directory.GetDirectories(rootfolder);
            int i = combobox_investigators.Active;


            if (i > -1 & i < investigators.Length)
            {
                string[] studies = Directory.GetDirectories(investigators[i]);
                int j = combobox_studies.Active;

                if (j > -1 & j < studies.Length)
                {
                    string[] subjids = Directory.GetDirectories(studies[j]);

                    Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
                    comboboxentry_subjID.Model = ClearList;

                    if (subjids.Length > 0)
                    {
                        //this.investigatorlist.Clear ();
                        foreach (string s in subjids)
                        {
                            string[] s2 = s.Split(System.IO.Path.DirectorySeparatorChar);
                            comboboxentry_subjID.AppendText(s2[s2.Length - 1]);
                        }
                    }
                    comboboxentry_subjID.AppendText("New Subject");
                    comboboxentry_subjID.Active = subjids.Length;

                    string probefile = System.IO.Path.Combine(studies[j], @"Probe.xml");
                    if (System.IO.File.Exists(probefile))
                    {
                        probe = nirs.io.LoadProbe(probefile);
                        sdgdraw(sender, e);
                    }
                    else
                    {
                        probe = new nirs.core.Probe();
                    }

                }

            }
            ShowAll();
        }
        

        protected void LoadProbeClicked(object sender, EventArgs e)
        {
            Gtk.FileChooserDialog fcd = new Gtk.FileChooserDialog("Open File", null, Gtk.FileChooserAction.Open);
            fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
            fcd.AddButton(Gtk.Stock.Open, Gtk.ResponseType.Ok);
            fcd.DefaultResponse = Gtk.ResponseType.Ok;
            fcd.SelectMultiple = false;

            Gtk.FileFilter filter = new Gtk.FileFilter();
            filter.Name = "NIRS probe";
            filter.AddMimeType("XML");
            filter.AddMimeType("Matlab");
            filter.AddPattern("*.SD");
            filter.AddPattern("*.xml");
            fcd.Filter = filter;

            fcd.SetCurrentFolder(MainClass.win.settings.PROBEDIR);

            Gtk.ResponseType response = (Gtk.ResponseType)fcd.Run();
            if (response == Gtk.ResponseType.Ok)
            {
                probe = nirs.io.LoadProbe(fcd.Filename);
                sdgdraw(sender, e);
            }

            fcd.Destroy();



        }

        protected void RegisterAccept(object sender, EventArgs e)
        {

            nirs.core.Data data = new nirs.core.Data();
            data.demographics.set("SubjID", comboboxentry_subjID.ActiveText);
            data.demographics.set("Investigator", combobox_investigators.ActiveText);
            data.demographics.set("Study", combobox_studies.ActiveText);
            data.demographics.set("Gender", demo_gender.ActiveText);
            data.demographics.set("Age", demo_age.Text);
            data.demographics.set("Instrument", MainClass.win.settings.SYSTEM);
            data.demographics.set("head_circumference", demo_headsize.Text);
            data.demographics.set("Technician", demo_tecnician.Text);
            data.demographics.set("comments", demo_comments.Buffer.Text);
            DateTime now = DateTime.Now;
            data.demographics.set("scan_date",now.ToString("F"));
            data.probe = probe;

            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            MainClass.win._handles.whichdata.Model =ClearList;

            List<string> datatypes = new List<string>();
            for(int i=0; i<data.probe.ChannelMap.Length; i++) { 
                datatypes.Add(data.probe.ChannelMap[i].datasubtype);
            }
            datatypes= datatypes.Distinct().ToList();

            foreach (string s in datatypes)
            {
                MainClass.win._handles.whichdata.AppendText(s);
            }

            MainClass.win._handles.whichdata.Active = 0;

            MainClass.win.nirsdata.Add(data);
            MainClass.win._handles.SDGplot.QueueDraw();
            Destroy();
        }

        protected void RegisterCancel(object sender, EventArgs e)
        {
            Destroy();
        }
    }
}
