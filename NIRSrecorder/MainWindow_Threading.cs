using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using MathNet.Filtering;
using System.Linq;

public partial class MainWindow : Window
{
    protected void InitializeData()
{

    nirsdata[0].stimulus = new List<nirs.Stimulus>();
    for (int i = 0; i < MainClass.devices.Length; i++)
    {
        nirsdata[i].data = new List<double>[nirsdata[i].probe.ChannelMap.Length];
        for (int ii = 0; ii < nirsdata[i].probe.ChannelMap.Length; ii++)
        {
            nirsdata[i].data[ii] = new List<double>();
        }
        nirsdata[i].time = new List<double>();
        nirsdata[i].stimulus = nirsdata[0].stimulus;
    }

    realtimeEngine = new RealtimeEngine();


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

        maindisplaythread = new Thread(Updatedata);
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

        scancount++;
        DateTime now = DateTime.Now;
        nirsdata[0].demographics.set("scan_date", now.ToString("F"));

        string file = string.Format("{0}-scan{1}-{2}.snirf", (string)nirsdata[0].demographics.get("SubjID"),
            scancount, now.ToString("o"));

        string[] paths = new string[] {(string)nirsdata[0].demographics.get("Investigator"),
            (string)nirsdata[0].demographics.get("Study"),file};
        string[] pathsfull = new string[] { MainClass.win.settings.DATADIR, (string)nirsdata[0].demographics.get("Investigator"),
            (string)nirsdata[0].demographics.get("Study"),file};

        string filename = System.IO.Path.Combine(paths);

        //TODO nirs.io.writeSNIRF(nirsdata, pathsfull);


        MyTreeNodeData tr = new MyTreeNodeData(filename, "  ");
        nodeview4.NodeStore.AddNode(tr);

    }
}


protected void CheckBattery()
{

    int id = combobox_statusBattery.Active;
    if (id < 0) { id = 0; }



    NIRSDAQ.info[] info = new NIRSDAQ.info[MainClass.devices.Length];
    string[] battery = new string[MainClass.devices.Length];


    for (int i = 0; i < MainClass.devices.Length; i++)
    {
        info[i] = MainClass.devices[i].GetInfo();
        battery[i] = MainClass.devices[i].GetBatteryInfo();
    }

    Gtk.ListStore ClearList = new Gtk.ListStore(typeof(string));
    combobox_statusBattery.Model = ClearList;
    for (int i = 0; i < MainClass.devices.Length; i++)
    {
        combobox_statusBattery.AppendText(string.Format("{0} {1}", info[i].PortName, battery[i]));
    }
    combobox_statusBattery.Active = id;


}


    //-----------------------------------------------------------------------
    protected void Updatedata()
    {
        // This loop is evoked with the maindisplaythread is started and updates the data display

        // float cnt = 0;
        while (maindisplaythread.IsAlive)
        {

            Thread.Sleep(MainClass.win.settings.UPDATETIME);  // update rate (default 500ms)

            // Get data from the instrument
            int[] s = new int[MainClass.devices.Length];
            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                s[i] = nirsdata[i].time.Count;
                nirsdata[i] = MainClass.devices[i].GetNewData(nirsdata[i]);
               
            }

            nirsdata = realtimeEngine.UpdateRTengine(nirsdata);

            if (checkbutton_LSLdata.Active)
            {
                for (int i = 0; i < MainClass.devices.Length; i++)
                {

                    int m = nirsdata[i].probe.numChannels * combobox_LSLOutType.Active;
                    for (int j = s[i]; j < nirsdata[i].time.Count; j++)
                    {
                        double[] d = new double[nirsdata[i].probe.numChannels];
                        for (int k = 0; k < d.Length; k++)
                        {
                            d[k] = nirsdata[i].data[k + m][j];
                        }
                        dataLSL[i].push_sample(d, nirsdata[i].time[j]);
                    }
                }
            }
        


            try
            {
                if (checkbutton_LSLStimInlet.Active)
                {
                    if (stimulusInLSL != null)
                    {
                        if (stimulusInLSL.samples_available() > 0)
                        {
                            string[] msg = new string[2];
                            stimulusInLSL.pull_sample(msg);
                            textview_LSLIn.Buffer.Text += string.Format("{0}LSL Inlet Recieved: {1} : {2} (@{3}s)", (char)10, msg[0], msg[1],
                                nirsdata[0].time[nirsdata[0].time.Count-1]);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("LSL read failed");
            }






            drawingarea_Data.QueueDraw();
            drawingarea_Data2.QueueDraw();

            progressbar1.Pulse();
            progressbar1.QueueDraw();

        }
    }

    protected void ChangeBPF(object sender, EventArgs e)
    {

        if (maindisplaythread.IsAlive)
        {
            bool usehpf = MainClass.win._handles.useHPF.Active;
            bool uselpf = MainClass.win._handles.useLPF.Active;
            double hpf = Convert.ToDouble(MainClass.win._handles.editHPF.Text);
            double lpf = Convert.ToDouble(MainClass.win._handles.editLPF.Text);

            double[] fs = new double[MainClass.devices.Length];

            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                NIRSDAQ.info info = MainClass.devices[i].GetInfo();
                fs[i] = info.sample_rate;

                for (int j = 0; j < MainClass.win.nirsdata[i].probe.numChannels; j++)
                {
                    if (uselpf & usehpf)
                    {
                        realtimeEngine.OnlineFIRFiltersBPF[i][j] = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, fs[i], hpf, lpf);
                    }
                    else if (!uselpf & usehpf)
                    {
                        realtimeEngine.OnlineFIRFiltersBPF[i][j] = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, fs[i], hpf);
                    }
                    else if (uselpf & !usehpf)
                    {
                        realtimeEngine.OnlineFIRFiltersBPF[i][j] = OnlineFilter.CreateLowpass(ImpulseResponse.Finite, fs[i], lpf);
                    }
                    else
                    {
                        realtimeEngine.OnlineFIRFiltersBPF[i][j] = OnlineFilter.CreateDenoise(4);
                    }
                }
            }
        }
    }
}