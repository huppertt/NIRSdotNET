using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace NIRSrecorder
{
    public partial class RegisterSubject : Gtk.Window
    {

        public nirs.core.Probe probe;
        private nirs.Dictionary[] demo;
        private int device_previous;
        private string probefilename;

        public RegisterSubject() :
                base(Gtk.WindowType.Toplevel)
        {

            this.Build();
            device_previous = 0;
            this.drawingarea1.ExposeEvent += Sdgdraw;
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

            demo = new nirs.Dictionary[MainClass.devices.Length];
            for(int i=0; i<MainClass.devices.Length; i++)
            {
                demo[i] = new nirs.Dictionary();
                demo[i].set("SubjID", comboboxentry_subjID.ActiveText);
                demo[i].set("Investigator", combobox_investigators.ActiveText);
                demo[i].set("Study", combobox_studies.ActiveText);
                demo[i].set("Gender", demo_gender.ActiveText);
                demo[i].set("Group", demo_group.Text);
                demo[i].set("Age", demo_age.Text);
                demo[i].set("Instrument", MainClass.win.settings.SYSTEM);
                demo[i].set("head_circumference", demo_headsize.Text);
                demo[i].set("Technician", demo_tecnician.Text);
                demo[i].set("comments", demo_comments.Buffer.Text);
            }

            

            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            this.combobox_hyperscanning.Model = ClearList2;
            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                this.combobox_hyperscanning.AppendText(MainClass.devices[i].devicename);
            }
            this.combobox_hyperscanning.Active = 0;

            if (MainClass.devices.Length == 1)
            {
                this.combobox_hyperscanning.Destroy();
                this.label_hypersacan.Destroy();
                this.combobox_hyperscanning.Destroy();
            }


            ShowAll();
        }

        protected void Sdgdraw(object sender, EventArgs e)
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
                        probefilename = probefile;
                        Sdgdraw(sender, e);
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

            Gtk.FileFilter filter = new FileFilter
            {
                Name = "NIRS probe"
            };
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
                Sdgdraw(sender, e);
            }
            probefilename = fcd.Filename;
            fcd.Destroy();



        }

        protected void RegisterAccept(object sender, EventArgs e)
        {

            int i = combobox_hyperscanning.Active;

            demo[i].set("SubjID", comboboxentry_subjID.ActiveText);
            demo[i].set("Investigator", combobox_investigators.ActiveText);
            demo[i].set("Study", combobox_studies.ActiveText);
            demo[i].set("Gender", demo_gender.ActiveText);
            demo[i].set("Group", demo_group.Text);
            demo[i].set("Age", demo_age.Text);
            demo[i].set("Instrument", MainClass.win.settings.SYSTEM);
            demo[i].set("head_circumference", demo_headsize.Text);
            demo[i].set("Technician", demo_tecnician.Text);
            demo[i].set("comments", demo_comments.Buffer.Text);

            // Add channels for Optical Density, HbO2, and HbR

            int cnt = probe.ChannelMap.Length;
            nirs.ChannelMap[] ChannelMap = new nirs.ChannelMap[cnt*2+ 2*cnt / probe.numWavelengths];
            for(int ii=0; ii < cnt; ii++)
            {
                ChannelMap[ii] = probe.ChannelMap[ii];
            }
            for (int ii = 0; ii < cnt; ii++)
            {
                ChannelMap[ii+cnt] = probe.ChannelMap[ii];
                ChannelMap[ii+cnt].datasubtype = String.Format("ΔOD {0}nm", ChannelMap[ii].wavelength);
            }
            for (int ii = 0; ii < cnt/probe.numWavelengths; ii++)
            {
                ChannelMap[ii+2*cnt] = probe.ChannelMap[ii];
                ChannelMap[ii+2*cnt].datasubtype = "HbO2";
            }
            for (int ii = cnt / probe.numWavelengths; ii < cnt; ii++)
            {
                ChannelMap[ii + 2 * cnt] = probe.ChannelMap[ii];
                ChannelMap[ii + 2 * cnt].datasubtype = "Hb";
            }
            probe.ChannelMap = ChannelMap;


            for (int dId = 0; dId < MainClass.devices.Length; dId++)
            {

                nirs.core.Data data = new nirs.core.Data();
                data.demographics = demo[dId];
                data.demographics.set("Investigator", combobox_investigators.ActiveText);
                data.demographics.set("Study", combobox_studies.ActiveText);
                data.demographics.set("Instrument", MainClass.win.settings.SYSTEM);
                DateTime now = DateTime.Now;
                data.demographics.set("scan_date", now.ToString("F"));
                data.probe = probe.Clone();

                data.probe.measlistAct = new bool[data.probe.ChannelMap.Length];
                for (int ii = 0; ii < data.probe.ChannelMap.Length; ii++)
                {
                    data.probe.measlistAct[ii] = true;
                }

                Gdk.Color[] cmap = new Gdk.Color[data.probe.ChannelMap.Length];
                for (int ii = 0; ii < data.probe.numChannels; ii++) {
                    cmap[ii]= data.probe.colormap[ii];
                    cmap[data.probe.numChannels + ii] = data.probe.colormap[ii];
                    cmap[data.probe.numChannels*2+ii] = data.probe.colormap[ii];
                }
                data.probe.colormap = cmap;

                Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
                MainClass.win._handles.whichdata.Model = ClearList;

                List<string> datatypes = new List<string>();
                for (int ii = 0; ii < data.probe.ChannelMap.Length; ii++)
                {
                    datatypes.Add(data.probe.ChannelMap[ii].datasubtype);
                }
                datatypes = datatypes.Distinct().ToList();

                foreach (string s in datatypes)
                {
                    MainClass.win._handles.whichdata.AppendText(s);
                }

                MainClass.win._handles.whichdata.Active = 0;

                MainClass.win.nirsdata.Add(data);
            
            }
            // Save the tmp file for quick reload
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = System.IO.Path.Combine(path, "LastSettings.xml");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartElement("settings");
                writer.WriteElementString("probefile", probefilename);
                writer.WriteElementString("Investigator", (string)demo[i].get("Investigator"));
                writer.WriteElementString("Study", (string)demo[i].get("Study"));
                writer.WriteEndElement();
                writer.Flush();
            }

            MainClass.win.EnableControls(true);
            MainClass.win.ShowAll();
            MainClass.win._handles.SDGplot.QueueDraw();
            Destroy();
        }

        protected void RegisterCancel(object sender, EventArgs e)
        {
            Destroy();
        }

        protected void SelectDevice(object sender, EventArgs e)
        {

            int i = device_previous;

            demo[i].set("SubjID", comboboxentry_subjID.ActiveText);
            demo[i].set("Investigator", combobox_investigators.ActiveText);
            demo[i].set("Study", combobox_studies.ActiveText);
            demo[i].set("Gender", demo_gender.ActiveText);
            demo[i].set("Age", demo_age.Text);
            demo[i].set("Instrument", MainClass.win.settings.SYSTEM);
            demo[i].set("head_circumference", demo_headsize.Text);
            demo[i].set("Technician", demo_tecnician.Text);
            demo[i].set("comments", demo_comments.Buffer.Text);
            demo[i].set("Group", demo_group.Text);

            i = combobox_hyperscanning.Active;
            device_previous = i;
            string subjid= (string)demo[i].get("SubjID");
            string gender = (string)demo[i].get("Gender");


            var store = (ListStore)comboboxentry_subjID.Model;
            int index = 0;
            bool found = false;
            foreach (object[] row in store)
            {
                // Check for match
                if (subjid == row[0].ToString())
                {
                    comboboxentry_subjID.Active = index;
                    found = true;
                    break;
                }
                // Increment the index so we can reference it for the active.
                index++;
            }
            if (!found)
            {
                comboboxentry_subjID.AppendText(subjid);
                comboboxentry_subjID.Active = index;
            }


            store = (ListStore)demo_gender.Model;
           index = 0;
           found = false;
            foreach (object[] row in store)
            {
                // Check for match
                if (gender == row[0].ToString())
                {
                    demo_gender.Active = index;
                    found = true;
                    break;
                }
                // Increment the index so we can reference it for the active.
                index++;
            }
            if (!found)
            {
                demo_gender.AppendText(gender);
                demo_gender.Active = index;
            }



            demo_age.Text= (string)demo[i].get("Age");
            demo_headsize.Text=(string)demo[i].get("head_circumference");
            demo_tecnician.Text= (string)demo[i].get("Technician");
            demo_comments.Buffer.Text=(string)demo[i].get("comments");
            demo_group.Text=(string)demo[i].get("Group");
        }

       
    }
}
