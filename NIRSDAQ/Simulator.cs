using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

namespace NIRSDAQ
{

    public partial class Instrument
    {

        public partial class Devices
        {

            public class Simulator
            {
                public String Name = "Simulator";
                public String Manufacturer = "Simulator";

                public bool isrunning;

                public bool[] laserstates;
                public int[] laserpower;
                public  int[] detgains;

                public bool usefilter;
                public int sample_rate;

                private Queue[] dataqueue;
                private Thread newthread;

                public int _nsrcs = 32;
                public int _ndets = 32;

                // num measurements
                private int _nmeas = 24;

                public void Initialize(nirs.core.Probe probe)
                {
                    _nmeas = probe.numChannels;
                    dataqueue = new Queue[_nmeas];
                    for (int i = 0; i < _nmeas; i++)
                    {
                        dataqueue[i] = new Queue();
                    }

                }

                public string GetBatteryInfo()
                {
                    return "-----";
                }

                public int getsamplerate()
                {
                    return sample_rate;
                }

                public string portname()
                {
                    return "simulator";
                }

                public int nsrcs()
                {
                    return _nsrcs;
                }
                public int ndets()
                {
                    return _ndets;
                }
                public int nmeas()
                {
                    return _nmeas;
                }

                public void Destroy()
                {
                    
                }

                public Simulator()
                {

                    isrunning = false;
                    laserstates = new bool[_nsrcs];
                    laserpower = new int[_nsrcs];

                    newthread = new Thread(adddata);

                    SetFilter(false);
                    SetSampleRate(20);

                    for (int i = 0; i < _nsrcs; i++)
                    {
                        SetLaserState(i, false);
                        SetLaserPower(i, 0);
                    }
                    detgains = new int[_ndets];
                    for (int i = 0; i < _ndets; i++)
                    {
                        SetDetGain(i, 0);
                    }

                    dataqueue = new Queue[_nmeas];
                    for (int i = 0; i < _nmeas; i++)
                    {
                        dataqueue[i] = new Queue();
                    }

                }

                // List all valid COM ports
                // TODO- handle Windows OS names
                public List<string> ListPorts()
                {
                    string[] ports = SerialPort.GetPortNames();
                    List<string> foundports = new List<string>();
                    foundports.Add("Simulator");
                    return foundports;
                }




                public void Start()
                {
                    newthread = new Thread(adddata);
                    isrunning = true;
                 
                    newthread.Start();
                }


                public void SetSampleRate(int fs)
                {

                    sample_rate = fs;

                }

                public void SetFilter(bool flag)
                {
                    usefilter = flag;

                }

                public double GetBattery()
                {
                    //        fprintf(obj.serialport, sprintf('BAT\r'));%\n
                    //idn = fscanf(obj.serialport);
                    //                      bat = dec2bin(double(idn(1)));
                    //                    obj.battery = 10 * bin2dec(bat(1:4));
                    return 0;
                }


                public void Stop()
                {
                    isrunning = false;
                    newthread.Abort();
                }


                public void SetLaserPower(int sIdx, int pwr)
                {
                    if (pwr < 1) { pwr = 1; }
                    if (pwr > 127) { pwr = 127; }

                    laserpower[sIdx] = pwr;
                }



                public void SetLaserState(int sIdx, bool state)
                {
                    laserstates[sIdx] = state;
                }


                public void SetDetGain(int dIdx, int gain)
                {
                    if (gain < 1) { gain = 1; }
                    if (gain > 127) { gain = 127; }
                    detgains[dIdx] = gain;


                }

                public void AllOn()
                {
                    for (int i = 0; i < _nsrcs; i++)
                    {
                        SetLaserState(i, true);
                    }
                }
                public void AllOff()
                {
                    for (int i = 0; i < _nsrcs; i++)
                    {
                        SetLaserState(i, false);
                    }
                }

                public void FlushBuffer()
                {
                    // does nothing
                }


                // Get Data
                public double[] Getdata()
                {
                    double[] thisdata = new double[_nmeas];
                    for (int i = 0; i < _nmeas; i++)
                    {
                        if (dataqueue[i].Count > 0)
                        {
                            thisdata[i] = (double)dataqueue[i].Dequeue();
                        }
                        else
                        {
                            thisdata[i] = 999;
                        }
                    }
                    return thisdata;
                }


                public int SamplesAvaliable()
                {
                    double cnt = 0;
                    for (int i = 0; i < _nmeas; i++)
                    {
                        cnt = cnt + dataqueue[i].Count;
                    }
                    cnt = Math.Floor(cnt / _nmeas);
                    return (int)cnt;
                }



                ~Simulator()
                {
                    Stop();
                    AllOff();

                }



                public void adddata()
                {
                    try
                    {
                        // TODO
                        int wait;
                        wait = 950 / sample_rate;
                        Random rnd = new Random();

                        while (isrunning)
                        {


                            for (int i = 0; i < _nmeas; i++)
                            {
                                dataqueue[i].Enqueue(rnd.NextDouble() * 100 + i * 10 + 50 * detgains[0]);
                            }
                            Thread.Sleep(wait);
                        }

                    }
                    catch (ThreadAbortException e)
                    {
                        Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                        Console.WriteLine("Exception message: {0}", e.Message);
                        Thread.ResetAbort();
                    }
                }

                // Connect to device
                public bool Connect(string port)
                {

                    return true;
                }


            }
        }
    }
}
