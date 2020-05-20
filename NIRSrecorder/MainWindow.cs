using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using LSL;


public partial class MainWindow : Window
{
   

    public MainWindow() : base(WindowType.Toplevel)
    {
        Build();
        settings = new NIRSrecorder.Settings();
        MainClass.obj_Splash.label.Text = "Loading GUI";
 
    }

    public void IntializeGUI()
    {

        
        MainClass.obj_Splash.label.Text = string.Format("Finding Devices: {0}", settings.SYSTEM);
        MainClass.obj_Splash.QueueDraw();
        MainClass.obj_Splash.ShowNow();

        #if !ADDLSL
            checkbutton_LSLStimInlet.Sensitive = false;
            checkbutton_LSLStimOutlet.Sensitive = false;
            combobox_LSLOutType.Sensitive = false;
            combobox_selectLSLStimInlet.Sensitive = false;
            textview_LSLIn.Sensitive = false;
        #endif


        // For now
        // This will allow me to handle multiple devices (eventually).  For now, just default to 1
        List<string> ports = new List<string>();
        if (settings.SYSTEM.Trim().ToLower().Equals("simulator"))
        {
            ports.Add("simulator");
            label_deviceConnected.Text = "Connected to Simulator";
            //colorbutton3.Color = new Gdk.Color(128, 255, 128);
            DebugMessage("Connected to Simulator");
        }
        else if (settings.SYSTEM.Trim().ToLower().Equals("simulatorhyperscan"))
        {
            settings.SYSTEM = "Simulator";
            ports.Add("1");
            ports.Add("2");
            label_deviceConnected.Text = "Connected to Dual-Simulator";
            DebugMessage("Connected to Hyperscanning Simulator");
        }
        else  
        {
            NIRSDAQ.Instrument.Devices.TechEn.BTnirs bTnirs = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
            ports = bTnirs.ListPorts();
            MainClass.obj_Splash.label.Text = string.Format("Found {0} devices", ports.Count);
            MainClass.obj_Splash.ShowNow();
            if (ports.Count == 0)
            {
                label_deviceConnected.Text = "No Devices found";
            }
            else
            {
                label_deviceConnected.Text = "Connected to TechEn BTNIRS";
            }
        }

        EnableControls(false);

        scancount = 0;

        _handles = new Handles
        {
            detectors = new List<Detector>(),
            lasers = new List<Lasers>(),

            Dataplot = drawingarea_Data,
            SDGplot = drawingarea_SDG,
            whichdata = combobox_whichdata,

            useHPF = checkbutton_hpf,
            useLPF = checkbutton_lpf,
            useMOCO = checkbutton_moco,
            editHPF = entry_hpf,
            editLPF = entry_lpf
        };

        nirsdata = new List<nirs.core.Data>();
        colorbutton1.Color = new Gdk.Color(128, 128, 128);


        if (ports.Count == 0)
        {
            NewSubjectAction.Sensitive = false;
            QuickStartAction.Sensitive = false;
        }
        else
        {
           SetupGUI(ports);
        }


        

        Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
        combobox_statusBattery.Model = ClearList;
        combobox_statusBattery.AppendText("---------");
        combobox_statusBattery.Active = 0;

        batteryCheck = new Thread(CheckBatteryWrapper);
        batteryCheck.Start();

        ShowSystemMessagingAction.Active = settings.DEBUG;

        drawingarea_SDG.ExposeEvent += Sdgdraw;
        drawingarea_Data.ExposeEvent += Datadraw;
        drawingarea_SDG.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        drawingarea_SDG.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        drawingarea_SDG.ButtonReleaseEvent += ClickSDG;
        drawingarea_SDG2.ExposeEvent += Sdgdraw;
        drawingarea_Data2.ExposeEvent += Datadraw;
        drawingarea_SDG2.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        drawingarea_SDG2.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        drawingarea_SDG2.ButtonReleaseEvent += ClickSDG2;

        _handles.DataTree = new TreeView();
        PlaceHolder_Data.Add(_handles.DataTree);
        Gtk.TreeViewColumn fileColumn = new Gtk.TreeViewColumn();
        fileColumn.Title = "File Name";
        _handles.DataTree.AppendColumn(fileColumn);
        Gtk.TreeViewColumn commentsColumn = new Gtk.TreeViewColumn();
        commentsColumn.Title = "Comments";
        _handles.DataTree.AppendColumn(commentsColumn);
        _handles.dataListStore = new Gtk.ListStore(typeof(string), typeof(string));
        _handles.DataTree.Model = _handles.dataListStore;
        Gtk.CellRendererText fileNameCell = new Gtk.CellRendererText();
        fileColumn.PackStart(fileNameCell, true);
        Gtk.CellRendererText commentCell = new Gtk.CellRendererText();
        commentsColumn.PackStart(commentCell, true);
        fileColumn.AddAttribute(fileNameCell, "text", 0);
        commentsColumn.AddAttribute(commentCell, "text", 1);
        commentCell.Editable = true;


        _handles.StimTree = new TreeView();
        PlaceHolderStim.Add(_handles.StimTree);
        Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn();
        nameColumn.Title = "Name";
        _handles.StimTree.AppendColumn(nameColumn);
        Gtk.TreeViewColumn onsetColumn = new Gtk.TreeViewColumn();
        onsetColumn.Title = "Onset";
        _handles.StimTree.AppendColumn(onsetColumn);
        Gtk.TreeViewColumn durColumn = new Gtk.TreeViewColumn();
        durColumn.Title = "Dur";
        _handles.StimTree.AppendColumn(durColumn);
        Gtk.TreeViewColumn ampColumn = new Gtk.TreeViewColumn();
        ampColumn.Title = "Amp";
        _handles.StimTree.AppendColumn(ampColumn);
        _handles.stimListStore = new Gtk.ListStore(typeof(string), typeof(double), typeof(double), typeof(double));
        _handles.StimTree.Model = _handles.stimListStore;


        Gtk.CellRendererText nameCell = new Gtk.CellRendererText();
        nameColumn.PackStart(nameCell, true);
        Gtk.CellRendererText onsetCell = new Gtk.CellRendererText();
        onsetColumn.PackStart(onsetCell, true);
        Gtk.CellRendererText durCell = new Gtk.CellRendererText();
        durColumn.PackStart(durCell, true);
        Gtk.CellRendererText ampCell = new Gtk.CellRendererText();
        ampColumn.PackStart(ampCell, true);
        nameCell.Editable = true;
        onsetCell.Editable = true;
        durCell.Editable = true;
        ampCell.Editable = true;

      
        nameColumn.AddAttribute(nameCell, "text", 0);
        onsetColumn.AddAttribute(onsetCell, "text", 1);
        durColumn.AddAttribute(durCell, "text", 2);
        ampColumn.AddAttribute(ampCell, "text", 3);



        _handles.StimTree.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        _handles.StimTree.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        _handles.StimTree.ButtonPressEvent += StimNode_Clicked;



        CheckBattery();

#if ADDLSL
        liblsl.StreamInfo inf = new liblsl.StreamInfo("NIRSRecordIREvents", "Markers",2,liblsl.IRREGULAR_RATE,
                                                      liblsl.channel_format_t.cf_string);
        stimulusLSL = new liblsl.StreamOutlet(inf);

        liblsl.StreamInfo[] results = liblsl.resolve_streams();
        if (results.Length > 0)
        {
           ClearList = new Gtk.ListStore(typeof(string));

            combobox_selectLSLStimInlet.Model = ClearList;
            for (int i=0; i<results.Length; i++)
            {
                combobox_selectLSLStimInlet.AppendText(string.Format("{0}:{1}",results[i].name(),results[i].hostname()));
            }
        }
        else
        {
            combobox_selectLSLStimInlet.Sensitive = false;
            checkbutton_LSLStimInlet.Active = false;
        }
#endif

        ShowAll();

    }


    public void SetupGUI(List<string> ports)
    {

        MainClass.win.settings.LoadSettingsSystem();

        // remove all pages
        int n = notebook_detectors.NPages;
        for (int i = n-1; i >-1; i--)
        {
            notebook_detectors.RemovePage(i);
        }
        n = notebook_sources.NPages;
        for (int i = n - 1; i > -1; i--)
        {
            notebook_sources.RemovePage(i);
        }

        if (ports.Count > 0)
        {
            DeviceOptionsAction.Sensitive = true;
        }

        List<string> ports2 = new List<string>();
        for(int i=0; i<ports.Count; i++)
        {
            ports2.Add(ports[i]);
        }


        // If the device already has some connected
        if (MainClass.devices!=null)
        {
            NIRSDAQ.Instrument.instrument[] dev = new NIRSDAQ.Instrument.instrument[ports.Count];
            int cnt = 0;
            for(int j=0; j<MainClass.devices.Length; j++)
            {
                NIRSDAQ.info _info = MainClass.devices[j].GetInfo();
                if (ports2.Contains(_info.PortName))
                {
                    ports2.Remove(_info.PortName);
                    dev[cnt] = MainClass.devices[j];
                    cnt++;
                }
                else
                {
                    MainClass.devices[j].Disconnect();
                }
            }

            for (int i = 0; i < ports2.Count; i++)
            {
                MainClass.devices[cnt+i] = new NIRSDAQ.Instrument.instrument(settings.SYSTEM);
                MainClass.devices[cnt + i].Connect(ports2[i]);
                colorbutton3.Color = new Gdk.Color(128, 255, 128);
                DebugMessage(string.Format("Connected to device {0}", cnt + 1));

            }

        }
        else
        {
            MainClass.devices = new NIRSDAQ.Instrument.instrument[ports2.Count];
            for (int i = 0; i < ports2.Count; i++)
            {
                MainClass.devices[i] = new NIRSDAQ.Instrument.instrument(settings.SYSTEM);
               MainClass.devices[i].Connect(ports2[i]);
               colorbutton3.Color = new Gdk.Color(128, 255, 128);
                DebugMessage(string.Format("Connected to device {0}", i + 1));
            }

        }



        
        for (int i = 0; i < ports.Count; i++)
        {


            MainClass.devices[i].devicename = string.Format("{0}-{1}", MainClass.devices[i].devicename, i + 1);
            NIRSDAQ.info _info = MainClass.devices[i].GetInfo();

            _info.numDet = settings.system_Info.numdet;
            _info.numSrc = settings.system_Info.numsrc;

            
                               
            HBox hBox = new HBox(true, 0);
            Label label = new Label();
            for (int j = 0; j < _info.numDet; j++)
            {
                if (j % 8 == 0)
                {
                    hBox = new HBox(true, 0);
                    label = new Label(string.Format("{0}\n{1}-{2}", _info.DeviceName, j + 1, j + 8))
                    {
                        LineWrap = true
                    };
                    notebook_detectors.InsertPage(hBox, label, notebook_detectors.NPages);
                }
                Detector det = new Detector
                {
                    detectorIdx = j,
                    deviceIdx = i,
                    gain = 0,
                    name = string.Format("Det-{0}", j + 1)
                };

                det.frame = new Frame(det.name);
                VBox _box = new VBox(false, 0);
                det.led = new ColorButton(new Gdk.Color(0, 255, 0))
                {
                    HeightRequest = 25,
                    Sensitive = false
                };

                det.vScale = new VScale(0, settings.system_Info.maxgain, 1)
                {
                    Value = det.gain,
                    ValuePos = PositionType.Bottom,
                    Inverted = true,
                    HeightRequest = 175,
                    Name = string.Format("{0}", _handles.detectors.Count)
                };
                det.vScale.ValueChanged += DetChanged;

                _box.PackStart(det.led);
                _box.PackStart(det.vScale);
                det.frame.Add(_box);
                hBox.PackStart(det.frame);

                _handles.detectors.Add(det);

            }

            VBox _vbx = new VBox();
            // Now add the source controls
            for (int j = 0; j < _info.numSrc; j += _info.numwavelengths)
            {
                if (j % 8 == 0)
                {
                    _vbx = new VBox(true, 0);
                    label = new Label(string.Format("{0}\n{1}-{2}", _info.DeviceName, j + 1, j + 8))
                    {
                        LineWrap = true
                    };
                    notebook_sources.InsertPage(_vbx, label, notebook_sources.NPages);
                }
                int sIdx = j / _info.numwavelengths + 1;
                Frame frame = new Frame(string.Format("Source-{0}", sIdx));
                HBox _hbx = new HBox(true, 0);
                Lasers src = new Lasers
                {
                    laserIdx = new int[_info.numwavelengths],
                    wavelength = new int[_info.numwavelengths],
                    gain = new int[_info.numwavelengths],
                    state = new bool[_info.numwavelengths],
                    buttons = new Button[_info.numwavelengths],
                    spinButtons = new SpinButton[_info.numwavelengths],
                    deviceIdx = i,
                    frame = frame
                };
                for (int k = 0; k < _info.numwavelengths; k++)
                {
                    src.laserIdx[k] = j + k;
                    src.wavelength[k] = _info.wavelengths[k];
                    src.gain[k] = 0;
                    src.state[k] = false;
                    src.name = string.Format("{0}nm", src.wavelength[k]);
                    HBox _hbx2 = new HBox(true, 0);
                    src.buttons[k] = new Button
                    {
                        Label = src.name
                    };
                    _hbx2.PackStart(src.buttons[k]);
                    src.spinButtons[k] = new SpinButton(0, settings.system_Info.maxpower, 1)
                    {
                        Value = 0
                    };
                    src.buttons[k].Name = string.Format("{0}", _handles.lasers.Count);
                    src.spinButtons[k].Name = string.Format("{0} {1}", _handles.lasers.Count, k);
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

            DebugMessage("Last Settings Avaliable  " + investigator + " : " + study);

        }
        else
        {
            QuickStartAction.Sensitive = false;
        }


        combobox_device1.Model = new ListStore(typeof(string));
        combobox_device2.Model = new ListStore(typeof(string));
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            combobox_device1.AppendText(MainClass.devices[i].devicename);
            combobox_device2.AppendText(MainClass.devices[i].devicename);
        }



        ShowAll();

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
            MultipleDevicesAction.Sensitive = false;
            drawingarea_Data2.Hide();
            drawingarea_SDG2.Hide();
            combobox_device1.Active = 0;
            DualViewAction.Sensitive = false;
            SingleViewAction.Sensitive = false;
        }
        else
        {
            //   fixed_device1.Hide();
            fixed_device2.Hide();

            combobox_device1.Active = 0;
            combobox_device2.Active = 1;

            //   combobox_device1.Hide();
            combobox_device2.Hide();
            MultipleDevicesAction.Sensitive = true;
            SingleViewAction.Active = false;
            drawingarea_Data2.Hide();
            drawingarea_SDG2.Hide();
        }
        if (MainClass.devices.Length > 0)
        {
            NewSubjectAction.Sensitive = true;
        }
        
    }


    public void EnableControls(bool flag)
    {
        buttonStartDAQ.Sensitive = flag;
        button_srcOnOff.Sensitive = flag;
        combobox_whichdata.Sensitive = flag;
        combobox_selectview.Sensitive = flag;
        combobox_device1.Sensitive = flag;
        combobox_device2.Sensitive = flag;
        comboboxentry_stimtype.Sensitive = flag;
        togglebutton_stim.Sensitive = flag;
        notebook1.Sensitive = flag;
        notebook_detectors.Sensitive = flag;
        notebook_sources.Sensitive = flag;
        checkbutton_autoscaleY.Sensitive = flag;
        checkbutton_timeWindow.Sensitive = flag;
        button_markevent.Sensitive = flag;
        button_autoadjust.Sensitive = flag;
        entry_timeWindow.Sensitive = flag;
        MultipleDevicesAction.Sensitive = flag;

        

    }

    protected void ConnectDevices(object sender, EventArgs e)
    {
        NIRSrecorder.ConnectDevices connectDevices = new ConnectDevices();
        connectDevices.Run();

    }

    protected void DeviceOptions(object sender, EventArgs e)
    {
    }


    protected void DeviceDebugging(object sender, EventArgs e)
    {
    }

    [GLib.ConnectBeforeAttribute]
    protected void StimNode_Clicked(object o, ButtonPressEventArgs args)
    {
        if (args.Event.Button == 3)
        { /* right click */
            Gtk.Menu popup_menu = new Gtk.Menu();

            MenuItem removeEvent = new MenuItem("Remove Event");
            MenuItem addEvent = new MenuItem("Add New Event");
            removeEvent.ButtonReleaseEvent += RemoveEvent_ButtonReleaseEvent;
            addEvent.ButtonReleaseEvent += AddEvent_ButtonReleaseEvent;
            popup_menu.Add(removeEvent);
            popup_menu.Add(addEvent);
            popup_menu.ShowAll();
            popup_menu.Popup();
        }
    }
   
    private void AddEvent_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
       //TODO
    }

    private void RemoveEvent_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
        //TODO
        drawingarea_Data.QueueDraw();
        drawingarea_Data2.QueueDraw();




    }

    protected void ToggleLSLStimInlet(object sender, EventArgs e)
    {
        if (combobox_selectLSLStimInlet.Active < 0)
        {
            checkbutton_LSLStimInlet.Active = false;
            return;
        }
#if ADDLSL
            int dIDX = combobox_selectLSLStimInlet.Active;
            liblsl.StreamInfo[] results = liblsl.resolve_streams();
            stimulusInLSL = new liblsl.StreamInlet(results[dIDX]);
#endif

    }

    protected void AutoAdjust(object sender, EventArgs e)
    {
    }

    protected void EditTimeWinChanged(object sender, EventArgs e)
    {
        try
        {
            double val = Convert.ToDouble(entry_timeWindow.Text);
            if (val<1) {
                entry_timeWindow.Text = string.Format("{0}", 1);
            }
        }
        catch
        {
            entry_timeWindow.Text = string.Format("{0}", 30);
        }
    }




    protected void ChangedDemoDevice(object sender, EventArgs e)
    {
        int dID = comboboxdeviceDemo.Active;
        if (dID < 0)
        {
            return;
        }

        entrysubjid.Text = (string)nirsdata[dID].demographics.get("SubjID");
        entryage.Text = (string)nirsdata[dID].demographics.get("Age");
        entryheadcirm.Text = (string)nirsdata[dID].demographics.get("head_circumference");
        entry1.Text = (string)nirsdata[dID].demographics.get("Group");

        string gender = (string)nirsdata[dID].demographics.get("Gender");

        ListStore store = (ListStore)comboboxentrygender.Model;
        int index = 0;
        bool found = false;
        foreach (object[] row in store)
        {
            // Check for match
            if (gender == row[0].ToString())
            {
                comboboxentrygender.Active = index;
                found = true;
                break;
            }
            // Increment the index so we can reference it for the active.
            index++;
        }
        if (!found)
        {
            comboboxentrygender.AppendText(gender);
            comboboxentrygender.Active = index;
        }

        string custom1 = entrycustom1Name.Text;
        string custom2 = entrycustom2Name.Text;
        string custom3 = entryCustom3Name.Text;

        entryCustom1Val.Text = (string)nirsdata[dID].demographics.get(custom1);
        entryCustom2Val.Text = (string)nirsdata[dID].demographics.get(custom2);
        entryCustom3Val.Text = (string)nirsdata[dID].demographics.get(custom3);

        
    }

    protected void DemoIDdevice(object sender, EventArgs e)
    {

        int dID = comboboxdeviceDemo.Active;
        if (dID < 0)
        {
            return;
        }

        try
        {
            MainClass.devices[dID].IDmode(true);
            Thread.Sleep(3000);
            MainClass.devices[dID].IDmode(false);
        }
        catch { }



    }

    protected void ChangedDemo(object sender, EventArgs e)
    {


        int dID = comboboxdeviceDemo.Active;
        if (dID < 0)
        {
            return;
        }


        nirsdata[dID].demographics.set("SubjID",entrysubjid.Text);
        nirsdata[dID].demographics.set("Age",entryage.Text);
        nirsdata[dID].demographics.set("head_circumference",entryheadcirm.Text);
        nirsdata[dID].demographics.set("Group",entry1.Text);
        nirsdata[dID].demographics.set("Gender",comboboxentrygender.ActiveText);

        string custom1 = entrycustom1Name.Text;
        string custom2 = entrycustom2Name.Text;
        string custom3 = entryCustom3Name.Text;

        nirsdata[dID].demographics.set(custom1, entryCustom1Val.Text);
        nirsdata[dID].demographics.set(custom2, entryCustom2Val.Text);
        nirsdata[dID].demographics.set(custom3, entryCustom3Val.Text);

    }
}



