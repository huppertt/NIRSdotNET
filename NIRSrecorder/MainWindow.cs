using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;



public partial class MainWindow : Window
{
   

    public MainWindow() : base(WindowType.Toplevel)
    {
        Build();
        settings = new NIRSrecorder.Settings();
      
    }

    public void IntializeGUI()
    {

        EnableControls(false);

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
            //colorbutton3.Color = new Gdk.Color(128,255, 128);
            DebugMessage("Connected to Hyperscanning Simulator");
        }
        else  //TODO
        {
            label_deviceConnected.Text = "Connected to TechEn BTNIRS";
            ports.Add("/dev/tty.Dual-SPP-SerialPort");
        }

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


            // remove all pages
        notebook_detectors.RemovePage(0);
        notebook_sources.RemovePage(0);


       

        MainClass.devices = new NIRSDAQ.Instrument.instrument[ports.Count];
        for (int i = 0; i < ports.Count; i++)
        {
            MainClass.devices[i]= new NIRSDAQ.Instrument.instrument(settings.SYSTEM);

            MainClass.devices[i].Connect(ports[i]);

            colorbutton3.Color = new Gdk.Color(128, 255, 128);
            DebugMessage(string.Format("Connected to device {0}", i + 1));

            MainClass.devices[i].devicename= string.Format("{0}-{1}", MainClass.devices[i].devicename, i + 1);
            NIRSDAQ.info _info = MainClass.devices[i].GetInfo();
            
            _info.numDet = settings.system_Info.numdet;
            _info.numSrc = settings.system_Info.numsrc;


            //DevicesInUseAction

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
                    HeightRequest = 5,
                    Sensitive = false
                };

                det.vScale = new VScale(0, settings.system_Info.maxgain, 1)
                {
                    Value = det.gain,
                    ValuePos = PositionType.Bottom,
                    Inverted = true,
                    HeightRequest = 200,
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


        nodeview_stim.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        nodeview_stim.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        nodeview_stim.ButtonPressEvent += StimNode_Clicked;

        combobox_device1.Model = new ListStore(typeof(string));
        combobox_device2.Model = new ListStore(typeof(string));
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            combobox_device1.AppendText(MainClass.devices[i].devicename);
            combobox_device2.AppendText(MainClass.devices[i].devicename);
        }


        nodeview4.AppendColumn("FileName", new CellRendererText(), "text", 0);
        nodeview4.AppendColumn("Comments", new CellRendererText(), "text", 1);
        nodeview4.NodeStore = new NodeStore(typeof(MyTreeNodeData));

        CellRendererText cellRenderer = new CellRendererText();
        cellRenderer.Editable = true;
        cellRenderer.Edited += EditStimTable;
        nodeview_stim.AppendColumn("Name",cellRenderer, "text", 0);
        nodeview_stim.AppendColumn("Onset", cellRenderer, "text", 1);
        nodeview_stim.AppendColumn("Duration", cellRenderer, "text", 2);
        nodeview_stim.AppendColumn("Amplitude", cellRenderer, "text", 3);
        nodeview_stim.NodeStore = new NodeStore(typeof(MyTreeNode));

        nodeviewdemo.AppendColumn("SubjID", new CellRendererText(), "text", 0);
        nodeviewdemo.AppendColumn("Group", new CellRendererText(), "text", 1);
        nodeviewdemo.AppendColumn("Age", new CellRendererText(), "text", 2);
        nodeviewdemo.AppendColumn("Gender", new CellRendererText(), "text", 3);
        nodeviewdemo.AppendColumn("Headsize", new CellRendererText(), "text", 4);
        nodeviewdemo.NodeStore = new NodeStore(typeof(MyTreeNodeDemo));

        CheckBattery();

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

            MenuItem deleteItem = new MenuItem("Remove Event");
            MenuItem deleteItem2 = new MenuItem("Rename Event");

            popup_menu.Add(deleteItem);
            popup_menu.Add(deleteItem2);
            popup_menu.ShowAll();
            popup_menu.Popup();
        }
    }
}



