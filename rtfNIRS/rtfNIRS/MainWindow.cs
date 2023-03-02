using System;
using Gtk;
using nirs;
using Gdk;
using System.Collections.Generic;
using rtfNIRS;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public static class Extensions
{
    public static T DeepClone<T>(this T obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }
}

public partial class MainWindow : Gtk.Window
{


    public class GUIsettings
    {
        public double ProbeLineSensitivity = 5;
        public double ProbeOptodeSensitivity = 5;
    }

    public nirs.core.CSession Session;
    private Gtk.ListStore loadeddata;
    public GUIsettings settings;


    public List<NIRS_Plugins.IPlugin> loadedOfflineplugins;
    public List<NIRS_Plugins.IPlugin> loadedRTplugins;

    private bool RTmode;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        settings = new GUIsettings();
        Session = new nirs.core.CSession();
        RTmode = false;

        loadedOfflineplugins = new List<NIRS_Plugins.IPlugin>();

        update_display();
    }

    public void initialize_GUI()
    {

        // This sets the drawing functions for the time-course and probe windows

        this.drawingareaMain.ExposeEvent += datadraw;
        this.drawingareaMain1.ExposeEvent += datadraw;
        this.drawingareaSDG.ExposeEvent += sdgdraw;

        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingareaSDG.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingareaSDG.ButtonReleaseEvent += ClickSDG;


        this.drawingareaMain1.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingareaMain1.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingareaMain1.ButtonPressEvent += MainWindow1ButtonPress;

        // Update the data table
        Gtk.TreeViewColumn treeViewColumn1 = new TreeViewColumn();
        treeViewColumn1.Title = "Subject";

        Gtk.TreeViewColumn treeViewColumn2 = new TreeViewColumn();
        treeViewColumn2.Title = "Scan";

        this.treeviewData.AppendColumn(treeViewColumn1);
        this.treeviewData.AppendColumn(treeViewColumn2);

        loadeddata = new ListStore(typeof(string), typeof(string), typeof(string), typeof(int));
        this.treeviewData.Model = loadeddata;
        Gtk.CellRendererText SubjectNameCell = new Gtk.CellRendererText();
        treeViewColumn1.PackStart(SubjectNameCell, true);
        treeViewColumn1.AddAttribute(SubjectNameCell, "text", 0);

        Gtk.CellRendererText ScanNameCell = new Gtk.CellRendererText();
        treeViewColumn2.PackStart(ScanNameCell, true);
        treeViewColumn2.AddAttribute(ScanNameCell, "text", 1);


        update_display();

    }

    public void datadraw(object sender, EventArgs e)
    {
        update_display();
    }


    public void sdgdraw(object sender, EventArgs e)
    {
        update_display();
    }


    public void ClickSDG(object o, ButtonReleaseEventArgs args)
    {

        double x = args.Event.X;
        double y = args.Event.Y;

        this.drawingareaSDG.GdkWindow.GetSize(out int width, out int height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;

        if (((Gdk.EventButton)args.Event).State == (Gdk.ModifierType.Button1Mask
            | Gdk.ModifierType.ShiftMask))
        {
            reset = false;  // right clicked
        }

        Session.datas[Session.selected].probe.updateML((int)x, (int)y, reset, width, height, settings.ProbeOptodeSensitivity, settings.ProbeLineSensitivity);  // update the active measurement list

        /*
        if (combobox_selectview.ActiveText.Equals("Flat View"))
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device1.Active].probe.updateML((int)x, (int)y, reset, width, height, settings.ProbeOptodeSensitivity, settings.ProbeLineSensitivity);  // update the active measurement list
        }
        else
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device1.Active].probe.updateML1020((int)x, (int)y, reset, width, height, settings.ProbeOptodeSensitivity, settings.ProbeLineSensitivity);  // update the active measurement list
        }

       */
        update_display();

    }


    public void add_data(nirs.core.Data data)
    {

        data.UUID=Guid.NewGuid();
        Session.datas.Add(data);
        string subjid;
        if (data.demographics.Keys.Contains("subjid"))
        {
            subjid = (string)data.demographics.get("subjid");
        }
        else
        {
            subjid = "unknown";
        }
        string scan = data.description;

        TreeIter iter = loadeddata.AppendValues(subjid, scan);


        this.treeviewData.ActivateRow(loadeddata.GetPath(iter), this.treeviewData.Columns[0]);
        update_display();
    }



    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void ToggleMainDisplayCount(object sender, EventArgs e)
    {
        if (((Gtk.Action)sender).Label == "Single View"){
            DataFrame2.Visible = false;
        }
        else {
            DataFrame2.Visible = true;
        }
        update_display();
    }


    public void update_display()
    {
        // This updates the display window
        if (Session.datas.Count == 0)
        {
            clear_drawing(drawingareaMain.GdkWindow);
            clear_drawing(drawingareaMain1.GdkWindow);
            drawingareaMain.QueueDraw();
            drawingareaMain1.QueueDraw();
            return;
        }




        if (Session.selected == -1)
        {
            clear_drawing(drawingareaMain.GdkWindow);
            clear_drawing(drawingareaMain1.GdkWindow);
            drawingareaMain.QueueDraw();
            drawingareaMain1.QueueDraw();
            return;
        }

        string Dataselected1 = combobox1.ActiveText;
        string Dataselected2 = combobox2.ActiveText;


        GtkLabelData1.Text = String.Format("<b>{0}</b>", Dataselected1);
        GtkLabelData1.UseMarkup = true;
        GtkLabelData2.Text = String.Format("<b>{0}</b>", Dataselected2);
        GtkLabelData2.UseMarkup = true;


        Session.draw(drawingareaMain.GdkWindow, Dataselected1);
        Session.draw(drawingareaMain1.GdkWindow, Dataselected2);
        //TODO fix this to the session (and the set ML code)
        Session.datas[Session.selected].probe.draw(drawingareaSDG.GdkWindow);


    }


    private void clear_drawing(Gdk.Drawable da)
    {
        if (da == null)
        {
            return;
        }

        int xoffset = 50;
        int yoffset = 1;

        int width, height;
        da.GetSize(out width, out height);
        height = height - 31;
        width = width - 51;

        Gdk.GC gc = new Gdk.GC(da);

        gc.RgbBgColor = new Gdk.Color(0, 0, 0);
        gc.RgbFgColor = new Gdk.Color(0, 0, 0);
        Rectangle rarea = new Rectangle();
        rarea.X = xoffset - 1;
        rarea.Y = yoffset - 1;
        rarea.Height = height + 2;
        rarea.Width = width + 2;
        da.DrawRectangle(gc, true, rarea);

        gc.RgbBgColor = new Color(0, 0, 0);
        gc.RgbFgColor = new Color(255, 255, 255);
        rarea = new Rectangle();
        rarea.X = xoffset;
        rarea.Y = yoffset;
        rarea.Height = height;
        rarea.Width = width;
        da.DrawRectangle(gc, true, rarea);

    }

    protected void LoadDataFile(object sender, EventArgs e)
    {

        Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog("Choose the *.snirf file to open",
                                                     this, Gtk.FileChooserAction.Open,
                                                     "Cancel", Gtk.ResponseType.Cancel,
                                                     "Open", Gtk.ResponseType.Accept);

        fc.Filter = new FileFilter();
        fc.Filter.AddPattern("*.snirf");
        fc.Filter.AddPattern("*.nirs");

        if (fc.Run() == (int)Gtk.ResponseType.Accept)
        {
            // use the NIRSdotNET toolbox to load
            for (int i = 0; i < fc.Filenames.Length; i++)
            {
                if (fc.Filenames[i].Contains(".nirs"))
                {
                    nirs.core.Data data = nirs.io.readDOTnirs(fc.Filenames[i]);
                    add_data(data);

                }
                else if (fc.Filenames[i].Contains(".snirf"))
                {
                    nirs.core.Data[] data = nirs.io.readSNIRF(fc.Filenames[i]);
                    for (int j = 0; j < data.Length; j++)
                    {
                        add_data(data[j]);
                    }
                }


            }
        }
        fc.Destroy();
        return;
    }

    protected void SelectData(object o, RowActivatedArgs args)
    {

        Session.selected = args.Path.Indices[0];

        List<string> types = Session.GetTypes();

        bool hasunique = false;
        TreeIter iter = new TreeIter();
        for (int i = 0; i < types.Count; i++)
        {
            bool flag = combobox1.Model.GetIterFromString(out iter, types[i]);
            if (!flag)
            {
                hasunique = true;
            }
        }

        if (hasunique)
        {
            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            combobox1.Model = ClearList;
            combobox2.Model = ClearList2;
            for (int i = 0; i < types.Count; i++)
            {
                combobox1.AppendText(types[i]);
                combobox2.AppendText(types[i]);
            }
            combobox1.Active = 0;
            combobox2.Active = 0;
        }


        return;
    }

    protected void TestRun(object sender, EventArgs e)
    {

        NIRS_Plugins.IPlugin plugin = new NIRS_Plugins.MBLL();

        plugin.Run(Session.datas[0]);


    }


    protected void Pipeline_Widget(object sender, EventArgs e)
    {
        PipelineManager pipelineManager = new PipelineManager();
        pipelineManager.ShowAll();
    }

    protected void PipelineManagerGUI(object sender, EventArgs e)
    {
        PipelineManager pipelineManager = new PipelineManager();
        pipelineManager.ShowAll();
    }

    protected void ChangeModeRT(object sender, EventArgs e)
    {

        RTmode = ToggleRTMode.Active;
        if (!RTmode)
        {
            ToggleRTMode.Label = "  Real-time  ";
            ToggleRTMode.ShortLabel = "  Real-time  ";
        }
        else
        {
            ToggleRTMode.Label = "Post-analysis";
            ToggleRTMode.ShortLabel = "Post-analysis";
        }
        update_display();
    }

    protected void RunOfflineAnalysis(object sender, EventArgs e)
    {

        List<nirs.core.Data> data = new List<core.Data>();
        for(int i=0; i<Session.datas.Count; i++)
        {
            data.Add((core.Data)Session.datas[i].Clone());
        }
        for(int i=0; i<loadedOfflineplugins.Count; i++)
        {
            if (loadedOfflineplugins[i].Name == "ImportData")
            {
                // save this data as a new data derivative
                data= (List<core.Data>)Session.Get_DataDerivative((string)loadedOfflineplugins[i].Parameters[0].value);
            }

            if (loadedOfflineplugins[i].DataIn_type == typeof(List<nirs.core.Data>))
            {
                data = (List<core.Data>)loadedOfflineplugins[i].Run(data);
            }else if (loadedOfflineplugins[i].DataIn_type == typeof(nirs.core.Data))
            {
                for(int fI=0; fI<data.Count; fI++)
                {
                    data[fI] = (core.Data)loadedOfflineplugins[i].Run(data[fI]);
                }
            }
            if (loadedOfflineplugins[i].Name == "ExportData")
            {
                // save this data as a new data derivative
                Session.Add_DataDerivative(data, (string)loadedOfflineplugins[i].Parameters[0].value);
            }
        }
        List<string> types = Session.GetTypes();
        bool hasunique = false;
        TreeIter iter = new TreeIter();
        for (int i = 0; i < types.Count; i++)
        {
            bool flag = combobox1.Model.GetIterFromString(out iter, types[i]);
            if (!flag)
            {
                hasunique = true;
            }
        }

        if (hasunique)
        {
            Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
            Gtk.ListStore ClearList2 = new Gtk.ListStore(typeof(string));
            combobox1.Model = ClearList;
            combobox2.Model = ClearList2;
            for (int i = 0; i < types.Count; i++)
            {
                combobox1.AppendText(types[i]);
                combobox2.AppendText(types[i]);
            }
            combobox1.Active = 0;
            combobox2.Active = 0;
        }

        return;
    }

    protected void AutoScale(object sender, EventArgs e)
    {
    }

    [GLib.ConnectBefore]
    protected void MainWindow1ButtonPress(object o, ButtonPressEventArgs args)
    {

        Gtk.Menu mbox = new Gtk.Menu();
        Gtk.MenuItem Test = new Gtk.MenuItem("test");
        Test.Activated += delegate (object sender, EventArgs e) {
            Console.WriteLine("test");
        };
        mbox.Append(Test);
        mbox.ShowAll();
        mbox.Popup();


    }

}