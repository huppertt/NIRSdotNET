using System;
using nirs;
using System.Xml;
using System.Collections.Generic;

namespace NIRS_Plugins
{

    public enum my_choices { 
        OLS,
        ARIRLS,
        NIRS_SPM
    }

    [Serializable]
    public class Test : IPlugin
    {

        private parameters[] local_params;

        private List<double> Exthbr;
        private List<double> Exthbo;
        private List<double> lambda;

        



        public Test()
        {
            local_params = new parameters[3];

            local_params[0] = new parameters();
            local_params[0].datatype = typeof(double);
            local_params[0].name = "DPF";
            local_params[0].value = 6;
            local_params[0].description = "Test";

            local_params[1] = new parameters();
            local_params[1].datatype = typeof(bool);
            local_params[1].name = "Bool value";
            local_params[1].value = true;
            local_params[1].description = "test2";

            local_params[2] = new parameters();
            local_params[2].datatype = typeof(my_choices);
            local_params[2].name = "Enum";
            local_params[2].value = my_choices.ARIRLS;
            local_params[2].description = "Example of an enum";

            load_Ext();


        }


        public string Name
        {
            get
            {
                return "Test";
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