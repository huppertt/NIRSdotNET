using System;
using nirs;
using System.Xml;
using System.Collections.Generic;

namespace NIRS_Plugins
{
    public class OpticalDensity : IPlugin
    {

        private parameters[] local_params;




        publicOpticalDensity()
        {
            local_params = null;


        }


        public string Name
        {
            get
            {
                return "Optical Density";
            }
        }

        public string Description
        {
            get
            {
                return "Optical Density";
            }
        }
        public string Citation
        {
            get
            {
                return "Some paper citaition";
            }
        }

        public parameters[] Parameters
        {
            get
            {
                return local_params;
            }
            set
            {
                local_params = Parameters;
            }

        }
        public Type DataIn_type
        {
            get
            {
                return typeof(nirs.core.Data);
            }
        }
        public Type DataOut_type
        {
            get
            {
                return typeof(nirs.core.Data);
            }
        }


        // This is the actual code that is called by the plugin
        public object Run(object data)
        {
            nirs.core.Data[] dataout = new nirs.core.Data[data.length];

            for (int cnt = 0; cnt < data.length; cnt++) {

                dataout[cnt] = data[cnt].clone();

                double meand = mathnet.mean(dataout[cnt].data[i]);
                for (int i = 0; i < dataout[cnt].data.length(), i++)
                {
                    dataout[cnt].data[i] = Math.Log(dataout[cnt].data[i] / meand);
                }
            }

            return dataout;
        }


    }
}