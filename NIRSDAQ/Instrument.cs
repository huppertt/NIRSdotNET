using System;


namespace NIRSDAQ
{

    public struct info
    {
        public string PortName;
        public string DeviceName;
        public int numDet;
        public int numSrc;
        public int numMeas;
        public int numwavelengths;
        public int[] wavelengths;
        public int sample_rate;
    }


    public partial class Instrument
    {

        public class instrument
        {
            private int devicetype;
            public string devicename;
            private object device;

            public instrument(string type)
            {
                if (type.ToLower().Equals("simulator"))
                {
                    devicename = "Simulator";
                    devicetype = 0;
                    device = new NIRSDAQ.Instrument.Devices.Simulator();
                }
                else if(type.ToLower().Equals("btnirs"))
                {
                    devicename = "BTNIRS";
                    devicetype = 1;
                    device = new NIRSDAQ.Instrument.Devices.TechEn.BTnirs();
                }
            }

            

            // deconstructor
            ~instrument()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).Destroy();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Destroy();
                        break;
                }

            }

            public info GetInfo()
            {
                info _info = new info();
                _info.DeviceName = devicename;
                switch (devicetype)
                {
                    case 0:
                        _info.numDet = ((NIRSDAQ.Instrument.Devices.Simulator)device).ndets();
                        _info.numSrc = ((NIRSDAQ.Instrument.Devices.Simulator)device).nsrcs();
                        _info.numMeas = ((NIRSDAQ.Instrument.Devices.Simulator)device).nmeas();
                        _info.PortName = ((NIRSDAQ.Instrument.Devices.Simulator)device).portname();
                        _info.numwavelengths = 2;
                        _info.wavelengths = new int[2];
                        _info.wavelengths[0] = 690;
                        _info.wavelengths[1] = 830;
                        _info.sample_rate = ((NIRSDAQ.Instrument.Devices.Simulator)device).sample_rate;
                        break;
                    case 1:
                        _info.numDet = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).ndets();
                        _info.numSrc = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).nsrcs();
                        _info.numMeas = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).nmeas();
                        _info.PortName = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).portname();
                        _info.numwavelengths = 2;
                        _info.wavelengths = new int[2];
                        _info.wavelengths[0] = 690;
                        _info.wavelengths[1] = 830;
                        _info.sample_rate = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).sample_rate;
                        break;
                }

                return _info;

            }

            public void Connect(string port)
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).Connect(port);
                        devicename += " " + port;
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Connect(port);
                        devicename += " " + port;
                        break;
                }
                AllOff();

            }

            public void Start()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).Start();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Start();
                        break;
                }

            }

            public void Stop()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).Stop();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Stop();
                        break;
                }

            }

            public string GetBatteryInfo()
            {
                string battery = "--------";
                switch (devicetype)
                {
                    case 0:
                        battery = ((NIRSDAQ.Instrument.Devices.Simulator)device).GetBatteryInfo();
                        break;
                    case 1:
                        battery=((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).GetBatteryInfo();
                        break;
                }
                return battery;
            }



            public void AllOn()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).AllOn();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).AllOn();
                        break;
                }

            }

            public void AllOff()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).AllOff();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).AllOff();
                        break;
                }

            }

            public void FlushBuffer()
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).FlushBuffer();
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).FlushBuffer();
                        break;
                }

            }

            public int SamplesAvaliable()
            {
                int flag = -1;
                switch (devicetype)
                {
                    case 0:
                        flag= ((NIRSDAQ.Instrument.Devices.Simulator)device).SamplesAvaliable();
                        break;
                    case 1:
                        flag= ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).SamplesAvaliable();
                        break;
                }
                return flag;

            }

            public void SetLaserPower(int sIdx, int pwr)
            {
                
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).SetLaserPower(sIdx, pwr);
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).SetLaserPower(sIdx, pwr);
                        break;
                }

            }

            public void SetLaserState(int sIdx, bool state)
            {
                
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).SetLaserState(sIdx, state);
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).SetLaserState(sIdx, state);
                        break;
                }

            }

            public void SetDetGain(int dIdx, int gain)
            {

                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).SetDetGain(dIdx, gain);
                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).SetDetGain(dIdx, gain);
                        break;
                }

            }

            public void Intialize(nirs.core.Probe probe)
            {
                switch (devicetype)
                {
                    case 0:
                        ((NIRSDAQ.Instrument.Devices.Simulator)device).Initialize(probe);

                        break;
                    case 1:
                        ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Initialize(probe);
                        break;

                }
            }

            public bool isrunning()
            {
                bool flag = false;
                switch (devicetype)
                {
                    case 0:
                        flag=((NIRSDAQ.Instrument.Devices.Simulator)device).isrunning;

                        break;
                    case 1:
                        flag=((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).isrunning;
                        break;

                }
                return flag;
            }



            public nirs.core.Data GetNewData(nirs.core.Data data)
            {
                int nsamples = 0;
                switch (devicetype)
                {
                    case 0:
                        nsamples = ((NIRSDAQ.Instrument.Devices.Simulator)device).SamplesAvaliable();
                        for (int i = 0; i < nsamples; i++)
                        {
                            double[] d =((NIRSDAQ.Instrument.Devices.Simulator)device).Getdata();
                            for(int j=0; j<d.Length; j++)
                            {
                                data.data[j].Add(d[j]);
                            }
                            double fs = ((NIRSDAQ.Instrument.Devices.Simulator)device).getsamplerate();
                            data.time.Add(data.time.Count / fs);
                            data.numsamples = data.time.Count;
                        }
                        break;
                    case 1:
                        nsamples = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).SamplesAvaliable();
                        for (int i = 0; i < nsamples; i++)
                        {
                            double[] d = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).Getdata();
                            for (int j = 0; j < d.Length; j++)
                            {
                                data.data[j].Add(d[j]);
                            }
                            double fs = ((NIRSDAQ.Instrument.Devices.TechEn.BTnirs)device).getsamplerate();
                            data.time.Add(data.time.Count / fs);
                            data.numsamples = data.time.Count;
                        }

                        
                        break;
                }


                return data;
            }

        }
    }
}

