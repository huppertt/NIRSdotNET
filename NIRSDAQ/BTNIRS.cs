using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace NIRSDAQ
{

    public partial class Instrument
    {

        public partial class Devices
        {
            public partial class TechEn
            {
                public class BTnirs
                {
                    public String Name = "BTnirs";
                    public String Manufacturer = "TechEn";
                    public static bool isrunning;
                    public bool isconnected;

                    public bool[] laserstates;
                    public int[] laserpower;
                    public int[] detgains;

                    public bool usefilter;
                    public static int sample_rate;

                    private static SerialPort _serialPort;
                    private static Queue[] dataqueue;
                    private static Thread newthread;

                    private static int wordsperrecord;

                    public static readonly int _nsrcs = 8;
                    public static readonly int _ndets = 6;

                    // num measurements
                    private static readonly int _nmeas = 24;



                    public string portname()
                    {
                        return _serialPort.PortName;
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

                    public BTnirs()
                    {
                        _serialPort = new SerialPort();

                        isrunning = false;
                        laserstates = new bool[_nsrcs];
                        laserpower = new int[_nsrcs];

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
                        foreach (string s in ports)
                        {
                            if (s.Contains("SerialPort"))
                            {
                                foundports.Add(s);
                            }
                        }
                        return foundports;
                    }

                    public void Start()
                    {

                        isrunning = true;
                        newthread = new Thread(adddata);
                        newthread.Start();

                        // flush the Serial buffer
                        FlushBuffer();

                        isrunning = SendCommMsg("RUN");
                    }


                    public void SetSampleRate(int fs)
                    {

                        fs = (int)(Math.Round((double)fs / 10) * 10);
                        if (fs < 10)
                        {
                            fs = 10;
                        }
                        if (fs > 80)
                        {
                            fs = 80;
                        }

                        sample_rate = fs;
                        SendCommMsg(String.Format("SSR {0}", fs));

                        wordsperrecord = 5 + 64 * fs / 10 + 11;
                    }

                    public void SetFilter(bool flag)
                    {
                        usefilter = flag;
                        if (flag)
                        {
                            SendCommMsg("SFT 1");
                        }
                        else
                        {
                            SendCommMsg("SFT 0");
                        }

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
                        SendCommMsg("STP");
                        newthread.Abort();
                    }


                    public void SetLaserPower(int sIdx, int pwr)
                    {
                        if (pwr < 1) { pwr = 1; }
                        if (pwr > 127) { pwr = 127; }

                        laserpower[sIdx] = pwr;
                        SendCommMsg(string.Format("SLE {0} {1}", sIdx, pwr));
                    }



                    public void SetLaserState(int sIdx, bool state)
                    {
                        laserstates[sIdx] = state;
                        if (state)
                        {
                            SendCommMsg(String.Format("SSO {0} 1", sIdx));
                        }
                        else
                        {
                            SendCommMsg(String.Format("SSO {0} 0", sIdx));
                        }


                    }
                    public void SetDetGain(int dIdx, int gain)
                    {
                        if (gain < 1) { gain = 1; }
                        if (gain > 127) { gain = 127; }
                        detgains[dIdx] = gain;

                        SendCommMsg(String.Format("SDG {0} {1}", dIdx, gain));

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
                        while (_serialPort.BytesToRead > 0)
                        {
                            _serialPort.DiscardInBuffer();
                        }

                    }


                    // Get Data from the instrument and place in data queue
                    public double[] Getdata()
                    {
                        double[] thisdata = new double[_nmeas];
                        for (int i = 0; i < _nmeas; i++)
                        {
                            if (dataqueue[i].Count > 0)
                            {
                                thisdata[i] = (double)dataqueue[i].Dequeue();
                            }
                        }
                        return thisdata;
                    }


                    // Return the number of samples in the data queue
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


                    // deconstructor
                    ~BTnirs()
                    {
                        Stop();
                        AllOff();
                        _serialPort.Close();
                        _serialPort.Dispose();

                    }



                    public static void adddata()
                    {
                        // TODO
                        int wait;
                        wait = 500 / sample_rate;
                        int cnt = 0;
                        char[] line = new char[wordsperrecord];
                        while (isrunning)
                        {
                            for (int i = 0; i < _nmeas; i++)
                            {
                                double value = 0;
                                value += 16 * 16 * 16 * char2val(line[10 + i * 5]);
                                value += 16 * 16 * char2val(line[10 + i * 5 + 1]);
                                value += 16 * char2val(line[10 + i * 5 + 2]);
                                value += char2val(line[10 + i * 5 + 3]);
                                dataqueue[i].Enqueue(value);
                            }


                            cnt = _serialPort.BytesToRead;
                        }
                        Thread.Sleep(wait);
                    }

                    // helper function
                    static double char2val(char a)
                    {
                        switch (a.ToString())
                        {
                            case "0":
                                return 0;
                            case "1":
                                return 1;
                            case "2":
                                return 2;
                            case "3":
                                return 3;
                            case "4":
                                return 4;
                            case "5":
                                return 5;
                            case "6":
                                return 6;
                            case "7":
                                return 7;
                            case "8":
                                return 8;
                            case "9":
                                return 9;
                            case "A":
                                return 10;
                            case "B":
                                return 11;
                            case "C":
                                return 12;
                            case "D":
                                return 13;
                            case "E":
                                return 14;
                            case "F":
                                return 15;
                            default:
                                return 0;
                        }

                    }


                    // Connect to device
                    public bool Connect(string port)
                    {
                        bool flag = false;

                        // Check that the port is already open
                        if (_serialPort.IsOpen & isconnected) 
                        {
                           return true;
                        }

                        if (_serialPort.IsOpen)
                        {
                            _serialPort.Close();
                        }

                        string[] ports = SerialPort.GetPortNames();
                        foreach (string x in ports)
                        {
                            if (port.Equals(x))
                            {

                                try
                                {
                                    _serialPort = new SerialPort(port, 115200, Parity.None, 8);
                                    _serialPort.StopBits = StopBits.One;
                                    _serialPort.Handshake = Handshake.None;
                                    _serialPort.Open();

                                    SendCommMsg("PID");

                                    if (_serialPort.BytesToRead > 0)
                                    {
                                        string msg = ReadCommMsg();
                                        flag = true;
                                    }

                                    FlushBuffer();
                                    isconnected = flag;
                                    return flag;

                                }
                                catch (Exception)
                                {
                                    // handle the exception
                                    Console.WriteLine("Failed Open Serial: " + port + "/n");
                                    flag = false;
                                }
                            }

                        }
                        
                        return flag;
                    }



                    // Helper function for safe comm write 
                    public bool SendCommMsg(string msg)
                    {
                        
                        bool flag = false;
                        
                        // Check that the port is open
                        if (!_serialPort.IsOpen)
                        {
                            return flag;
                        }

                        msg = msg + (char)13 + (char)10;
                        byte[] bytes = Encoding.ASCII.GetBytes(msg);
                        

                        try
                        {
                            
                            _serialPort.Write(bytes, 0, bytes.Length);
                            Thread.Sleep(200);
                         
                            flag = true;
                        }
                        catch (Exception)
                        {
                            // handle the exception
                            Console.WriteLine("Failed " + msg + "/n");
                            flag = false;
                        }
                        
                        return flag;
                    }

                    // Safe serial port read
                    public string ReadCommMsg()
                    {
                        string msg = null;
                        
                        if (!_serialPort.IsOpen | !isconnected)
                        {
                            return msg;
                        }

                        try
                        {
                            if (_serialPort.BytesToRead > 0)
                            {
                                msg = _serialPort.ReadLine();
                            }
                            else
                            {
                                Console.WriteLine("Read Failed: No bytes avalaiable/n");
                            }
                        }
                        catch (Exception)
                        {
                            // handle the exception
                            Console.WriteLine("Failed Serial Read/n");
                        }
                        
                        return msg;
                    }

                }
            }
        }

    }
}
