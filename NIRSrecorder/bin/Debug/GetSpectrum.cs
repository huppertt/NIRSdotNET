using System;
using System.Xml;

namespace nirs
{

    public class Media
    {
        private int[] lambda;
        private double[] Exthbo;
        private double[] Exthbr;

        public Media()
        {
            return;

        }


        public void GetSpectrum(double wavelength, out double Ehbo, out double Ehbr)
        {

            if (lambda == null)
            {
                XmlDocument doc = new XmlDocument();
                XmlDocument doc2= new XmlDocument();

                doc.Load(@"HemoglobinSpectrum.xml");
                XmlNodeList elemList;
                XmlNodeList elemListsub;

                elemList = doc.GetElementsByTagName("ext");

                lambda = new int[elemList.Count];
                Exthbo = new double[elemList.Count];
                Exthbr = new double[elemList.Count];

                for (int i = 0; i < elemList.Count; i++)
                {
                    doc2 = new XmlDocument();
                    doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
                    elemListsub = doc2.GetElementsByTagName("lambda");
                    lambda[i] = Convert.ToInt32(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("Ehbo");
                    Exthbo[i] = Convert.ToDouble(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("Ehbr");
                    Exthbr[i] = Convert.ToDouble(elemListsub[0].InnerXml);
                }
            }

            Ehbo = -1;
            Ehbr = -1;

            for (int i = 0; i < lambda.Length; i++)
            {
                if (lambda[i] == (int)Math.Round(wavelength))
                {
                    Ehbo = Exthbo[i];
                    Ehbr = Exthbr[i];
                    break;
                }
            }



        }
    }
}