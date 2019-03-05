using System;
using Gtk;
using nirs;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

/*
 * This is a simple file type conversion program.  This loads NIRS data using my 
 * NIRSdotNET toolbox (nirs.io.loadX methods).  The data will be plotted using an
 * interactive probe.  The data can be saved to a .nirs or .snirf data format
 * Currently, this only supports one file at a time.
 */


public partial class MainWindow : Gtk.Window
{

    nirs.core.Data data; // this holds the NIRS data (time-course) class

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        data = new nirs.core.Data();

        // This sets the drawing functions for the time-course and probe windows
        this.drawingareaSDG.ExposeEvent += sdgdraw;
        this.drawingarea_main.ExposeEvent += datadraw;
        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingareaSDG.ButtonReleaseEvent += ClickSDG;

    }

    // close function
    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Gtk.Application.Quit();
        a.RetVal = true;
    }




    // This adds info about the file that was just loaded 
    protected void updateInfoText()
    {

        if(data==null){
            return;
        }

        // Set the combobox to allow selecting the data types 
        List<string> datatypes = new List<string>();
        for (int i = 0; i < data.probe.numChannels; i++)
        {
            if (!datatypes.Contains(data.probe.ChannelMap[i].datasubtype))
            {
                datatypes.Add(data.probe.ChannelMap[i].datasubtype);
            }
        }
        ((ListStore)this.combobox1.Model).Clear();

        foreach (var item in datatypes)
        {
            this.combobox1.AppendText(item);
        }
       this.combobox1.Active = 0;


        //  TODO- make this more useful.  This is just a placeholder for now
        string fileInfo = String.Format("File Info\n " +
                                       "{0}\n" +
                                       "number of time points: {1}\n" +
                                       "number of channels: {2}",
                                       data.description,
                                       data.numsamples,
                                       data.probe.numChannels);
        fileInfo = fileInfo + "\n-----------------------------\nDemographics:";
        for (int i = 0; i < data.demographics.length(); i++)
        {
            fileInfo = fileInfo + String.Format("\n{0} : {1}", data.demographics.Keys[i],
                                                data.demographics.get(data.demographics.Keys[i]));
        }




        this.textFileInfo.Buffer.Text = fileInfo;

        string eventInfo = "Stimulus information";
        for (int i = 0; i < data.stimulus.Length; i++){
            eventInfo = eventInfo + String.Format("\n{0}:\n\t {1} events\n\t {2}-{3}s",
                                                data.stimulus[i].name, data.stimulus[i].onsets.Length,
                                                data.stimulus[i].onsets.Min(), data.stimulus[i].onsets.Max());

        }

        this.textEventInfo.Buffer.Text = eventInfo;

        if(data.probe.isregistered){
            this.ViewAction.Visible = true;
        }else{
            this.ViewAction.Visible = false;
            this.TwoDimensionalAction.Active = true;
            this.TenTwentyViewAction.Active = false;

        }


        return;
    }

    protected void sdgdraw(object sender, EventArgs e)
    {
        // This is evoked on exposure of the SDG window to draw the probe
        // drawing is handled by the CProbe class and is reused by several GUIs in the code

        if (data.data == null)
        {
            return;
        }

        if(this.TwoDimensionalAction.Active){
            data.probe.default_display = probedisplay.TwoDimensional;
        }else{
            data.probe.default_display = probedisplay.TenTwenty;
        }

        data.probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        return;
    }

    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG(object o, ButtonReleaseEventArgs args)
    {
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused

        if (data.data == null)
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
        data.probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list

        // update the probe and the data on the the next cycle
        this.drawingareaSDG.QueueDraw();
        this.drawingarea_main.QueueDraw();
    }


    //-----------------------------------------------------------------------
    protected void datadraw(object sender, EventArgs e)
    {

        if (data.data == null)
        {
            return;
        }

        // This is evoked on exposure of the main data window to update the drawing
        data.draw(this.drawingarea_main.GdkWindow, this.combobox1.ActiveText);
        this.drawingarea_main.QueueDraw();
        return;
    }


    // Pull-down menu function to load *.nirs data
    protected void LoadDOTNIRS(object sender, EventArgs e)
    {
        Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Choose the *.nirs file to open",
                                                             this, Gtk.FileChooserAction.Open, "Cancel",
                                                             Gtk.ResponseType.Cancel,
                                                             "Open", Gtk.ResponseType.Accept);

        fc.Filter = new FileFilter();
        fc.Filter.AddPattern("*.nirs");

        if (fc.Run() == (int)Gtk.ResponseType.Accept)
        {
            // use the NIRSdotNET toolbox to load   
            data = nirs.io.readDOTnirs(fc.Filename);
        }
        //Destroy() to close the File Dialog
        fc.Destroy();

        // This adds some info to the bottom right tab menu in the GUI
        updateInfoText();

        // use the data and probe methods to handle the drawing code
        data.probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        data.draw(this.drawingarea_main.GdkWindow);
        this.drawingarea_main.QueueDraw();

    }

    // Loads data in a SNIRF format
    protected void LoadSNIRF(object sender, EventArgs e)
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
            nirs.core.Data[] tmp = nirs.io.readSNIRF(fc.Filename);
            data = (nirs.core.Data)tmp[0].Clone();
        }
        //Destroy() to close the File Dialog
        fc.Destroy();

        // This adds some info to the bottom right tab menu in the GUI
        updateInfoText();

        // use the data and probe methods to handle the drawing code
        data.probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        data.draw(this.drawingarea_main.GdkWindow);
        this.drawingarea_main.QueueDraw();
    }

    protected void LoadNIRx(object sender, EventArgs e)
    {
        Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Choose the NIRx *.wl1 file to open",
                                                      this, Gtk.FileChooserAction.Open,
                                                      "Cancel", Gtk.ResponseType.Cancel,
                                                      "Open", Gtk.ResponseType.Accept);

        fc.Filter = new FileFilter();
        fc.Filter.AddPattern("*.wl1");

        if (fc.Run() == (int)Gtk.ResponseType.Accept)
        {
            // use the NIRSdotNET toolbox to load   
            data = nirs.io.readNIRx(fc.Filename);
        }
        //Destroy() to close the File Dialog
        fc.Destroy();

        // This adds some info to the bottom right tab menu in the GUI
        updateInfoText();

        // use the data and probe methods to handle the drawing code
        data.probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        data.draw(this.drawingarea_main.GdkWindow);
        this.drawingarea_main.QueueDraw();
    }


    // Save the data as a .nirs format.  
    protected void SaveDOTnirs(object sender, EventArgs e)
    {

        Gtk.FileChooserDialog saveDialog = new Gtk.FileChooserDialog("Save as", null,
                                                                     Gtk.FileChooserAction.Save,
                                                                     "Cancel", Gtk.ResponseType.Cancel,
                                                                     "Save", Gtk.ResponseType.Accept);
        saveDialog.Filter = new FileFilter();
        saveDialog.Filter.AddPattern("*.nirs");
        if (saveDialog.Run() == (int)Gtk.ResponseType.Accept)
        {
            string file = saveDialog.Filename;
            if (!file.Contains(".nirs"))
            {
                file = file + ".nirs";
            }
            nirs.io.writeDOTnirs(data, file);

            MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
                                                 MessageType.Info, ButtonsType.Ok,
                                                 "File saved : {0}", file);
            md.Run();
            md.Destroy();

        }

        saveDialog.Destroy();
        return;
    }

    // save the data to the SNIRF format
    protected void SaveSNIRF(object sender, EventArgs e)
    {
        Gtk.FileChooserDialog saveDialog = new Gtk.FileChooserDialog("Save as",
                                                                     null, Gtk.FileChooserAction.Save,
                                                                     "Cancel", Gtk.ResponseType.Cancel,
                                                                     "Save", Gtk.ResponseType.Accept);
        saveDialog.Filter = new FileFilter();
        saveDialog.Filter.AddPattern("*.snirf");
        if (saveDialog.Run() == (int)Gtk.ResponseType.Accept)
        {
            string file = saveDialog.Filename;
            if (!file.Contains(".snirf"))
            {
                file = file + ".snirf";
            }
            nirs.io.writeSNIRF(data, file);

            MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
                                                 MessageType.Info, ButtonsType.Ok,
                                                 "File saved : {0}", file);
            md.Run();
            md.Destroy();

        }

        saveDialog.Destroy();
        return;
    }

   

    // Exit the program
    protected void Exit(object sender, EventArgs e)
    {
        Gtk.Application.Quit();
    }

    // This function is called when the user changes the datatype combobox
    protected void ChangeData(object sender, EventArgs e)
    {
        data.draw(this.drawingarea_main.GdkWindow, this.combobox1.ActiveText);
        this.drawingarea_main.QueueDraw();
        return;
    }

    protected void changeview(object o, ChangedArgs args)
    {
        if (data.data == null)
        {
            return;
        }

        if (this.TwoDimensionalAction.Active)
        {
            data.probe.default_display = probedisplay.TwoDimensional;
        }
        else
        {
            data.probe.default_display = probedisplay.TenTwenty;
        }

        data.probe.draw(this.drawingareaSDG.GdkWindow);
        this.drawingareaSDG.QueueDraw();
        return;
    }
}
