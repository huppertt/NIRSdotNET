using System;

using csmatio.io;
using csmatio.types;
using System.Collections.Generic;

namespace nirs
{
    public static partial class io
    {  // methods devoted to file I/O


        public static core.Data readDOTnirs(string filename)
        {
            core.Data data = new core.Data();

            MatFileReader mfr = new MatFileReader(filename);

            MLDouble mlD = (mfr.Content["d"] as MLDouble);
            if (mlD != null)
            {
                double[][] d = mlD.GetArray();

                double[] dd = d[0];
                data.data = new List<double>[dd.Length];
                for (int i=0; i<dd.Length; i++)
                {
                    data.data[i] = new List<double>();
                }

                
                for (int i = 0; i < d.Length; i++)
                {
                   
                    double[] dd3 = d[i];
                    for (int j = 0; j < dd3.Length; j++)
                    {
                        data.data[j].Add(dd3[j]);
                    }
                }
            }
            MLDouble mlT = (mfr.Content["t"] as MLDouble);
            if (mlT != null)
            {
                double[][] t = mlT.GetArray();
                data.time = new List<double>();
                for (int i = 0; i < t.Length; i++)
                {
                    data.time.Add(t[i][0]);
                }
            }
            data.numsamples = data.time.Count;

            double[][] lambda;
            MLStructure SD = (mfr.Content["SD"] as MLStructure);

            MLDouble MLlambda = null;
            if (SD.Keys.Contains("Lambda"))
            {
                MLlambda = (SD["Lambda"] as MLDouble);
            }
            else if (SD.Keys.Contains("lambda"))
            {
                MLlambda = (SD["lambda"] as MLDouble);
            }

            MLDouble MLsrcpos = null;
            if (SD.Keys.Contains("SrcPos"))
            {
                MLsrcpos = (SD["SrcPos"] as MLDouble);
            }
            else if (SD.Keys.Contains("srcpos"))
            {
                MLsrcpos = (SD["srcpos"] as MLDouble);
            }

            MLDouble MLdetpos = null;
            if (SD.Keys.Contains("DetPos"))
            {
                MLdetpos = (SD["DetPos"] as MLDouble);
            }
            else if (SD.Keys.Contains("detpos"))
            {
                MLdetpos = (SD["detpos"] as MLDouble);
            }

            if (MLdetpos != null)
            {
                double[][] detpos = MLdetpos.GetArray();
                data.probe.DetPos = new double[detpos.Length, 3];
                data.probe.DetectorLabels = new string[detpos.Length];

                for (int i = 0; i < detpos.Length; i++)
                {
                    data.probe.DetPos[i, 0] = (float)detpos[i][0];
                    data.probe.DetPos[i, 1] = (float)detpos[i][1];
                    data.probe.DetPos[i, 2] = (float)detpos[i][2];

                    data.probe.DetectorLabels[i] = String.Format("Detector-{0}", +1);



                }
                data.probe.numDet = detpos.Length;
            }

            if (MLsrcpos != null)
            {
                double[][] srcpos = MLsrcpos.GetArray();
                data.probe.SrcPos = new double[srcpos.Length, 3];
                data.probe.SourceLabels = new string[srcpos.Length];
                for (int i = 0; i < srcpos.Length; i++)
                {
                    data.probe.SrcPos[i, 0] = (float)srcpos[i][0];
                    data.probe.SrcPos[i, 1] = (float)srcpos[i][1];
                    data.probe.SrcPos[i, 2] = (float)srcpos[i][2];
                    data.probe.SourceLabels[i] = String.Format("Source-{0}", i + 1);
                }
                data.probe.numSrc = srcpos.Length;
            }


            if (MLlambda != null)
            {
                lambda = MLlambda.GetArray();




                MLDouble mlMeasList = (mfr.Content["ml"] as MLDouble);
                if (mlMeasList != null)
                {
                    double[][] ml = mlMeasList.GetArray();
                    data.probe.ChannelMap = new ChannelMap[ml.Length];
                    for (int i = 0; i < ml.Length; i++)
                    {
                        data.probe.ChannelMap[i] = new ChannelMap();
                        data.probe.ChannelMap[i].sourceindex = (int)ml[i][0] - 1;
                        data.probe.ChannelMap[i].detectorindex = (int)ml[i][1] - 1;
                        data.probe.ChannelMap[i].channelname = String.Format("Src{0}-Det{1}",
                                                                             data.probe.ChannelMap[i].sourceindex + 1,
                                                                             data.probe.ChannelMap[i].detectorindex + 1);
                        data.probe.ChannelMap[i].wavelength = lambda[0][(int)ml[i][3] - 1];
                        data.probe.ChannelMap[i].datasubtype = String.Format("{0}nm", data.probe.ChannelMap[i].wavelength);
                    }
                }
            }

            data.probe.numChannels = data.probe.ChannelMap.Length;
            data.probe.measlistAct = new bool[data.probe.numChannels];
            for (int i = 0; i < data.probe.numChannels; i++){
                data.probe.measlistAct[i] = false;
            }
            data.probe.measlistAct[0] = true;

            // TODO add stimulus information
            // TODO add 3D probe information
            // TODO add demographics info
            // TODO add auxillary information
            // TODO exceptipon catches for case-sensitive variable names

            data.description = filename;


            return data;
        }

        public static void writeDOTnirs(core.Data data, string filename,int startIdx=0,int endIdx= Int32.MaxValue)
        {

            // Store the data into the *.nirs matlab format
            try
            {
                int numsamples = data.numsamples;
                int numch = data.probe.numChannels;
                int naux = data.auxillaries.Length;

                numsamples = Math.Min(endIdx - startIdx, numsamples - startIdx);


                // save the structure as mat file using MatFileWriter
                List<MLArray> mlList = new List<MLArray>();

                double[][] d = new double[numsamples][];
                double[][] t = new double[numsamples][];
                double[][] aux = new double[numsamples][];

                for (int j = startIdx; j < startIdx + numsamples; j++)
                {
                    double[] dloc = new double[numch];

                    for (int i = 0; i < numch; i++)
                    {
                        dloc[i] = data.data[i][j];
                    }

                    if (naux > 0)
                    {
                        double[] aloc = new double[naux];
                        for (int i = 0; i < naux; i++)
                        {
                            aloc[i] = data.auxillaries[i].data[j];
                        }
                        aux[j - startIdx] = aloc;
                    }
                    else
                    {
                        double[] aa = new double[1];
                        aa[0] = 0;
                        aux[j - startIdx] = aa;
                    }

                    double[] tt = new double[1];
                    double[] ss = new double[1];
                    ss[0] = 0;
                    tt[0] = data.time[j];
                    t[j - startIdx] = tt;
                    d[j - startIdx] = dloc;

                }

                MLDouble mldata = new MLDouble("d", d);
                mlList.Add(mldata);
                MLDouble mlaux = new MLDouble("aux", aux);
                mlList.Add(mlaux);
                MLDouble mltime = new MLDouble("t", t);
                mlList.Add(mltime);


                double[][] s = new double[numsamples][];

                for (int j = startIdx; j < startIdx + numsamples; j++)
                {
                    double[] dloc = new double[data.stimulus.Count];
                    double thistime = data.time[j];
                    for (int i = 0; i < data.stimulus.Count; i++)
                    {
                        dloc[i] = 0;
                        for (int k = 0; k < data.stimulus[i].onsets.Count; k++)
                        {
                            double onset = data.stimulus[i].onsets[k];
                            double dur = data.stimulus[i].duration[k];
                            if (thistime >= onset & thistime <= onset + dur)
                            {
                                dloc[i] = data.stimulus[i].amplitude[k];
                            }
                        }

                    }
                    s[j - startIdx] = dloc;
                }


                MLDouble mls = new MLDouble("s", s);
                mlList.Add(mls);

                MLCell condNames = new MLCell("CondNames", new int[] { 1, data.stimulus.Count });
                for (int i = 0; i < data.stimulus.Count; i++)
                {
                    condNames[0, i] = new MLChar(null, data.stimulus[i].name);
                }
                mlList.Add(condNames);



                // Probe 
                MLStructure mlSD = new MLStructure("SD", new int[] { 1, 1 });


                List<string> datasubtype = new List<string>();
                for (int i = 0; i < data.probe.numChannels; i++)
                {
                    if (!datasubtype.Contains(data.probe.ChannelMap[i].datasubtype))
                    {
                        datasubtype.Add(data.probe.ChannelMap[i].datasubtype);
                    }
                }
                double[] lambda = new double[datasubtype.Count];
                for (int i = 0; i < data.probe.numChannels; i++)
                {
                    lambda[datasubtype.IndexOf(data.probe.ChannelMap[i].datasubtype)] = data.probe.ChannelMap[i].wavelength;
                }

                double[][] srcpos = new double[data.probe.numSrc][];
                for (int j = 0; j < data.probe.numSrc; j++)
                {
                    double[] slo = new double[3];
                    for (int i = 0; i < 2; i++)
                    {
                        slo[i] = data.probe.SrcPos[j, i];
                    }
                    slo[2] = 0;
                    srcpos[j] = slo;
                }


                double[][] detpos = new double[data.probe.numDet][];
                for (int j = 0; j < data.probe.numDet; j++)
                {
                    double[] dlo = new double[3];
                    for (int i = 0; i < 2; i++)
                    {
                        dlo[i] = data.probe.DetPos[j, i];
                    }
                    dlo[2] = 0;
                    detpos[j] = dlo;
                }





                mlSD["NumDet", 0] = new MLDouble("", new double[] { data.probe.numDet }, 1);
                mlSD["NumSrc", 0] = new MLDouble("", new double[] { data.probe.numSrc }, 1);
                mlSD["Lambda", 0] = new MLDouble("", lambda, 1);
                mlSD["SrcPos", 0] = new MLDouble("", srcpos);
                mlSD["DetPos", 0] = new MLDouble("", detpos);

                if (data.probe.isregistered)
                {
                    double[][] srcpos3d = new double[data.probe.numSrc][];
                    for (int j = 0; j < data.probe.numSrc; j++)
                    {
                        double[] slo = new double[3];
                        for (int i = 0; i < 3; i++)
                        {
                            slo[i] = data.probe.SrcPos3D[j, i];
                        }
                        srcpos3d[j] = slo;
                    }


                    double[][] detpos3d = new double[data.probe.numDet][];
                    for (int j = 0; j < data.probe.numDet; j++)
                    {
                        double[] dlo = new double[3];
                        for (int i = 0; i < 3; i++)
                        {
                            dlo[i] = data.probe.DetPos3D[j, i];
                        }
                        detpos3d[j] = dlo;
                    }
                    mlSD["SrcPos3D", 0] = new MLDouble("", srcpos3d);
                    mlSD["DetPos3D", 0] = new MLDouble("", detpos3d);
                }

                // fixes for HOMER2
                mlSD["SpatialUnit"] = new MLChar("", "mm");




                // Add demographics as a struct
                MLStructure demo = new MLStructure("demographics", new int[] { 1, 1 });
                for (int i = 0; i < data.demographics.Keys.Count; i++)
                {
                    object val = data.demographics.get(data.demographics.Keys[i]);
                    string valstr = string.Format("{0}", val);

                    demo[data.demographics.Keys[i], 0] = new MLChar("", valstr);
                }
                mlList.Add(demo);

                double[][] ml = new double[data.probe.numChannels][];
                for (int i = 0; i < data.probe.numChannels; i++)
                {
                    double[] m = new double[4];
                    m[0] = data.probe.ChannelMap[i].sourceindex + 1;
                    m[1] = data.probe.ChannelMap[i].detectorindex + 1;
                    m[2] = 0;
                    m[3] = 1 + datasubtype.IndexOf(data.probe.ChannelMap[i].datasubtype);
                    ml[i] = m;
                }

                MLDouble mlml = new MLDouble("ml", ml);
                mlList.Add(mlml);


                mlSD["MeasList", 0] = new MLDouble("", ml);
                mlList.Add(mlSD);


                MLStructure mlStim = new MLStructure("StimDesign", new int[] { data.stimulus.Count, 1 });
                for (int i = 0; i < data.stimulus.Count; i++)
                {
                    mlStim["name", i] = new MLChar("", data.stimulus[i].name);


                    double[] onset = new double[data.stimulus[i].onsets.Count];
                    for (int ii = 0; ii < data.stimulus[i].onsets.Count; ii++) { onset[ii] = data.stimulus[i].onsets[ii]; }
                    double[] dur = new double[data.stimulus[i].duration.Count];
                    for (int ii = 0; ii < data.stimulus[i].duration.Count; ii++) { dur[ii] = data.stimulus[i].duration[ii]; }
                    double[] amp = new double[data.stimulus[i].amplitude.Count];
                    for (int ii = 0; ii < data.stimulus[i].amplitude.Count; ii++) { amp[ii] = data.stimulus[i].amplitude[ii]; }


                    mlStim["onset", i] = new MLDouble("", onset, 1);
                    mlStim["dur", i] = new MLDouble("", dur, 1);
                    mlStim["amp", i] = new MLDouble("", amp, 1);
                }
                if (data.stimulus.Count > 0)
                {
                    mlList.Add(mlStim);
                }

                new MatFileWriter(filename, mlList, false);
            }
            catch
            {
                Console.WriteLine("Unable to save .nirs file");
            }
            return;

        }


    }
}



