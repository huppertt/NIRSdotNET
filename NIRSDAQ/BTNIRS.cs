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
                    private Queue[] auxqueue;
                    private Thread newthread;

                    private int wordsperrecord;

                    public int _nsrcs = 8;
                    public int _ndets = 2;
                    public int _naux = 0;

                    // num measurements
                    private int _nmeas = 32;
                    private string battery;

                    private List<int> MLorder;
                    public int[] wavelengths;

                    public void Initialize(nirs.core.Probe probe)
                    {
                        // Sets the mapping between data and the probe.

                        wavelengths = new int[] { 735, 850 };
                        int[] DetIdx = new int[] { 1, 5, 1, 5, 1, 5, 1, 5, 2, 6, 2, 6, 2, 6, 2, 6, 3, 7, 3, 7, 3, 7, 3, 7, 4, 8, 4, 8, 4, 8, 4, 8 };
                        int[] SrcIdx = new int[] { 1, 3, 1, 3, 2, 4, 2, 4, 1, 3, 1, 3, 2, 4, 2, 4, 1, 3, 1, 3, 2, 4, 2, 4, 1, 3, 1, 3, 2, 4, 2, 4 };
                        int[] TypIdx = new int[] { 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2 };

                        _nmeas = probe.numChannels;

                        MLorder = new List<int>();
                        for(int i=0; i<probe.numChannels; i++)
                        {
                            int found = -1;
                            for (int j=0; j < DetIdx.Length; j++)
                            {
                                if(probe.ChannelMap[i].sourceindex==SrcIdx[j]-1 &
                                   probe.ChannelMap[i].detectorindex==DetIdx[j]-1 &
                                   probe.ChannelMap[i].wavelength == wavelengths[TypIdx[j] - 1])
                                {
                                    found = j;
                                    break;
                                }
                            }
                            if (found > -1)
                            {
                                MLorder.Add(found);
                            }

                        }




                    }

                    public void IDmode(bool flag)
                    {
                        if (flag)
                        {
                            SendCommMsg("ION");
                        }
                        else
                        {
                            SendCommMsg("IOF");
                        }

                    }


                    public int getsamplerate()
                    {
                        return sample_rate;
                    }

                    public string portname()
                    {
                        return _serialPort.PortName;
                    }

                    public int naux()
                    {
                        return _naux;
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
                        try
                        {
                            Stop();
                            AllOff();
                            _serialPort.Close();
                            _serialPort.Dispose();
                        }
                        catch { }
                    }

                    public BTnirs()
                    {
                        _serialPort = new SerialPort();
                        isconnected = false;
                        isrunning = false;
                        laserstates = new bool[_nsrcs];
                        laserpower = new int[_nsrcs];

                     
                        dataqueue = new Queue[_nmeas];
                        for (int i = 0; i < _nmeas; i++)
                        {
                            dataqueue[i] = new Queue();
                        }

                        auxqueue = new Queue[_naux];
                        for(int i=0; i < _naux; i++)
                        {
                            auxqueue[i] = new Queue();
                        }

                    }

                    // List all valid COM ports
                    // TODO- handle Windows OS names
                    public List<string> ListPorts()
                    {
                        /*
    System.Management.ManagementObjectSearcher Searcher = new System.Management.ManagementObjectSearcher("Select * from WIN32_SerialPort");
    foreach (System.Management.ManagementObject Port in Searcher.Get())
    {
        foreach (System.Management.PropertyData Property in Port.Properties)
        {
            Console.WriteLine(Property.Name + " " + (Property.Value == null ? null : Property.Value.ToString()));
        }
    }
    */

                        string[] ports = SerialPort.GetPortNames();
                        SerialPort[] _serialPort = new SerialPort[ports.Length];
                        List<string> foundports = new List<string>();
                        for (int i = 0; i < ports.Length; i++)
                        {

                            if (ports[i].Contains("SerialPort") | ports[i].Contains("COM"))
                            {
                                try
                                {
                                    _serialPort[i] = new SerialPort(ports[i], 115200, Parity.None, 8);
                                    _serialPort[i].StopBits = StopBits.One;
                                    _serialPort[i].Handshake = Handshake.None;
                                    _serialPort[i].ReadTimeout = 10;
                                    _serialPort[i].WriteTimeout = 10;
                                    _serialPort[i].NewLine = string.Format("{0}", (char)13);
                                    
                                    _serialPort[i].Open();
                                    //_serialPort[i].Close();

                                    string msg = "PID";
                                    msg = msg + (char)13 + (char)10;
                                    byte[] bytes = Encoding.ASCII.GetBytes(msg);
                                    _serialPort[i].Write(bytes, 0, bytes.Length);
                                    Thread.Sleep(250);
                                    if (_serialPort[i].BytesToRead > 0)
                                    {
                                        foundports.Add(ports[i]);
                                    }

                                    
                                }
                                catch
                                {
                                    // do nothing
                                }
                            }
                        }
                        for (int i = 0; i < _serialPort.Length; i++)
                        {
                            try
                            {
                                if (_serialPort[i] != null)
                                {
                                    if (_serialPort[i].IsOpen)
                                    {
                                        _serialPort[i].Close();
                                    }
                                }
                            }
                            catch { }
                        }

                        return foundports;
                    }

                    public void Start()
                    {

                        if (isconnected)
                        {
                            SetSampleRate(sample_rate);

                            isrunning = true;
                            newthread = new Thread(adddata);


                            // flush the Serial buffer
                            FlushBuffer();
                            FlushBuffer();
                            isrunning = SendCommMsg("RUN");
                            int i = _serialPort.BytesToRead;
                            Thread.Sleep(100);
                            newthread.Start();
                        }
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
                        SendCommMsg(String.Format("SSR {0}", fs/10));

                        wordsperrecord = 5 + 64 * fs / 10 + 11;

                        SendCommMsg("GSR");
                        string msg = ReadCommMsg();

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
                        if (isconnected)
                        {
                            isrunning = false;
                            SendCommMsg("STP");
                            newthread.Abort();
                        }
                    }


                    public void SetLaserPower(int sIdx, int pwr)
                    {
                        if (pwr < 1) { pwr = 1; }
                        if (pwr > 127) { pwr = 127; }

                        laserpower[sIdx] = pwr;
                        SendCommMsg(string.Format("SLE {0} {1}", sIdx+1, pwr));
                    }


                    public string GetBatteryInfo()
                    {
                        if (isconnected)
                        {
                            if (!isrunning)
                            {

                                SendCommMsg("BAT");
                                string msg = ReadCommMsg();

                                if (msg == null)
                                {
                                    return battery;
                                }
                                byte bat = Convert.ToByte(msg[6]);
                                byte bat2 = Convert.ToByte(msg[7]);
                                battery = BatteryString(bat);
                            }
                        }
                        return battery;
                    }

                    private string BatteryString(byte msg)
                    {
                        int stim = (int)(msg & 0b11110000 );
                       int bat = (int)(msg & 0b00001111 );

                        string b = "";
                        if (bat == 0b1010) { b = "100%"; }
                        if (bat == 0b1001) { b = "90%"; }
                        if (bat == 0b1000) { b = "80%"; }
                        if (bat == 0b0111) { b = "70%"; }
                        if (bat == 0b0110) { b = "60%"; }
                        if (bat == 0b0101) { b = "50%"; }
                        if (bat == 0b0100) { b = "40%"; }
                        if (bat == 0b0011) { b = "30%"; }
                        if (bat == 0b0010) { b = "20%"; }
                        if (bat == 0b0001) { b = "10%"; }
                        if (bat == 0b0000) { b = "CHARGE BATTERY"; }

                        return b;
                    }

                    public void SetLaserState(int sIdx, bool state)
                    {
                        laserstates[sIdx] = state;
                        if (state)
                        {
                            SendCommMsg(String.Format("SSO {0} 1", sIdx+1));
                        }
                        else
                        {
                            SendCommMsg(String.Format("SSO {0} 0", sIdx+1));
                        }


                    }
                    public void SetDetGain(int dIdx, int gain)
                    {
                        if (gain < 1) { gain = 1; }
                        if (gain > 127) { gain = 127; }
                        detgains[dIdx] = gain;

                        SendCommMsg(String.Format("SDG {0} {1}", dIdx+1, gain));

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
                                _serialPort.DiscardOutBuffer();

                                Thread.Sleep(50);
                                if (_serialPort.BytesToRead > 0)
                                {
                                    _ = _serialPort.ReadExisting();
                                }
                            }
                       

                    }


                    // Get Data from the instrument and place in data queue
                    public double[] Getdata()
                    {

                        int cnt = 9999;
                        for (int i=0; i<dataqueue.Length; i++)
                        {
                            if(cnt> dataqueue[i].Count) { cnt = dataqueue[i].Count; }
                        }
                       

                        double[] thisdata = new double[_nmeas];
                        for (int i = 0; i < _nmeas; i++)
                        {
                            if (cnt > 0)
                            {
                                thisdata[i] = (double)dataqueue[MLorder[i]].Dequeue();
                            }
                        }
                        return thisdata;
                    }


                    // Get Data from the instrument and place in data queue
                    public double[] GetdataAux()
                    {

                        int cnt = 9999;
                        for (int i = 0; i < auxqueue.Length; i++)
                        {
                            if (cnt > auxqueue[i].Count) { cnt = auxqueue[i].Count; }
                        }


                        double[] thisdata = new double[_naux];
                        for (int i = 0; i < _naux; i++)
                        {
                            if (cnt > 0)
                            {
                                thisdata[i] = (double)auxqueue[i].Dequeue();
                            }
                        }
                        return thisdata;
                    }


                    // Return the number of samples in the data queue
                    public int SamplesAvaliableAux()
                    {
                        double cnt = 0;
                        for (int i = 0; i < _naux; i++)
                        {
                            cnt = cnt + auxqueue[i].Count;
                        }
                        cnt = Math.Floor(cnt / _nmeas);
                        return (int)cnt;
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
                        try
                        {
                            Stop();
                            AllOff();
                            _serialPort.Close();
                            _serialPort.Dispose();
                        }
                        catch { }
                    }



                    public void adddata()
                    {
                        if (isconnected)
                        {
                            // TODO
                            int wait;
                            wait = 500 / sample_rate;

                            while (isrunning)
                            {
                                if (_serialPort.BytesToRead > wordsperrecord)
                                {
                                    uint startPack1 = new uint();
                                    uint startPack2 = 0;
                                    while (startPack1 != 160 | startPack2 != 162)
                                    {
                                        startPack1 = startPack2;  // should be 160 = 0xA0
                                        startPack2 = (uint)_serialPort.ReadByte(); // should be 162 = 0xA2
                                    }

                                    uint seqnum = (uint)_serialPort.ReadByte();
                                    uint lenPack = 256 * (uint)_serialPort.ReadByte() + (uint)_serialPort.ReadByte();
                                    uint nsamp = (lenPack - 16) / 64;
                                    //int nsamp = sample_rate / 10;

                                    byte[] data = new byte[64 * nsamp + 11];
                                    _ = _serialPort.Read(data, 0, data.Length);

                                    int count = 0;
                                    for (int pack = 0; pack < nsamp; pack++)
                                    {
                                        for (int i = 0; i < _nmeas; i++)
                                        {
                                            double value = data[count] * 256 + data[count + 1];
                                            dataqueue[i].Enqueue(value);
                                            count += 2;

                                        }
                                    }
                                    //uint bat = (uint)data[64 * nsamp];
                                    battery = BatteryString(data[64 * nsamp]);
                                    int temp = (int)data[64 * nsamp + 1];
                                    uint reserve1 = (uint)data[64 * nsamp + 2];
                                    uint reserve2 = (uint)data[64 * nsamp + 3];

                                    uint ACCX = (uint)data[64 * nsamp + 4];
                                    uint ACCY = (uint)data[64 * nsamp + 5];
                                    uint ACCZ = (uint)data[64 * nsamp + 6];

                                    uint CRC1 = (uint)data[64 * nsamp + 7];
                                    uint CRC2 = (uint)data[64 * nsamp + 8];

                                    int endPack1 = data[64 * nsamp + 9]; // should be 176 = 0xB0
                                    int endPack2 = data[64 * nsamp + 10]; // should be 179 = 0xB3

                                    auxqueue[0].Enqueue(reserve1);
                                    auxqueue[1].Enqueue(reserve2);
                                    auxqueue[2].Enqueue(ACCX);
                                    auxqueue[3].Enqueue(ACCY);
                                    auxqueue[4].Enqueue(ACCZ);
                                    auxqueue[5].Enqueue(CRC1);
                                    auxqueue[6].Enqueue(CRC2);

                                }

                                Thread.Sleep(wait);
                            }
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
                                    _serialPort.ReadBufferSize = 1024 * 1024;
                                    _serialPort.ReadTimeout = 50;
                                //    _serialPort.WriteTimeout = 50;
                                    _serialPort.NewLine = string.Format("{0}", (char)13);
                                    _serialPort.Open();
                                    isconnected = true;

                                    SendCommMsg("STP");
                                    AllOff();
                                   
                                    SendCommMsg("PID");
                                    Thread.Sleep(250);
                                   if (_serialPort.BytesToRead > 0)
                                    {
                                        string msg = ReadCommMsg();
                                        flag = true;
                                    }

                                    FlushBuffer();
                                    isconnected = flag;


                                    SetFilter(false);
                                    SetSampleRate(10);

                                    for (int i = 0; i < _nsrcs; i++)
                                    {
                                        SetLaserState(i, false);
                                        SetLaserPower(i, 100);
                                    }
                                    detgains = new int[_ndets];
                                    for (int i = 0; i < _ndets; i++)
                                    {
                                        SetDetGain(i, 1);
                                    }


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
                        if (isconnected)
                        {
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
                                Thread.Sleep(10);

                                flag = true;
                            }
                            catch (Exception)
                            {
                                // handle the exception
                                Console.WriteLine("Failed " + msg + "/n");
                                flag = false;
                            }
                        }
                        return flag;
                    }

                    // Safe serial port read
                    public string ReadCommMsg()
                    {
                        string msg = null;
                        if (isconnected)
                        {
                            if (!_serialPort.IsOpen)
                            {
                                return msg;
                            }

                            Thread.Sleep(200);
                            try
                            {


                                if (_serialPort.BytesToRead > 0)
                                {
                                    msg = _serialPort.ReadExisting();

                                }
                                else
                                {
                                   // Console.WriteLine("Read Failed: No bytes avalaiable/n");

                                }
                            }
                            catch (Exception)
                            {
                                // handle the exception
                                Console.WriteLine("Failed Serial Read/n");
                            }

                            
                        }
                        return msg;
                    }

                }
            }
        }

    }
}
