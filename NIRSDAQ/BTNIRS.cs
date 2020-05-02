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
                    public bool isrunning;
                    public bool isconnected;

                    public bool[] laserstates;
                    public int[] laserpower;
                    public int[] detgains;

                    public bool usefilter;
                    public int sample_rate;

                    private SerialPort _serialPort;
                    private Queue[] dataqueue;
                    private Thread newthread;

                    private int wordsperrecord;

                    public int _nsrcs = 8;
                    public int _ndets = 6;

                    // num measurements
                    private int _nmeas = 24;


                    public void Initialize(nirs.core.Probe probe)
                    {
                       // TODO

                    }



                    public int getsamplerate()
                    {
                        return sample_rate;
                    }

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

                        SetSampleRate(sample_rate);

                        isrunning = true;
                        newthread = new Thread(adddata);
                       

                        // flush the Serial buffer
                        FlushBuffer();

                        isrunning = SendCommMsg("RUN");
                        newthread.Start();
                        return;
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


                    public string GetBatteryInfo()
                    {

                        SendCommMsg("BAT");
                        string msg =ReadCommMsg();

                        byte bat = Convert.ToByte(msg[0]);

                        bat = (byte)(bat >> 4);
                        return string.Format("{0}%", 10 * Convert.ToInt16(bat));
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



                    public void adddata()
                    {
                        // TODO
                        int wait;
                        wait = 500 / sample_rate;
                        int cnt = 0;

                        int npack = sample_rate / 10;



                        while (isrunning)
                        {
                            if (_serialPort.BytesToRead > wordsperrecord)
                            {
                                char[] data = new char[wordsperrecord];
                                int c = _serialPort.Read(data, 0, wordsperrecord);



                                char[] hdr = new char[4];
                                hdr[0] = data[0];
                                hdr[1] = data[1];
                                hdr[2] = data[2];
                                hdr[3] = data[3];

                                // byte1 =[6 8 10 12 14 16 18 20 22 24 26 28 30 32 34 36 38 40 42 44 46 48 50 52 54 56 58 60 62 64 66 68]';
                                //byte2 =[7 9 11 13 15 17 19 21 23 25 27 29 31 33 35 37 39 41 43 45 47 49 51 53 55 57 59 61 63 65 67 69]';
                                // detector =  [1 5 1 5 1 5 1 5 2 6 2 6 2 6 2 6 3 7 3 7 3 7 3 7 4 8 4 8 4 8 4 8]';
                                // source =[1 3 1 3 2 4 2 4 1 3 1 3 2 4 2 4 1 3 1 3 2 4 2 4 1 3 1 3 2 4 2 4]';
                                //type =      [1 1 2 2 1 1 2 2 1 1 2 2 1 1 2 2 1 1 2 2 1 1 2 2 1 1 2 2 1 1 2 2]';
                                //type(type == 1) = 735; type(type == 2) = 850;

                                int count = 4;
                                for (int pack = 0; pack < npack; pack++)
                                {
                                    for (int i = 0; i < _nmeas; i++)
                                    {
                                        double value = char2val(data[count]) + 256 * char2val(data[count + 1]);
                                        dataqueue[i].Enqueue(value);
                                        count += 2;

                                    }
                                }
                                byte bat = Convert.ToByte(data[cnt]);
                                bat = (byte)(bat >> 4);
                                cnt++;
                                //PERC = flipdim([100 95 90 85 80 75 70 65 60 55 50 40 30 20 10 0],2);

                                // aux.stim(i, 1) = 1 * strcmp(bat(5), '1');
                                // aux.stim(i, 2) = 1 * strcmp(bat(6), '1');
                                // aux.stim(i, 3) = 1 * strcmp(bat(7), '1');
                                // aux.stim(i, 4) = 1 * strcmp(bat(8), '1');
                                cnt++;
                                char temp = data[cnt];
                                char[] acc = new char[3];
                                acc[0] = data[cnt]; cnt++;
                                acc[1] = data[cnt]; cnt++;
                                acc[2] = data[cnt]; cnt++;
                            }
                            
                            Thread.Sleep(wait);
                        }
                      
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
