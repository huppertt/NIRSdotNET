using System;
using System.Xml;

using csmatio.io;
using csmatio.types;
using System.Collections.Generic;

namespace nirs
{
    public static partial class io
    {
        public static nirs.core.Probe LoadProbe(string filename)
        {

			core.Probe probe = loadxmlprobe(filename);



            return probe;
        }


		private static nirs.core.Probe loadxmlprobe(string file)
		{  // This function loads a probe from an xml file

			nirs.core.Probe probe = new core.Probe();

			XmlDocument doc = new XmlDocument();
			XmlDocument doc2;
			doc.Load(file);
			XmlNodeList elemList;
			XmlNodeList elemListsub;

			elemList = doc.GetElementsByTagName("wavelength");
			int[] lambda = new int[elemList.Count];
			for (int i = 0; i < elemList.Count; i++)
			{
				lambda[i] = Convert.ToInt32(elemList[i].InnerXml);
			}

				// Source-positions
				elemList = doc.GetElementsByTagName("srcpos");
			probe.numSrc = elemList.Count;

			probe.SrcPos = new double[probe.numSrc, 2];
			for (int i = 0; i < elemList.Count; i++)
			{
				doc2 = new XmlDocument();
				doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
				elemListsub = doc2.GetElementsByTagName("index");
				int index = Convert.ToInt32(elemListsub[0].InnerXml)-1;
				elemListsub = doc2.GetElementsByTagName("x");
				probe.SrcPos[index, 0] = Convert.ToDouble(elemListsub[0].InnerXml);
				elemListsub = doc2.GetElementsByTagName("y");
				probe.SrcPos[index, 1] = Convert.ToDouble(elemListsub[0].InnerXml);

			}

			// Detector-positions
			elemList = doc.GetElementsByTagName("detpos");
			probe.numDet= elemList.Count;
			probe.DetPos = new double[probe.numDet, 2];
			for (int i = 0; i < elemList.Count; i++)
			{
				doc2 = new XmlDocument();
				doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
				elemListsub = doc2.GetElementsByTagName("index");
				int index = Convert.ToInt32(elemListsub[0].InnerXml)-1;
				elemListsub = doc2.GetElementsByTagName("x");
				probe.DetPos[index, 0] = Convert.ToDouble(elemListsub[0].InnerXml);
				elemListsub = doc2.GetElementsByTagName("y");
				probe.DetPos[index, 1] = Convert.ToDouble(elemListsub[0].InnerXml);

			}

			// 3D corrdinates

			// Source-positions
			elemList = doc.GetElementsByTagName("srcpos3D");
			if (elemList.Count > 0)
			{
				probe.SrcPos3D = new double[probe.numSrc, 3];
				for (int i = 0; i < elemList.Count; i++)
				{
					doc2 = new XmlDocument();
					doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
					elemListsub = doc2.GetElementsByTagName("index");
					int index = Convert.ToInt32(elemListsub[0].InnerXml)-1;
					elemListsub = doc2.GetElementsByTagName("x");
					probe.SrcPos3D[index, 0] = Convert.ToDouble(elemListsub[0].InnerXml);
					elemListsub = doc2.GetElementsByTagName("y");
					probe.SrcPos3D[index, 1] = Convert.ToDouble(elemListsub[0].InnerXml);
					elemListsub = doc2.GetElementsByTagName("z");
					probe.SrcPos3D[index, 2] = Convert.ToDouble(elemListsub[0].InnerXml);
					probe.isregistered = true;

				}


				// Detector-positions
				elemList = doc.GetElementsByTagName("detpos3D");
				probe.DetPos3D = new double[probe.numDet, 3];
				for (int i = 0; i < elemList.Count; i++)
				{
					doc2 = new XmlDocument();
					doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
					elemListsub = doc2.GetElementsByTagName("index");
					int index = Convert.ToInt32(elemListsub[0].InnerXml)-1;
					elemListsub = doc2.GetElementsByTagName("x");
					probe.DetPos3D[index, 0] = Convert.ToDouble(elemListsub[0].InnerXml);
					elemListsub = doc2.GetElementsByTagName("y");
					probe.DetPos3D[index, 1] = Convert.ToDouble(elemListsub[0].InnerXml);
					elemListsub = doc2.GetElementsByTagName("z");
					probe.DetPos3D[index, 2] = Convert.ToDouble(elemListsub[0].InnerXml);

				}

			}
			else
			{
				probe.isregistered = false;
			}
            

			// Measurement list
			elemList = doc.GetElementsByTagName("ml");
			probe.ChannelMap = new ChannelMap[elemList.Count*lambda.Length];
			probe.numChannels = elemList.Count* lambda.Length;

			for (int j = 0; j < lambda.Length; j++)
			{
				for (int i = 0; i < elemList.Count; i++)
				{
					int ii = i + j * elemList.Count;
					probe.ChannelMap[ii] = new ChannelMap();
					doc2 = new XmlDocument();
					doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");

					elemListsub = doc2.GetElementsByTagName("src");
					probe.ChannelMap[ii].sourceindex = Convert.ToInt16(elemListsub[0].InnerXml)-1;

					elemListsub = doc2.GetElementsByTagName("det");
					probe.ChannelMap[ii].detectorindex = Convert.ToInt16(elemListsub[0].InnerXml)-1;

					probe.ChannelMap[ii].wavelength = lambda[j];

					probe.ChannelMap[ii].moduleIndex = 0;
					probe.ChannelMap[ii].SourcePower = 0;
					probe.ChannelMap[ii].DetectorGain = 0;
					probe.ChannelMap[ii].datatype = datatype.CW_absoption;
					probe.ChannelMap[ii].datasubtype = String.Format("raw {0}nm", probe.ChannelMap[ii].wavelength);
					probe.ChannelMap[ii].channelname = String.Format("Src{0}-Det{1} [{2}nm]", probe.ChannelMap[i].sourceindex+1,
						probe.ChannelMap[ii].detectorindex+1, probe.ChannelMap[ii].wavelength);
					probe.ChannelMap[ii].dataindex = 0;

				}
			}
			probe.measlistAct = new bool[probe.ChannelMap.Length];
            for(int i=0; i< probe.ChannelMap.Length; i++)
            {
				probe.measlistAct[i] = true;
			}

			return probe;

		}


	}
}
