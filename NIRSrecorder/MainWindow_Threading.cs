using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using MathNet.Filtering;
using MathNet.Numerics.IntegralTransforms;
using System.IO;

public partial class MainWindow : Window
{
    protected void InitializeData()
    {


        nirsdata[0].stimulus = new List<nirs.Stimulus>();
        int cnt = 0;
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            cnt += nirsdata[i].probe.numChannels / nirsdata[i].probe.numWavelengths;
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

            SCIupdate = new Thread(UpdateSCI);

            maindisplaythread = new Thread(Updatedata);
            maindisplaythread.Start(); // start the timing thread to update the display
            SCIupdate.Start();
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

            for (int i = 0; i < nirsdata.Count; i++)
            {

                NIRSDAQ.info info = MainClass.devices[i].GetInfo();


                nirsdata[i].demographics.set("scan_date", now.ToString("MM-dd-yyyy_HH:mm:ss"));
                nirsdata[i].demographics.set("device", info.DeviceName);
                nirsdata[i].demographics.set("manufacturer", info.Manufacturer);
                nirsdata[i].demographics.set("port", info.PortName);

                string test=(string)nirsdata[i].demographics.get("SubjID");
                if (test.Equals(""))
                {
                    nirsdata[i].demographics.set("SubjID","unknown");
                }


                string file = string.Format("{0}_scan{1}_{2}", (string)nirsdata[i].demographics.get("SubjID"),
                    scancount, now.ToString("MMMMddyyyy_HHmm"));

                if (nirsdata.Count > 1) {
                    file = string.Format("{0}_device{1}_scan{2}_{3}", (string)nirsdata[i].demographics.get("SubjID"),
                    i+1,scancount, now.ToString("MMMMddyyyy_HHmm"));
                }

                
            string[] paths = new string[] {MainClass.win.settings.DATADIR,
                (string)nirsdata[i].demographics.get("Investigator"),
                (string)nirsdata[i].demographics.get("Study"),
                now.ToString("MMMMddyyyy")};

                string pathname = System.IO.Path.Combine(paths);

                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }

                if (SaveNirsFormatAction.Active)
                {
                    string filename = System.IO.Path.Combine(pathname, string.Format("{0}.nirs", file));
                    nirs.io.writeDOTnirs(nirsdata[i], filename);
                    _handles.dataListStore.AppendValues(string.Format("{0}.nirs", file), "  ");
                }

                if (SaveSnirfFormatAction.Active & !CombineSnirfFilesAction.Active)
                {
                    string filename = System.IO.Path.Combine(pathname, string.Format("{0}.snirf", file));
                    nirs.io.writeSNIRF(nirsdata[i], filename);
                    _handles.dataListStore.AppendValues(string.Format("{0}.snirf", file), "  ");
                }

                


            }

            if (SaveSnirfFormatAction.Active & CombineSnirfFilesAction.Active)
            {

                string file = string.Format("{0}_scan{1}_{2}", "Hyperscan",
                  scancount, now.ToString("MMMMddyyyy_HHmm"));

                string[] paths = new string[] {MainClass.win.settings.DATADIR,
                (string)nirsdata[0].demographics.get("Investigator"),
                (string)nirsdata[0].demographics.get("Study"),
                now.ToString("MM_dd_yyyy")};

                string pathname = System.IO.Path.Combine(paths);

                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }



                string filename = System.IO.Path.Combine(pathname, string.Format("{0}.snirf", file));
                nirs.io.writeSNIRF(nirsdata, filename);
                _handles.dataListStore.AppendValues(string.Format("{0}.snirf", file), "  ");
               
            }


        }
    }


    protected void UpdateSCI()
    {
        int nFFT = 32;
        while (maindisplaythread.IsAlive)
        {
            Thread.Sleep(3000);  // update rate (default 500ms)

            for(int i=0; i<nirsdata.Count; i++)
            {

                if (nirsdata[i].data[0].Count > nFFT+5)
                {
                    double fs = MainClass.devices[i].GetSampleRate();
                    double[] SCI = new double[realtimeEngine.mBLLmappings[i].distances.Length];

                    for (int ch = 0; ch < realtimeEngine.mBLLmappings[i].distances.Length; ch++)
                    {
                        MathNet.Numerics.Complex32[] w1 = new MathNet.Numerics.Complex32[nFFT];
                        MathNet.Numerics.Complex32[] w2 = new MathNet.Numerics.Complex32[nFFT];
                        int ntps = nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][0]].Count;
                        double a = 0;
                        double b = 0;
                        int cc = 0;
                        for (int tpt = ntps - nFFT; tpt < ntps; tpt++)
                        {
                            w1[cc] = new MathNet.Numerics.Complex32((float)nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][0]][tpt], 0f);
                            w2[cc] = new MathNet.Numerics.Complex32((float)nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][1]][tpt], 0f);
                            a += nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][0]][tpt] * nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][0]][tpt];
                            b += nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][1]][tpt] * nirsdata[i].data[realtimeEngine.mBLLmappings[i].measurementPairs[ch][1]][tpt];
                            cc++;
                        }
                        Fourier.Forward(w1);
                        Fourier.Forward(w2);
                        cc = 0;
                        for (double ii = -fs / 2; ii < fs / 2; ii += fs / nFFT)
                        {
                            if (Math.Abs(ii) < 0.5 | Math.Abs(ii) > 2)
                            {
                                w1[cc] = new MathNet.Numerics.Complex32(0f, w1[cc].Imaginary);
                                w2[cc] = new MathNet.Numerics.Complex32(0f, w2[cc].Imaginary);

                            }
                            w1[cc] = w1[cc] * w2[cc].Conjugate();
                        }
                        Fourier.Inverse(w1);
                        SCI[ch] = w1[(int)Math.Round((double)nFFT / 2)].Real / Math.Sqrt(a * b);

                    }

                    double[] avgDetVal = new double[nirsdata[i].probe.numDet];
                    int[] cnt = new int[nirsdata[i].probe.numDet];
                    for (int ch = 0; ch < nirsdata[i].probe.numDet; ch++)
                    {
                        cnt[ch] = 0;
                        avgDetVal[ch] = 0;
                    }
                    for (int ch = 0; ch < nirsdata[i].probe.numChannels / nirsdata[i].probe.numWavelengths; ch++)
                    {
                        int dIDx = nirsdata[i].probe.ChannelMap[ch].detectorindex;
                        cnt[dIDx]++;
                        avgDetVal[dIDx] += SCI[ch];
                    }
                    for (int ch = 0; ch < nirsdata[i].probe.numDet; ch++)
                    {
                        Gdk.Color col;
                        if (SCI[ch] < .3)
                        {
                            col = new Gdk.Color(255, 0, 0);
                        }
                        else if (SCI[ch] < .6)
                        {
                            col = new Gdk.Color(255, 255, 0);
                        }else 
                        {
                            col = new Gdk.Color(0,255,0);
                        }

                        MainClass.win._handles.detectors[ch].led.Color = col;


                    }



                }

            }



        }
    }

    protected void CheckBattery()
    {

        if (MainClass.devices == null)
        {
            return;
        }

        int id = combobox_statusBattery.Active;
        if (id < 0) { id = 0; }

        try
        {
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
        catch
        {
        }
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
                    #if ADDLSL
                        dataLSL[i].push_sample(d, nirsdata[i].time[j]);
                    #endif
                    }
                }
            }


            #if ADDLSL
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
                                nirsdata[0].time[nirsdata[0].time.Count - 1]);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("LSL read failed");
            }
            #endif





            drawingarea_Data.QueueDraw();
            drawingarea_Data2.QueueDraw();

            progressbar1.Pulse();
            progressbar1.QueueDraw();

        }
    }

    protected void ChangeBPF(object sender, EventArgs e)
    {

        double[] fs = new double[MainClass.devices.Length];
        for (int i = 0; i < MainClass.devices.Length; i++)
        {
            NIRSDAQ.info info = MainClass.devices[i].GetInfo();
            fs[i] = info.sample_rate;
        }

        double hpf=0.016;
        double lpf=0.5;

        try
        {
            lpf = Convert.ToDouble(MainClass.win._handles.editLPF.Text);
            if (lpf > fs[0] / 2)
            {
                lpf = fs[0] * 5 / 11;
                MainClass.win._handles.editLPF.Text = string.Format("{0}", lpf);
            }
        }
        catch
        {
            MainClass.win._handles.editLPF.Text = string.Format("{0}", 0.5);
            lpf = 0.5;
        }

        try
        {
            hpf = Convert.ToDouble(MainClass.win._handles.editHPF.Text);
            if (hpf < 0)
            {
                hpf = 0;
                MainClass.win._handles.editHPF.Text = string.Format("{0}",0);
            }
            if (hpf > lpf)
            {
                hpf = lpf/2;
                MainClass.win._handles.editHPF.Text = string.Format("{0}", hpf);
            }
        }
        catch
        {
            MainClass.win._handles.editHPF.Text = string.Format("{0}",0.016);
            hpf = 0.016;
        }


          


        if (maindisplaythread.IsAlive)
        {
            bool usehpf = MainClass.win._handles.useHPF.Active;
            bool uselpf = MainClass.win._handles.useLPF.Active;
            

            for (int i = 0; i < MainClass.devices.Length; i++)
            {
                

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