using System;
using Gtk;
using nirs;
using System.Collections.Generic;

public partial class MainWindow : Gtk.Window
{
    nirs.core.Data[] data; // this holds the NIRS data (time-course) class

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        data = new core.Data[1];
        data[0] = new nirs.core.Data();


        // This sets the drawing functions for the time-course and probe windows
        this.drawingareaSDG.ExposeEvent += sdgdraw;
        this.drawingareaData.ExposeEvent += datadraw;
        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingareaSDG.ButtonReleaseEvent += ClickSDG;


    }



    protected void sdgdraw(object sender, EventArgs e)
    {
        // This is evoked on exposure of the SDG window to draw the probe
        // drawing is handled by the CProbe class and is reused by several GUIs in the code


        int idx = this.comboboxdataEntry.Active;

        if (data[idx].data == null)
        {
            return;
        }

        if (!this.checkbuttonProbe3D.Active)
        {
            data[idx].probe.default_display = probedisplay.TwoDimensional;
        }
        else
        {
            data[idx].probe.default_display = probedisplay.TenTwenty;
        }

        data[idx].probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        return;
    }

    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG(object o, ButtonReleaseEventArgs args)
    {
        int idx = this.comboboxdataEntry.Active;
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused

        if (data[idx].data == null)
        {
            return;
        }

        double x = args.Event.X;
        double y = args.Event.Y;

        int width, height;
        this.drawingareaSDG.GdkWindow.GetSize(out width, out height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;
        if (args.Event.Button == 3)
        {
            reset = false;  // right clicked
        }

        // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
        data[idx].probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list

        // update the probe and the data on the the next cycle
        this.drawingareaSDG.QueueDraw();
        this.drawingareaData.QueueDraw();
    }


    //-----------------------------------------------------------------------
    protected void datadraw(object sender, EventArgs e)
    {
        int idx = this.comboboxdataEntry.Active;
        if (data[idx].data == null)
        {
            return;
        }

        // This is evoked on exposure of the main data window to update the drawing
        data[idx].draw(this.drawingareaData.GdkWindow, this.comboboxWhichData.ActiveText);
        this.drawingareaData.QueueDraw();
        return;
    }


    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void ExportInfo(object sender, EventArgs e)
    {
    }

    protected void LoadFile(object sender, EventArgs e)
    {
        Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Choose the *.snirf file to open",
                                                             this, Gtk.FileChooserAction.Open,
                                                             "Cancel", Gtk.ResponseType.Cancel,
                                                             "Open", Gtk.ResponseType.Accept);

        fc.Filter = new FileFilter();
        fc.Filter.AddPattern("*.snirf");

        if (fc.Run() == (int)Gtk.ResponseType.Accept)
        {
            // use the NIRSdotNET toolbox to load


            List<nirs.HDF5info> info = nirs.io.SNIRFinfo(fc.Filename);
            List<string> invalid = new List<string>();
            bool isvalid = nirs.io.SNIRFValidate(info, ref invalid);
            
            textview1.Buffer.Text = "";
            for (int i = 0; i < info.Count; i++)
            {

                
                textview1.Buffer.Text += string.Format("{0} [ {1} ]  : {2}\r", info[i].field, info[i].HDFclass, info[i].description); ;
            }

            if (isvalid)
            {
                nirs.core.Data[] tmp = nirs.io.readSNIRF(fc.Filename);
                data = new core.Data[tmp.Length];
                for (int i = 0; i < tmp.Length; i++)
                {
                    data[i] = (nirs.core.Data)tmp[i].Clone();
                }

     


                if (data.Length == 1)
                {
                    comboboxdataEntry.Sensitive = false;
                    comboboxdataEntry.Active = 0;
                }
                int ii = comboboxdataEntry.Active;

                // Set the combobox to allow selecting the data types 
                List<string> datatypes = new List<string>();
                for (int i = 0; i < data[ii].probe.numChannels; i++)
                {
                    if (!datatypes.Contains(data[ii].probe.ChannelMap[i].datasubtype))
                    {
                        datatypes.Add(data[ii].probe.ChannelMap[i].datasubtype);
                    }
                }

                if (data[ii].probe.SrcPos3D == null)
                {
                    checkbuttonProbe3D.Sensitive = false;
                }

        ((ListStore)comboboxWhichData.Model).Clear();

                foreach (var item in datatypes)
                {
                    this.comboboxWhichData.AppendText(item);
                }
                this.comboboxWhichData.Active = 0;


                ((ListStore)comboboxdataEntry.Model).Clear();

                for (int i = 0; i < data.Length; i++)
                {
                    comboboxdataEntry.AppendText(data[i].description);
                }
                this.comboboxdataEntry.Active = 0;

                UpdateText();
            }
        }

        //Destroy() to close the File Dialog
        fc.Destroy();

        // This adds some info to the bottom right tab menu in the GUI
        // TODO    updateInfoText();

        int idx = this.comboboxdataEntry.Active;
        // use the data and probe methods to handle the drawing code
        data[idx].probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        data[idx].draw(this.drawingareaData.GdkWindow);
        this.drawingareaData.QueueDraw();


    }

    protected void UpdateText()
    {
        int idx = this.comboboxdataEntry.Active;
        if (data[idx].data == null)
        {
            return;
        }
        textview5.Buffer.Text = data[idx].description;
        textview5.Buffer.Text += string.Format("\rLength of scan = {0}s",
            data[idx].time[data[idx].time.Count - 1] - data[idx].time[0]);
        textview5.Buffer.Text += string.Format("\rNumber of data channels = {0}",
            data[idx].probe.numChannels);
        textview5.Buffer.Text += string.Format("\rNumber of Sources = {0}",
            data[idx].probe.numSrc);
        textview5.Buffer.Text += string.Format("\rNumber of Detectors = {0}",
            data[idx].probe.numSrc);
        textview5.Buffer.Text += string.Format("\rNumber of Wavelengths= {0}",
            data[idx].probe.numWavelengths);
        if (data[idx].probe.SrcPos3D != null)
        {
            textview5.Buffer.Text += string.Format("\rProbe is 3D registered");
        }
        textview5.Buffer.Text += string.Format("\rNumber of Auxillary = {0}",
            data[idx].auxillaries.Length);
        textview5.Buffer.Text += string.Format("\rNumber of Stimulus types = {0}",
            data[idx].stimulus.Count);
        for (int i=0; i<data[idx].stimulus.Count; i++)
        {
            textview5.Buffer.Text += string.Format("\r\t{0} : {1} events",
            data[idx].stimulus[i].name,data[idx].stimulus[i].onsets.Count);
        }

        textview3.Buffer.Text = string.Format("Demographics: {0}",data[idx].description);
        foreach (string demo in data[idx].demographics.Keys)
        {
            object val = data[idx].demographics.get(demo);
            textview3.Buffer.Text += string.Format("\r\t{0} : {1}", demo, val);
        }



    }


    protected void Exit(object sender, EventArgs e)
    {
        Gtk.Application.Quit();
    }

    protected void ChangeData(object sender, EventArgs e)
    {
        int idx = this.comboboxdataEntry.Active;
        if (idx < 0)
        {
            return;
        }

        UpdateText();

        if (data[idx].probe.SrcPos3D == null)
        {
            checkbuttonProbe3D.Sensitive = false;
        }
        else
        {
            checkbuttonProbe3D.Sensitive = true;
        }

        data[idx].probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
    }

    protected void ChangeWavelength(object sender, EventArgs e)
    {
        int idx = this.comboboxdataEntry.Active;
        data[idx].probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
    }


    protected void ChangeProbe3D(object sender, EventArgs e)
    {
        int idx = this.comboboxdataEntry.Active;
        if (data[idx].data == null)
        {
            return;
        }

        if (!this.checkbuttonProbe3D.Active)
        {
            data[idx].probe.default_display = probedisplay.TwoDimensional;
        }
        else
        {
            data[idx].probe.default_display = probedisplay.TenTwenty;
        }

        data[idx].probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        return;
    }
}
