using System;

using csmatio.io;
using csmatio.types;
using System.Collections.Generic;
using System.IO;

namespace nirs
{
    public static partial class io
    {  // methods devoted to file I/O

/// <subjid>.wl1  - wavelength #1 data
/// <subjid>.wl2  - wavelength #2 data
/// <subjid>_config.txt   - config file
/// <subjid>.evt  - stimulus events(data taken from config file)
/// <subjid>_probeInfo.mat - probe file
/// <subjid>.tpl -topology file(data taken from config file)
        /// 
        public static core.Data readNIRx(string filename)
        {
            core.Data data = new core.Data();

            filename = filename.Substring(0, filename.IndexOf(".wl1", StringComparison.Ordinal));

            // Read the header file
            List<string> hdrFields = new List<string>();
            List<string> hdrValues = new List<string>();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename +".hdr");
            while ((line = file.ReadLine()) != null)
            {
                if(line.Contains("=")){
                    int found = line.IndexOf("=", StringComparison.Ordinal);
                    hdrFields.Add(line.Substring(0, found));
                    string value = line.Substring(found + 1);
                    if(value.Contains("#")){
                        value = "";
                        while ((line = file.ReadLine()) != null){
                            if(line.Contains("#")){
                                break;
                            }
                            value = value + "\r" + line;
                        }
                    }
                    if (value.Contains("\"")){
                        value = value.Substring(1, value.Length - 2);
                    }
                    hdrValues.Add(value);
                   
                }
            }
            file.Close();

            string targetDirectory = Path.GetDirectoryName(filename + ".hdr");
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            string probeFile = Path.Combine(targetDirectory,"Standard_probeInfo.mat");
            foreach (string i in fileEntries){
                if(i.Contains("probeInfo.mat")){
                    probeFile = Path.Combine(targetDirectory, i);
                    break;
                }
            }


            // Now, read the Probe_info.mat file
            MatFileReader mfr = new MatFileReader(probeFile);
            MLStructure probeInfo = (mfr.Content["probeInfo"] as MLStructure);
            MLStructure probes = (probeInfo["probes"] as MLStructure);

            MLDouble coords_s2 = (probes["coords_s2"] as MLDouble);
            MLDouble coords_d2 = (probes["coords_d2"] as MLDouble);
            MLDouble coords_c2 = (probes["coords_c2"] as MLDouble);

            double[][] srcpos = coords_s2.GetArray();
            double[][] detpos = coords_d2.GetArray();
            double[][] landpos = coords_c2.GetArray();

            // TODO read all the 3D stuff too
            MLDouble coords_s3 = (probes["coords_s3"] as MLDouble);
            MLDouble coords_d3 = (probes["coords_d3"] as MLDouble);
            MLDouble coords_c3 = (probes["coords_c3"] as MLDouble);

            double[][] srcpos3D = coords_s3.GetArray();
            double[][] detpos3D = coords_d3.GetArray();
            double[][] landpos3D = coords_c3.GetArray();


            data.probe.numSrc = srcpos.Length;
            data.probe.numDet = detpos.Length;


           data.probe.DetPos = new double[detpos.Length, 3];
            data.probe.DetectorLabels = new string[detpos.Length];
                for (int i = 0; i < detpos.Length; i++)
                {
                    data.probe.DetPos[i, 0] = (float)detpos[i][0];
                    data.probe.DetPos[i, 1] = (float)detpos[i][1];
                    data.probe.DetPos[i, 2] = 0;
                data.probe.DetectorLabels[i]=string.Format("Detector-{0}",i+1);
                }

            data.probe.SrcPos = new double[srcpos.Length, 3];
            data.probe.SourceLabels = new string[srcpos.Length];
                for (int i = 0; i < srcpos.Length; i++)
                {
                    data.probe.SrcPos[i, 0] = (float)srcpos[i][0];
                    data.probe.SrcPos[i, 1] = (float)srcpos[i][1];
                    data.probe.SrcPos[i, 2] = 0;
                data.probe.SourceLabels[i] = string.Format("Source-{0}", i + 1);
                }

            data.probe.LandmarkPos = new double[landpos.Length, 3];
            data.probe.LandmarkLabels = new string[landpos.Length];
            for (int i = 0; i < landpos.Length; i++)
            {
                data.probe.LandmarkPos[i, 0] = (float)landpos[i][0];
                data.probe.LandmarkPos[i, 1] = (float)landpos[i][1];
                data.probe.LandmarkPos[i, 2] = 0;
                data.probe.LandmarkLabels[i] = string.Format("Landmark(temp)-{0}", i + 1);
            }


            data.probe.DetPos3D = new double[detpos3D.Length, 3];
            for (int i = 0; i < detpos3D.Length; i++)
            {
                data.probe.DetPos3D[i, 0] = (double)detpos3D[i][0];
                data.probe.DetPos3D[i, 1] = (double)detpos3D[i][1];
                data.probe.DetPos3D[i, 2] = (double)detpos3D[i][1];

            }

            data.probe.SrcPos3D = new double[srcpos3D.Length, 3];
            for (int i = 0; i < srcpos3D.Length; i++)
            {
                data.probe.SrcPos3D[i, 0] = (double)srcpos3D[i][0];
                data.probe.SrcPos3D[i, 1] = (double)srcpos3D[i][1];
                data.probe.SrcPos3D[i, 2] = (double)srcpos3D[i][2];

            }
            data.probe.LandmarkPos3D = new double[landpos.Length, 3];
            for (int i = 0; i < landpos.Length; i++)
            {
                data.probe.LandmarkPos3D[i, 0] = (float)landpos3D[i][0];
                data.probe.LandmarkPos3D[i, 1] = (float)landpos3D[i][1];
                data.probe.LandmarkPos3D[i, 2] = (float)landpos3D[i][2];
            }
            data.probe.isregistered = true;



            int LambdaIdx = hdrFields.IndexOf("Wavelengths");
            string[] lam = hdrValues[LambdaIdx].Split('\t');
            double[] lambda = new double[lam.Length];
            for (int i = 0; i < lam.Length; i++){
                lambda[i] = Convert.ToDouble(lam[i]);
            }

            int SDmaskIdx = hdrFields.IndexOf("S-D-Mask");
            string[] mask = hdrValues[SDmaskIdx].Split('\r');
            bool[,] SDMask = new bool[data.probe.numSrc, data.probe.numDet];
            for (int i = 1; i < data.probe.numSrc + 1; i++){
                string[] mask2 = mask[i].Split('\t');
                for (int j = 0; j < data.probe.numDet; j++){
                    SDMask[i - 1, j] = false;
                    if (mask2[j].Contains("1")){
                        SDMask[i - 1, j] = true;
                    }
                }
            }

            int cnt = 0;
            for (int i = 0; i < SDMask.GetLength(0); i++){
                for (int j = 0; j < SDMask.GetLength(1); j++){
                    if(SDMask[i,j]){
                        cnt++;
                    }
                }
            }


            data.probe.ChannelMap = new ChannelMap[cnt*lambda.Length];
            cnt = 0;

            List<int> ChanIdx = new List<int>();
            int cnt2 = 0;

            for (int w = 0; w < lambda.Length; w++)
            {
                for (int i = 0; i < SDMask.GetLength(0); i++)
                {
                    for (int j = 0; j < SDMask.GetLength(1); j++)
                    {
                        if (SDMask[i, j])
                        {
                            data.probe.ChannelMap[cnt] = new ChannelMap();
                            data.probe.ChannelMap[cnt].sourceindex = i;
                            data.probe.ChannelMap[cnt].detectorindex = j;
                            data.probe.ChannelMap[cnt].channelname = String.Format("Src{0}-Det{1}",
                                                                                 data.probe.ChannelMap[cnt].sourceindex + 1,
                                                                                 data.probe.ChannelMap[cnt].detectorindex + 1);
                            data.probe.ChannelMap[cnt].wavelength = lambda[w];
                            data.probe.ChannelMap[cnt].datasubtype = String.Format("{0}nm", data.probe.ChannelMap[cnt].wavelength);
                            cnt++;
                            if (w == 0)
                            {
                                ChanIdx.Add(cnt2);
                            }
                        }
                        cnt2++;
                    }
                }
            }

            data.probe.numChannels = data.probe.ChannelMap.Length;
            data.probe.measlistAct = new bool[data.probe.numChannels];
            for (int i = 0; i < data.probe.numChannels; i++)
            {
                data.probe.measlistAct[i] = false;
            }
            data.probe.measlistAct[0] = true;



            // read the actual data


            System.IO.StreamReader file2 = new System.IO.StreamReader(filename + ".wl1");
            string lines = file2.ReadToEnd();
            string[] tpts = lines.Split('\r');
            data.data = new double[data.probe.numChannels,tpts.Length-1];
            for (int i = 0; i < tpts.Length-1; i++){
                if(tpts[i].Contains("\n")){
                    tpts[i] = tpts[i].Substring(1, tpts[i].Length-1);
                }
                string[] pts = tpts[i].Split(' ');
                for (int j = 0; j < ChanIdx.Count; j++)
                {
                    data.data[j,i] = Convert.ToDouble(pts[ChanIdx[j]]);
                }
            }
            file2 = new System.IO.StreamReader(filename + ".wl2");
            lines = file2.ReadToEnd();
            tpts = lines.Split('\r');
             for (int i = 0; i < tpts.Length-1; i++)
            {
                if (tpts[i].Contains("\n"))
                {
                    tpts[i] = tpts[i].Substring(1, tpts[i].Length-1);
                }
                string[] pts = tpts[i].Split(' ');
                for (int j = 0; j < ChanIdx.Count; j++)
                {
                    data.data[j+ data.probe.numChannels/2,i] = Convert.ToDouble(pts[ChanIdx[j]]);
                }
            }

            // finally, the time vector
            int fsIdx = hdrFields.IndexOf("SamplingRate");

            double fs = Convert.ToDouble(hdrValues[fsIdx]);

            data.numsamples = data.data.GetLength(1);
            data.time = new double[data.numsamples];
            for (int i = 0; i < data.numsamples; i++){
                data.time[i] = i/fs;
            }


            // TODO add stimulus information
            int EventIdx = hdrFields.IndexOf("Events");
            string[] eventline = hdrValues[EventIdx].Split('\r');
            double[,] events = new double[eventline.Length-1,3];

            for (int i = 1; i < eventline.Length; i++)
            {
                string[] eventline2 = eventline[i].Split('\t');
                for (int j = 0; j < 2; j++){
                    events[i-1, j] = Convert.ToDouble(eventline2[j]);
                }
            }
            List<double> uniqEvents = new List<double>();
            int[] uniqEventCount = new int[events.GetLength(0)];
            for (int i = 0; i < events.GetLength(0); i++){
                uniqEventCount[i] = 0;
                if (!uniqEvents.Contains(events[i,1])){
                    uniqEvents.Add(events[i, 1]);
                   
                }
                int ii = uniqEvents.IndexOf(events[i, 1]);
                uniqEventCount[ii]++;
            }

            data.stimulus = new Stimulus[uniqEvents.Count];
            for (int i = 0; i < uniqEvents.Count; i++){
                data.stimulus[i] = new Stimulus();
                data.stimulus[i].name = String.Format("Event-{0}", uniqEvents[i]);
                data.stimulus[i].onsets = new double[uniqEventCount[i]];
                data.stimulus[i].duration = new double[uniqEventCount[i]];
                data.stimulus[i].amplitude = new double[uniqEventCount[i],1];

                int n = 0;
                for (int j = 0; j < events.GetLength(0); j++){
                    if(Math.Abs(events[j,1]-uniqEvents[i])<0.0001){
                        data.stimulus[i].onsets[n] = events[j, 0];
                        data.stimulus[i].duration[n] = 1;
                        data.stimulus[i].amplitude[n,0] = 1;
                        n++;
                    }
                }


            }




            //add demographics info
            List<string> Fields = new List<string>(){
            "Subject","notes","FileName","Date","Device","Source",
                "Mod","APD","NIRStar","Mod Amp"};

            for (int i = 0; i < Fields.Count; i++){
                int idx = hdrFields.IndexOf(Fields[i]);
                if(idx>-1){
                    data.demographics.set(Fields[i], hdrValues[idx]);
                }
            }


            data.description = targetDirectory;


            return data;
        }



    }
}



