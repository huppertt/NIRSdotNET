using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Linq;

public struct handles
{
    public List<lasers> lasers;
    public List<detector> detectors;
    public Gtk.DrawingArea SDGplot;
    public Gtk.DrawingArea Dataplot;
    public Gtk.ComboBox whichdata;
}

public struct lasers
{
    public string name;
    public int[] wavelength;
    public bool[] state;
    public int[] gain;
    public int deviceIdx;
    public int[] laserIdx;
    public Gtk.SpinButton[] spinButtons;
    public Gtk.Button[] buttons;
    public Gtk.Frame frame;
}

public struct detector
{
    public string name;
    public int gain;
    public int deviceIdx;
    public int detectorIdx;
    public Gtk.Frame frame;
    public Gtk.ColorButton led;
    public Gtk.VScale vScale;
}

public partial class MainWindow : Gtk.Window
{
    public handles _handles;
    public NIRSrecorder.Settings settings;
    public List<nirs.core.Data> nirsdata;

    public static Thread maindisplaythread;  // Timing thread to handle drawing during running aquistion


    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        settings = new NIRSrecorder.Settings();
        // IntializeGUI();
    }

    public void IntializeGUI()
    {

        // For now
        // This will allow me to handle multiple devices (eventually).  For now, just default to 1
        List<string> ports = new List<string>();
        if (settings.SYSTEM.Trim().ToLower().Equals("simulator"))
        {
            ports.Add("simulator");
        }
        else if (settings.SYSTEM.Trim().ToLower().Equals("simulatorhyperscan"))
        {
            settings.SYSTEM = "Simulator";
            ports.Add("1");
            ports.Add("2");
        }
        else
        {
            ports.Add("/dev/tty.Dual-SPP-SerialPort");
        }


        _handles = new handles();
        _handles.detectors = new List<detector>();
        _handles.lasers = new List<lasers>();

        _handles.Dataplot = this.drawingarea_Data;
        _handles.SDGplot = this.drawingarea_SDG;
        _handles.whichdata = this.combobox_whichdata;


        nirsdata = new List<nirs.core.Data>();

        colorbutton1.Color = new Gdk.Color(128, 128, 128);

        string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        path = System.IO.Path.Combine(path, "LastSettings.xml");
        if (System.IO.File.Exists(path))
        {
            QuickStartAction.Sensitive = true;
            // Read the Config.xml file
            XmlDocument doc = new XmlDocument();

            doc.Load(path);
            XmlNodeList elemList;

            elemList = doc.GetElementsByTagName("Investigator");
            string investigator = elemList[0].InnerXml.Trim();
            elemList = doc.GetElementsByTagName("Study");
            string study = elemList[0].InnerXml.Trim();
            QuickStartAction.Label = "Quick Start: " + investigator + ":" + study;
            
        }
        else
        {
            QuickStartAction.Sensitive = false;
        }


            // remove all pages
            notebook_detectors.RemovePage(0);
        notebook_sources.RemovePage(0);


       

        MainClass.devices = new NIRSDAQ.Instrument.instrument[ports.Count];
        for (int i = 0; i < ports.Count; i++)
        {
            MainClass.devices[i]= new NIRSDAQ.Instrument.instrument(settings.SYSTEM);

            MainClass.devices[i].Connect(ports[i]);
            
            NIRSDAQ.info _info = MainClass.devices[i].GetInfo();
            _info.numDet = settings.system_Info.numdet;
            _info.numSrc = settings.system_Info.numsrc;


            Gtk.HBox hBox = new HBox(true, 0);
            Gtk.Label label = new Label();
            for (int j = 0; j < _info.numDet; j++)
            {
                //TODO remove and relabel detector panel
                if (j % 8 == 0)
                {
                    hBox = new HBox(true, 0);
                    label = new Label(String.Format("{0}\n{1}-{2}", _info.DeviceName, j + 1, j + 8));
                    label.LineWrap = true;
                    notebook_detectors.InsertPage(hBox, label, notebook_detectors.NPages);
                }
                detector det = new detector();
                det.detectorIdx = j;
                det.deviceIdx = i;
                det.gain = 0;
                det.name = String.Format("Det-{0}", j + 1);

                det.frame = new Frame(det.name);
                Gtk.VBox _box = new VBox(false, 0);
                det.led = new ColorButton(new Gdk.Color(0, 255, 0));
                det.led.HeightRequest = 5;
                det.led.Sensitive = false;

                det.vScale = new VScale(0, settings.system_Info.maxgain, 1);
                det.vScale.Value = det.gain;
                det.vScale.ValuePos = PositionType.Bottom;
                det.vScale.Inverted = true;
                det.vScale.HeightRequest = 100;
                det.vScale.Name = String.Format("{0}", _handles.detectors.Count);
                det.vScale.ValueChanged += DetChanged;

                _box.PackStart(det.led);
                _box.PackStart(det.vScale);
                det.frame.Add(_box);
                hBox.PackStart(det.frame);

                _handles.detectors.Add(det);

            }

            Gtk.VBox _vbx = new VBox();
            // Now add the source controls
            for (int j = 0; j < _info.numSrc; j += _info.numwavelengths)
            {
                //TODO remove and relabel detector panel
                if (j % 8 == 0)
                {
                    _vbx = new VBox(true, 0);
                    label = new Label(String.Format("{0}\n{1}-{2}", _info.DeviceName, j + 1, j + 8));
                    label.LineWrap = true;
                    notebook_sources.InsertPage(_vbx, label, notebook_sources.NPages);
                }
                int sIdx = j / _info.numwavelengths + 1;
                Gtk.Frame frame = new Frame(String.Format("Source-{0}", sIdx));
                Gtk.HBox _hbx = new HBox(true, 0);
                lasers src = new lasers();
                src.laserIdx = new int[_info.numwavelengths];
                src.wavelength = new int[_info.numwavelengths];
                src.gain = new int[_info.numwavelengths];
                src.state = new bool[_info.numwavelengths];
                src.buttons = new Button[_info.numwavelengths];
                src.spinButtons = new SpinButton[_info.numwavelengths];
                src.deviceIdx = i;
                src.frame = frame;
                for (int k = 0; k < _info.numwavelengths; k++)
                {
                    src.laserIdx[k] = j + k;
                    src.wavelength[k] = _info.wavelengths[k];
                    src.gain[k] = 0;
                    src.state[k] = false;
                    src.name = String.Format("{0}nm", src.wavelength[k]);
                    Gtk.HBox _hbx2 = new HBox(true, 0);
                    src.buttons[k] = new Button();
                    src.buttons[k].Label = src.name;
                    _hbx2.PackStart(src.buttons[k]);
                    src.spinButtons[k] = new SpinButton(0, settings.system_Info.maxpower, 1);
                    src.spinButtons[k].Value = 0;
                    src.buttons[k].Name = String.Format("{0}", _handles.lasers.Count);
                    src.spinButtons[k].Name = String.Format("{0} {1}", _handles.lasers.Count, k);
                    if (!settings.system_Info.laseradjustable)
                    {
                        src.spinButtons[k].Visible = false;
                    }

                    src.buttons[k].Clicked += SrcClicked;
                    src.buttons[k].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                    src.buttons[k].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                    src.buttons[k].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                    src.spinButtons[k].ValueChanged += SrcValueChanged;

                    _hbx2.PackStart(src.spinButtons[k]);
                    _hbx.PackStart(_hbx2);
                }
                src.frame.Add(_hbx);
                _vbx.PackStart(src.frame);
                _handles.lasers.Add(src);
            }


        }

        ShowSystemMessagingAction.Active = settings.DEBUG;

        this.drawingarea_SDG.ExposeEvent += sdgdraw;
        this.drawingarea_Data.ExposeEvent += datadraw;
        this.drawingarea_SDG.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingarea_SDG.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingarea_SDG.ButtonReleaseEvent += ClickSDG;
        this.drawingarea_SDG2.ExposeEvent += sdgdraw;
        this.drawingarea_Data2.ExposeEvent += datadraw;
        this.drawingarea_SDG2.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.drawingarea_SDG2.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.drawingarea_SDG2.ButtonReleaseEvent += ClickSDG2;

        combobox_device1.Model = new Gtk.ListStore(typeof(string));
        combobox_device2.Model = new Gtk.ListStore(typeof(string));
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            combobox_device1.AppendText(MainClass.devices[i].devicename);
            combobox_device2.AppendText(MainClass.devices[i].devicename);
        }


        nodeview4.AppendColumn("FileName", new Gtk.CellRendererText(), "text", 0);
        nodeview4.AppendColumn("Comments", new Gtk.CellRendererText(), "text", 1);
        nodeview4.NodeStore = new NodeStore(typeof(MyTreeNodeData));

        nodeview_stim.AppendColumn("Name",new Gtk.CellRendererText(), "text", 0);
        nodeview_stim.AppendColumn("Onset", new Gtk.CellRendererText(), "text", 1);
        nodeview_stim.AppendColumn("Duration", new Gtk.CellRendererText(), "text", 2);
        nodeview_stim.AppendColumn("Amplitude", new Gtk.CellRendererText(), "text", 3);
        nodeview_stim.NodeStore = new NodeStore(typeof(MyTreeNode));

        this.ShowAll();

        if (MainClass.devices.Length == 1)
        {
            fixed_device1.Visible = false;
            fixed_device2.Visible = false;
            combobox_device1.Visible = false;
            combobox_device2.Visible = false;
            drawingarea_Data2.Visible = false;
            drawingarea_SDG2.Visible = false;

            fixed_device1.Hide();
            fixed_device2.Hide();
            combobox_device1.Hide();
            combobox_device2.Hide();
            HyperscanningViewAction.Sensitive = false;
            drawingarea_Data2.Hide();
            drawingarea_SDG2.Hide();
            combobox_device1.Active = 0;
        }
        else
        {
         //   fixed_device1.Hide();
            fixed_device2.Hide();

            combobox_device1.Active = 0;
            combobox_device2.Active = 1;

         //   combobox_device1.Hide();
            combobox_device2.Hide();
            HyperscanningViewAction.Sensitive = true;
            HyperscanningViewAction.Active = false;
            drawingarea_Data2.Hide();
            drawingarea_SDG2.Hide();
        }


    }

    void SrcClicked(object sender, EventArgs e)
    {

        int WidgetIdx = Int32.Parse(((Gtk.Button)sender).Name);
        lasers laser = _handles.lasers[WidgetIdx];
        int deviceIdx = laser.deviceIdx;
        bool[] state = laser.state;
        string WL = ((Gtk.Button)sender).Label;
        WL = WL.Remove(WL.Length - 2); // remove the trailing "nm"
        int laserIdx = Int32.Parse(WL);
        for (int i = 0; i < laser.wavelength.Length; i++)
        {
            if (laser.wavelength[i] == laserIdx)
            {
                laserIdx = i;
                break;
            }
        }


        bool newstate = !state[laserIdx];
        if (checkbutton_linklasers.Active)
        {
            for (int i = 0; i < state.Length; i++)
            {
                if (state[i] != newstate)
                {
                    MainClass.devices[deviceIdx].SetLaserState(laser.laserIdx[i], newstate);
                    state[i] = newstate;
                }
                if (newstate)
                {
                    laser.buttons[i].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                    laser.buttons[i].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                    laser.buttons[i].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
                }
                else
                {
                    laser.buttons[i].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                    laser.buttons[i].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                    laser.buttons[i].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                }
            }
        }
        else
        {
            MainClass.devices[deviceIdx].SetLaserState(laser.laserIdx[laserIdx], newstate);
            state[laserIdx] = newstate;
            if (newstate)
            {
                laser.buttons[laserIdx].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                laser.buttons[laserIdx].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                laser.buttons[laserIdx].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
            }
            else
            {
                laser.buttons[laserIdx].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                laser.buttons[laserIdx].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                laser.buttons[laserIdx].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
            }
        }
        laser.state = state;
        _handles.lasers[WidgetIdx] = laser;
        return;

    }
    void SrcValueChanged(object sender, EventArgs e)
    {
        string[] name = ((Gtk.SpinButton)sender).Name.Split(' ');
        int WidgetIdx = Int32.Parse(name[0]);
        lasers laser = _handles.lasers[WidgetIdx];
        int deviceIdx = laser.deviceIdx;
        int laserIdx = Int32.Parse(name[1]);

        laser.gain[laserIdx] = (int)((Gtk.SpinButton)sender).Value;
        _handles.lasers[WidgetIdx] = laser;
        MainClass.devices[deviceIdx].SetLaserPower(laser.laserIdx[laserIdx], laser.gain[laserIdx]);
        return;
    }


    void DetChanged(object sender, EventArgs e)
    {
        int WidgetIdx = Int32.Parse(((Gtk.VScale)sender).Name);
        detector detector = _handles.detectors[WidgetIdx];
        int deviceIdx = detector.deviceIdx;
        int detIdx = detector.detectorIdx;
        int gain = (int)((Gtk.VScale)sender).Value;
        detector.gain = gain;
        _handles.detectors[WidgetIdx] = detector;
        MainClass.devices[deviceIdx].SetDetGain(detIdx, gain);
        return;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }



    protected void ClickedMenuPrefrences(object sender, EventArgs e)
    {
        PrefrencesWindow prefrenceswindow = new PrefrencesWindow();
        prefrenceswindow.Show();
        Application.Run();

    }

    protected void SourcePower_SetAll(object sender, EventArgs e)
    {
        int gain = (int)((SpinButton)sender).Value;
        for (int i = 0; i < _handles.lasers.Count; i++)
        {
            lasers lasers = _handles.lasers[i];
            for (int k = 0; k < lasers.spinButtons.Length; k++)
            {
                lasers.spinButtons[k].Value = gain;
            }
        }

    }

    protected void DetGain_SetAll(object sender, EventArgs e)
    {
        int gain = (int)((SpinButton)sender).Value;
        for (int i = 0; i < _handles.detectors.Count; i++)
        {
            detector detector = _handles.detectors[i];
            detector.vScale.Value = gain;
        }


    }

    protected void LasersAllOnOff(object sender, EventArgs e)
    {
        bool state = ((Gtk.Button)sender).Label.Equals("Sources On");
        if (state)
        {
            // turn them all off
            ((Gtk.Button)sender).Label = "Sources Off";

            for (int i = 0; i < _handles.lasers.Count; i++)
            {
                lasers lasers = _handles.lasers[i];
                int deviceIdx = lasers.deviceIdx;
                for (int j = 0; j < lasers.wavelength.Length; j++)
                {
                    MainClass.devices[deviceIdx].SetLaserState(lasers.laserIdx[j], state);
                    lasers.state[j] = state;
                    lasers.buttons[j].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                    lasers.buttons[j].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                    lasers.buttons[j].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
                }
                _handles.lasers[i] = lasers;
            }

        }
        else
        {
            ((Gtk.Button)sender).Label = "Sources On";

            for (int i = 0; i < _handles.lasers.Count; i++)
            {
                lasers lasers = _handles.lasers[i];
                int deviceIdx = lasers.deviceIdx;
                for (int j = 0; j < lasers.wavelength.Length; j++)
                {
                    MainClass.devices[deviceIdx].SetLaserState(lasers.laserIdx[j], state);
                    lasers.state[j] = state;
                    lasers.buttons[j].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                    lasers.buttons[j].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                    lasers.buttons[j].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                }
                _handles.lasers[i] = lasers;
            }

        }

    }

    protected void RegisterSubject(object sender, EventArgs e)
    {
        RegisterSubject registerSubject = new RegisterSubject();
        registerSubject.Show();
        Application.Run();


    }

    protected void ExitGUI(object sender, EventArgs e)
    {
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            MainClass.devices[i].Stop();
            MainClass.devices[i].AllOff();
            MainClass.devices[i].FlushBuffer();
        }

        this.Destroy();

    }

    protected void AboutDLG(object sender, EventArgs e)
    {
        HelpDLG dlg = new HelpDLG();
        dlg.Show();
        Application.Run();
    }

    protected void HelpDLG(object sender, EventArgs e)
    {
        _ = System.Diagnostics.Process.Start("https://bitbucket.org/huppertt/");
    }


    protected void sdgdraw(object sender, EventArgs e)
    {
        // This is evoked on exposure of the SDG window to draw the probe
        // drawing is handled by the CProbe class and is reused by several GUIs in the code

        if (nirsdata.Count == 0)
        {
            return;
        }


        if (this.combobox_selectview.ActiveText.Equals("Flat View"))
        {
            nirsdata[combobox_device1.Active].probe.default_display = nirs.probedisplay.TwoDimensional;
        }
        else
        {
            nirsdata[combobox_device1.Active].probe.default_display = nirs.probedisplay.TenTwenty;
        }

        nirsdata[combobox_device1.Active].probe.draw(this.drawingarea_SDG.GdkWindow);
        this.drawingarea_SDG.QueueDraw();
        if (HyperscanningViewAction.Active)
        {
            if (this.combobox_selectview.ActiveText.Equals("Flat View"))
            {
                nirsdata[combobox_device2.Active].probe.default_display = nirs.probedisplay.TwoDimensional;
            }
            else
            {
                nirsdata[combobox_device2.Active].probe.default_display = nirs.probedisplay.TenTwenty;
            }

            nirsdata[combobox_device2.Active].probe.draw(this.drawingarea_SDG2.GdkWindow);
            this.drawingarea_SDG2.QueueDraw();
        }

        return;
    }

    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG(object o, ButtonReleaseEventArgs args)
    {
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused
        if (nirsdata.Count == 0)
        {
            return;
        }

        double x = args.Event.X;
        double y = args.Event.Y;

        int width, height;
        this.drawingarea_SDG.GdkWindow.GetSize(out width, out height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;
        if (args.Event.Button == 3)
        {
            reset = false;  // right clicked
        }

        // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
        nirsdata[combobox_device1.Active].probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list

        // update the probe and the data on the the next cycle
        this.drawingarea_SDG.QueueDraw();
        this.drawingarea_Data.QueueDraw();
    }


    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG2(object o, ButtonReleaseEventArgs args)
    {
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused
        if (nirsdata.Count == 0)
        {
            return;
        }
        if (!HyperscanningViewAction.Active)
        {
            return;
        }


        double x = args.Event.X;
        double y = args.Event.Y;

        int width, height;
        this.drawingarea_SDG2.GdkWindow.GetSize(out width, out height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;
        if (args.Event.Button == 3)
        {
            reset = false;  // right clicked
        }

        // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
        nirsdata[combobox_device2.Active].probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list

        // update the probe and the data on the the next cycle
        this.drawingarea_SDG2.QueueDraw();
        this.drawingarea_Data2.QueueDraw();
    }


    //-----------------------------------------------------------------------
    protected void datadraw(object sender, EventArgs e)
    {
        if (nirsdata.Count == 0)
        {
            return;
        }
        if (nirsdata[0].data == null)
        {
            return;
        }


        // This is evoked on exposure of the main data window to update the drawing
        nirsdata[combobox_device1.Active].draw(this.drawingarea_Data.GdkWindow, this.combobox_whichdata.ActiveText);
        if (HyperscanningViewAction.Active)
        {
            nirsdata[combobox_device2.Active].draw(this.drawingarea_Data2.GdkWindow, this.combobox_whichdata.ActiveText);
            this.drawingarea_Data2.QueueDraw();
        }
        this.drawingarea_Data.QueueDraw();
        return;
    }

    protected void changeprobe_view(object sender, EventArgs e)
    {
        this.drawingarea_SDG.QueueDraw();
        if (HyperscanningViewAction.Active)
        {
            this.drawingarea_SDG2.QueueDraw();
        }
    }


    protected void SetHyperscanningView(object sender, EventArgs e)
    {
        if (HyperscanningViewAction.Active)
        {
            fixed_device1.Visible = true;
            fixed_device2.Visible = true;
            combobox_device1.Visible = true;
            combobox_device2.Visible = true;
            drawingarea_Data2.Visible = true;
            drawingarea_SDG2.Visible = true;

            fixed_device1.Show();
            fixed_device2.Show();
            combobox_device1.Show();
            combobox_device2.Show();
            drawingarea_Data2.Show();
            drawingarea_SDG2.Show();
        }
        else
        {
          //  fixed_device1.Visible = false;
            fixed_device2.Visible = false;
           // combobox_device1.Visible = false;
            combobox_device2.Visible = false;
            drawingarea_Data2.Visible = false;
            drawingarea_SDG2.Visible = false;

           // fixed_device1.Hide();
            fixed_device2.Hide();
           // combobox_device1.Hide();
            combobox_device2.Hide();
            drawingarea_Data2.Hide();
            drawingarea_SDG2.Hide();
        }

    }

    protected void SetShowSystemMsg(object sender, EventArgs e)
    {
        MainClass.win.settings.DEBUG = ShowSystemMessagingAction.Active;
    }

    protected void InitializeData()
    {

        nirsdata[0].stimulus = new List<nirs.Stimulus>();
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            nirsdata[i].data = new List<double>[nirsdata[i].probe.numChannels];
            for(int ii=0; ii< nirsdata[i].probe.numChannels; ii++)
            {
                nirsdata[i].data[ii] = new List<double>();
            }
            nirsdata[i].time = new List<double>();
            nirsdata[i].stimulus = nirsdata[0].stimulus;
        }




    }

    protected void ClickedStartDAQ(object sender, EventArgs e)
    {
        if (buttonStartDAQ.Label.Equals("Start"))
        {
            buttonStartDAQ.Label = "Stop";
            InitializeData();
            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                MainClass.devices[i].Intialize(nirsdata[i].probe);
            }

            maindisplaythread = new Thread(updatedata);
            maindisplaythread.Start(); // start the timing thread to update the display
            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                MainClass.devices[i].Start();
            }
            colorbutton1.Color = new Gdk.Color(255, 128, 128);
        }
        else
        {
            buttonStartDAQ.Label = "Start";
            maindisplaythread.Abort(); // start the timing thread to update the display
            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                MainClass.devices[i].Stop();
            }
            colorbutton1.Color = new Gdk.Color(128, 128, 128);

            int scancount = 1;
            DateTime now = DateTime.Now;
            nirsdata[0].demographics.set("scan_date", now.ToString("F"));

            string file = String.Format("{0}-scan{1}-{2}.snirf", (string)nirsdata[0].demographics.get("SubjID"),
                scancount, (string)nirsdata[0].demographics.get("scan_date"));

            string[] paths = new string[] {(string)nirsdata[0].demographics.get("Investigator"),
            (string)nirsdata[0].demographics.get("Study"),file};
            string[] pathsfull = new string[] { MainClass.win.settings.DATADIR, (string)nirsdata[0].demographics.get("Investigator"),
            (string)nirsdata[0].demographics.get("Study"),file};

            string filename = System.IO.Path.Combine(paths);

            //TODO nirs.io.writeSNIRF(nirsdata, pathsfull);


            MyTreeNodeData tr = new MyTreeNodeData(filename,"  ");
            nodeview4.NodeStore.AddNode(tr);




        }
    }

    //-----------------------------------------------------------------------
    protected void updatedata()
    {
        // This loop is evoked with the maindisplaythread is started and updates the data display

       // float cnt = 0;
        while (maindisplaythread.IsAlive)
        {
            
            Thread.Sleep(MainClass.win.settings.UPDATETIME);  // update rate (default 500ms)

            // Get data from the instrument
            for (int i=0; i<MainClass.devices.Length; i++)
            {

                nirsdata[i] = MainClass.devices[i].GetNewData(nirsdata[i]);
            }

            drawingarea_Data.QueueDraw();
            drawingarea_Data2.QueueDraw();


            // TODO - replace this with a tic/toc from the system so I can compare to the timing report from the instrument thread
            // TODO - add feedback about buffer overload and loss data in DEBUG mode
       //     cnt += 1;
       //     string msg = string.Format("Running: Time Elapsed: {0:0.0}", cnt / 2);
       //     dispstatus(msg);

            // spin the progress bar... 
            // TODO- make this not look so stupid
            this.progressbar1.Pulse();
            this.progressbar1.QueueDraw();

        }
    }

    protected void RegisterQuickStart(object sender, EventArgs e)
    {

        string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        path = System.IO.Path.Combine(path, "LastSettings.xml");

        // Read the Config.xml file
        XmlDocument doc = new XmlDocument();
      
        doc.Load(path);
        XmlNodeList elemList;

        elemList = doc.GetElementsByTagName("Investigator");
        string investigator = elemList[0].InnerXml.Trim();
        elemList = doc.GetElementsByTagName("Study");
        string study = elemList[0].InnerXml.Trim();
        elemList = doc.GetElementsByTagName("probefile");
        string probefile = elemList[0].InnerXml.Trim();

        nirs.core.Probe probe = nirs.io.LoadProbe(probefile);

        nirsdata = new List<nirs.core.Data>();

        for (int i=0; i<MainClass.devices.Length; i++)
        {
            nirs.core.Data data = new nirs.core.Data();
            data.demographics = new nirs.Dictionary();
            data.demographics.set("SubjID", "");
            data.demographics.set("Investigator", investigator);
            data.demographics.set("Study", study);
            data.demographics.set("Gender", "");
            data.demographics.set("Group", "");
            data.demographics.set("Age", "");
            data.demographics.set("Instrument", MainClass.win.settings.SYSTEM);
            data.demographics.set("head_circumference", "");
            data.demographics.set("Technician", "");
            data.demographics.set("comments", "");
            DateTime now = DateTime.Now;
            data.demographics.set("scan_date", now.ToString("F"));
            data.probe = probe.Clone();
            nirsdata.Add(data);
        }


        Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
        MainClass.win._handles.whichdata.Model = ClearList;

        List<string> datatypes = new List<string>();
        for (int ii = 0; ii < probe.ChannelMap.Length; ii++)
        {
            datatypes.Add(probe.ChannelMap[ii].datasubtype);
        }
        datatypes = datatypes.Distinct().ToList();

        foreach (string s in datatypes)
        {
            MainClass.win._handles.whichdata.AppendText(s);
        }

        MainClass.win._handles.whichdata.Active = 0;




        MainClass.win._handles.SDGplot.QueueDraw();



    }

    protected void MarkStim(object sender, EventArgs e)
    {
        if (!MainClass.devices[0].isrunning())
        {
            return;
        }


        double time = nirsdata[0].time[nirsdata[0].time.Count - 1];
        string condname = comboboxentry_stimtype.ActiveText;

        bool found = false;
        nirs.Stimulus ev = new nirs.Stimulus(); ;
        for (int i = 0; i < nirsdata[0].stimulus.Count; i++) {
            if (nirsdata[0].stimulus[i].name.Equals(condname))
            {
                ev = nirsdata[0].stimulus[i];
                ev.onsets.Add(time);
                ev.amplitude.Add(1);
                ev.duration.Add(1);
                found = true;
                nirsdata[0].stimulus[i] = ev;
            }

        }
        if (!found) {
            ev = new nirs.Stimulus();
            ev.onsets = new List<double>();
            ev.duration = new List<double>();
            ev.amplitude = new List<double>();
            ev.name = condname;
            ev.onsets.Add(time);
            ev.amplitude.Add(1);
            ev.duration.Add(1);
            nirsdata[0].stimulus.Add(ev);
        }

        MyTreeNode tr = new MyTreeNode(ev.name, time, 1, 1);        
        nodeview_stim.NodeStore.AddNode(tr);

        label_numstim.Text = String.Format("Marks: {0}", ev.amplitude.Count);


        
    }


    [Gtk.TreeNode(ListOnly = true)]
    public class MyTreeNode : Gtk.TreeNode
    {

        double onset;
        double duration;
        double amp;


        public MyTreeNode(string name, double onset,double duration,double amp)
        {
            Name = name;
            this.onset = onset;
            this.duration = duration;
            this.amp = amp;
        }

        [Gtk.TreeNodeValue(Column = 0)]
        public string Name;
        [Gtk.TreeNodeValue(Column = 1)]
        public string Onset { get { return String.Format("{0}",onset); } }
        [Gtk.TreeNodeValue(Column = 2)]
        public string Duration { get { return String.Format("{0}", duration); } }
        [Gtk.TreeNodeValue(Column = 3)]
        public string Amplitude { get { return String.Format("{0}", amp); } }
    }




    [Gtk.TreeNode(ListOnly = true)]
    public class MyTreeNodeData : Gtk.TreeNode
    {

        public MyTreeNodeData(string filename, string comments)
        {
            FileName = filename;
            Comments = comments;
        }

        [Gtk.TreeNodeValue(Column = 0)]
        public string FileName;
        [Gtk.TreeNodeValue(Column = 1)]
        public string Comments;
    }






    protected void ToggleStim(object sender, EventArgs e)
    {
        if (!MainClass.devices[0].isrunning())
        {
            return;
        }

        if (!togglebutton_stim.Active)
        {
            togglebutton_stim.Label= comboboxentry_stimtype.ActiveText; 
        }

        bool found = false;
        double time = nirsdata[0].time[nirsdata[0].time.Count - 1];
        string condname = togglebutton_stim.Label;

        nirs.Stimulus ev = new nirs.Stimulus(); ;
        for (int i = 0; i < nirsdata[0].stimulus.Count; i++)
        {
            if (nirsdata[0].stimulus[i].name.Equals(condname))
            {
                ev = nirsdata[0].stimulus[i];
                found = true;
                 
                if (!togglebutton_stim.Active)
                {
                    ev.onsets.Add(time);
                    ev.amplitude.Add(1);
                    ev.duration.Add(999);

                    label_numstim.Text = String.Format("Marks: {0}", ev.amplitude.Count);
                }
                else
                {
                    ev.duration[ev.duration.Count - 1] = time - ev.onsets[ev.onsets.Count - 1];
                }
                nirsdata[0].stimulus[i] = ev;
                
            }

        }
        if (!found)
        {
            if (!togglebutton_stim.Active)
            {
                ev = new nirs.Stimulus();
                ev.onsets = new List<double>();
                ev.duration = new List<double>();
                ev.amplitude = new List<double>();
                ev.name = condname;
                ev.onsets.Add(time);
                ev.amplitude.Add(1);
                ev.duration.Add(999);
                nirsdata[0].stimulus.Add(ev);
                label_numstim.Text = String.Format("Marks: {0}", ev.amplitude.Count);
            }
        }

        



    }




    protected void ReloadData(object sender, EventArgs e)
    {
    }
}



