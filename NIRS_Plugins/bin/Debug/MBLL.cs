using System;
using System.Xml;
using System.Collections.Generic;


namespace NIRS_Plugins
{
    [Serializable]
    public class MBLL : IPlugin
    {

        private parameters[] local_params;

        private List<double> Exthbr;
        private List<double> Exthbo;
        private List<double> lambda;


        public MBLL()
        {
            local_params = new parameters[1];
            local_params[0] = new parameters();

            local_params[0].datatype = typeof(double[]);
            local_params[0].name = "DPF";
            local_params[0].description = "Differential Pathlength factor";
            double[] dpf= new double[2];
            dpf[0] = 6;
            dpf[1] = 6;
            local_params[0].value = dpf;
            load_Ext();


        }


        public string Name
        {
            get
            {
                return "MBLL";
            }
        }

        public string Description
        {
            get
            {
                return "Modified Beer-Lambert Law";
            }
        }
        public string ShortDescription
        {
            get
            {
                return "Modified Beer-Lambert Law";
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }

      

        // This is the actual code that is called by the plugin
        public object Run(object data)
        {
            data = (nirs.core.Data)data;





            return data;
        }


        private double[][] GetSpectrum(double[] wavelength)
        {


            MathNet.Numerics.Interpolation.IInterpolation intHbR = MathNet.Numerics.Interpolate.Linear(lambda, Exthbr);
            MathNet.Numerics.Interpolation.IInterpolation intHbO = MathNet.Numerics.Interpolate.Linear(lambda, Exthbo);

            double[][] E = new double[wavelength.Length][];
            for (int i = 0; i < wavelength.Length; i++)
            {
                E[i] = new double[2];
                E[i][0] = intHbO.Interpolate(wavelength[i]);
                E[i][1] = intHbR.Interpolate(wavelength[i]);
            }


            return E;



        }


        private void load_Ext()
        {
            XmlDocument doc = new XmlDocument();
            XmlDocument doc2 = new XmlDocument();

            doc.Load(@"HemoglobinSpectrum.xml");
            XmlNodeList elemList;
            XmlNodeList elemListsub;

            elemList = doc.GetElementsByTagName("ext");

            List<double> lambda = new List<double>();
            List<double> Exthbo = new List<double>();
            List<double> Exthbr = new List<double>();

            for (int i = 0; i < elemList.Count; i++)
            {
                doc2 = new XmlDocument();
                doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
                elemListsub = doc2.GetElementsByTagName("lambda");
                lambda.Add(Convert.ToInt32(elemListsub[0].InnerXml));
                elemListsub = doc2.GetElementsByTagName("Ehbo");
                Exthbo.Add(Convert.ToDouble(elemListsub[0].InnerXml));
                elemListsub = doc2.GetElementsByTagName("Ehbr");
                Exthbr.Add(Convert.ToDouble(elemListsub[0].InnerXml));
            }
        }
    }

}