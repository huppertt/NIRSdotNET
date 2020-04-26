using System;
using System.Xml;

namespace NIRSrecorder
{
    public struct System_info
    {
        public int numsrc;
        public int numdet;
        public int numaux;
        public bool laseradjustable;
        public int maxgain;
        public int maxpower;
        public int samplerate;
        public int[] allowedsamplerates;
    }

    public class Settings
    {

        // Since C# doesn't allow #DEFINE constants, I'll use the MainClass to hold these
        public int BUFFER_SIZE = 20 * 60 * 20;  // Buffer size for the data storage array (20min @20Hz)
        public bool DEBUG;
        public string SYSTEM;
        public int UPDATETIME = 500; // How often to update refreshes to the display window 
        public string DATADIR;
        public string PROBEDIR;

        public System_info system_Info;

        public Settings()
        {
             // Read the Config.xml file
            XmlDocument doc = new XmlDocument();
            XmlDocument doc2 = new XmlDocument();
          
            doc.Load(@"Config.xml");
            XmlNodeList elemList;
            XmlNodeList elemListsub;

            elemList = doc.GetElementsByTagName("debug");
            DEBUG = elemList[0].InnerXml.Trim().ToLower().Equals("true");
      
            // default_folders
            elemList = doc.GetElementsByTagName("default_folders");
            doc2.LoadXml("<root>" + elemList[0].InnerXml + "</root>");
            elemListsub = doc2.GetElementsByTagName("datadir");
            DATADIR = elemListsub[0].InnerXml;
            DATADIR = DATADIR.Trim();
            elemListsub = doc2.GetElementsByTagName("probedir");
            PROBEDIR = elemListsub[0].InnerXml;
            PROBEDIR = PROBEDIR.Trim();

            // System info
            elemList = doc.GetElementsByTagName("systemtype");
            SYSTEM = elemList[0].InnerXml;
            SYSTEM = SYSTEM.Trim();


            // Load the system specific config file
            if (SYSTEM.ToLower().Equals("BTnirs"))
            {
                doc.Load(@"BTNIRS_Config.xml");
            }
            else if (SYSTEM.ToLower().Equals("simulator"))
            {
                doc.Load(@"Simulator_Config.xml");
            }
            
            system_Info = new System_info();

            elemList = doc.GetElementsByTagName("numsrc");
            system_Info.numsrc = Convert.ToInt32(elemList[0].InnerXml);
            elemList = doc.GetElementsByTagName("numdet");
            system_Info.numdet = Convert.ToInt32(elemList[0].InnerXml);
            elemList = doc.GetElementsByTagName("numaux");
            system_Info.numaux = Convert.ToInt32(elemList[0].InnerXml);
            elemList = doc.GetElementsByTagName("laseradjustable");
            system_Info.laseradjustable = elemList[0].InnerXml.Trim().ToLower().Equals("true");
            elemList = doc.GetElementsByTagName("maxgain");
            system_Info.maxgain= Convert.ToInt32(elemList[0].InnerXml);
            elemList = doc.GetElementsByTagName("maxpower");
            system_Info.maxpower = Convert.ToInt32(elemList[0].InnerXml);
            elemList = doc.GetElementsByTagName("samplerate");
            system_Info.samplerate = Convert.ToInt32(elemList[0].InnerXml);

            elemList = doc.GetElementsByTagName("allowedsamplerates");
            system_Info.allowedsamplerates = new int[elemList.Count];
            for(int i=0;i<elemList.Count; i++)
            {
                system_Info.allowedsamplerates[i] = Convert.ToInt32(elemList[i].InnerXml);
            }

        }
    }
}
