#define HDF5_VER1_10
using System;
using HDF.PInvoke;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

#if HDF5_VER1_10
using hid_t = System.Int64;
#else
    using hid_t = System.Int32;
#endif

using herr_t = System.Int32;
using hsize_t = System.UInt64;



namespace nirs
{
    public static partial class io
    {  // methods devoted to file I/O

        public static core.Data[] readSNIRF(string filename)
        {

            List<nirs.HDF5info> info = nirs.io.SNIRFinfo(filename);

            hid_t fileId = H5F.open(filename, H5F.ACC_RDONLY);
            string formatVersion = nirs.io.ReadDataString(fileId, "/formatVersion");
          

            List<string> dataLst = new List<string>();
            List<string> nirsLst = new List<string>();
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].field.Contains("/nirs") & info[i].field.Contains("/data"))
                {

                    int cnt = info[i].field.IndexOf("/");
                  
                    cnt = info[i].field.IndexOf("/", cnt + 1);
                    int cnt2 = cnt;
                    cnt = info[i].field.IndexOf("/", cnt + 1);  // find the third instance

                    string str = info[i].field.Substring(0, cnt);
                    string str2 = info[i].field.Substring(0, cnt2);

                    if (str.Contains("/nirs") & str.Contains("/data"))
                    {
                        if (!dataLst.Contains(str))
                        {
                            dataLst.Add(str);
                        }
                    }
                    if (str2.Contains("/nirs"))
                    {
                        if (!nirsLst.Contains(str2))
                        {
                            nirsLst.Add(str2);
                        }
                    }
                }
            }


            core.Probe[] probes = new core.Probe[nirsLst.Count];
            Dictionary[] demographics = new Dictionary[nirsLst.Count];
            core.Data[] data = new core.Data[dataLst.Count];
            List<Stimulus>[] stimuli = new List<Stimulus>[nirsLst.Count];
            auxillary[][] aux = new auxillary[nirsLst.Count][];

            for (int i = 0; i < nirsLst.Count; i++)
            {
                demographics[i] = new Dictionary();
                for (int j = 0; j < info.Count; j++)
                {
                    if (info[j].field.Contains(string.Format("{0}/metaDataTags", nirsLst[i])))
                    {
                        
                        string fld = info[j].field.Substring(string.Format("{0}/metaDataTags", nirsLst[i]).Length+1);

                        if (nirs.io.IsHDF5String(fileId, info[j].field))
                        {
                            string val = nirs.io.ReadDataString(fileId, info[j].field);
                            demographics[i].set(fld, val);
                        }
                        else
                        {
                            double val = nirs.io.ReadDataValue(fileId, info[j].field);
                            demographics[i].set(fld, val);
                        }

                        
                    }

                }

                //aux events
                List<string> auxList = new List<string>();
                for (int j = 0; j < info.Count; j++)
                {
                    if (info[j].field.Contains(string.Format("{0}/aux", nirsLst[i])) &
                        info[j].field.Contains("/name"))
                    {
                        string str = info[j].field.Substring(0, info[j].field.LastIndexOf("/"));
                        auxList.Add(str);
                    }
                }
                aux[i] = new auxillary[auxList.Count];
                for(int j=0; j<auxList.Count; j++)
                {
                    aux[i][j] = new auxillary();
                    aux[i][j].name = nirs.io.ReadDataString(fileId, string.Format("{0}/name", auxList[j]));
                    aux[i][j].data = nirs.io.ReadDataVector(fileId, string.Format("{0}/dataTimeSeries", auxList[j]));
                    aux[i][j].timeOffset = nirs.io.ReadDataValue(fileId, string.Format("{0}/timeOffset", auxList[j]));
                    aux[i][j].time = nirs.io.ReadDataVector(fileId, string.Format("{0}/time", auxList[j]));

                }


                // stim events
                stimuli[i] = new List<Stimulus>();
                for (int j = 0; j < info.Count; j++)
                {
                    if (info[j].field.Contains(string.Format("{0}/stim", nirsLst[i])) &
                        info[j].field.Contains("/name"))
                    {
                        string str = info[j].field.Substring(0, info[j].field.LastIndexOf("/"));
                        string stimname = nirs.io.ReadDataString(fileId, string.Format("{0}/name",str));

                        double[,] times = nirs.io.ReadDataArray(fileId, string.Format("{0}/data", str));
                        if(times.GetLength(0)==3 & times.GetLength(1) != 3)
                        {
                            times = nirs.io.ReadDataArray(fileId, string.Format("{0}/data", str), true);
                        }
                        Stimulus stim = new Stimulus();
                        stim.name = stimname;
                        stim.onsets = new List<double>();
                        stim.duration = new List<double>();
                        stim.amplitude = new List<double>();

                        for(int ii=0; ii < times.GetLength(0); ii++)
                        {
                            stim.onsets.Add(times[ii, 0]);
                            stim.duration.Add(times[ii, 1]);
                            stim.amplitude.Add(times[ii, 2]);
                        }
                        stimuli[i].Add(stim);


                    }
                }


                    // read the probe
                    probes[i] = new core.Probe();

                double[] wav;
                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/wavelengths", nirsLst[i])))
                {
                    wav = nirs.io.ReadDataVector(fileId, String.Format("{0}/probe/wavelengths", nirsLst[i]));
                } else {
                    wav = new double[0];
                 }


                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/sourcePos2D", nirsLst[i])))
                {
                    probes[i].SrcPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos2D", nirsLst[i]));
                    if (probes[i].SrcPos.GetLength(0)==3 & probes[i].SrcPos.GetLength(1)!=3)
                    {
                        probes[i].SrcPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos2D", nirsLst[i]), true);
                    }
                }
                else
                {
                    probes[i].SrcPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos", nirsLst[i]));
                    if (probes[i].SrcPos.GetLength(0) == 3 & probes[i].SrcPos.GetLength(1) != 3)
                    {
                        probes[i].SrcPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos", nirsLst[i]), true);
                    }
                }

                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/detectorPos2D", nirsLst[i])))
                {
                    probes[i].DetPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos2D", nirsLst[i]));
                    if (probes[i].DetPos.GetLength(0) == 3 & probes[i].SrcPos.GetLength(1) != 3)
                    {
                        probes[i].DetPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos2D", nirsLst[i]), true);
                    }
                }
                else
                {
                    probes[i].DetPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos", nirsLst[i]));
                    if (probes[i].DetPos.GetLength(0) == 3 & probes[i].DetPos.GetLength(1) != 3)
                    {
                        probes[i].DetPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos", nirsLst[i]), true);
                    }
                }

                
                if (nirs.io.groupexists(fileId,String.Format( "{0}/probe/landmarkPos2D", nirsLst[i]))){
                    probes[i].LandmarkPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos2D", nirsLst[i]));
                    if (probes[i].LandmarkPos.GetLength(0) == 3 & probes[i].LandmarkPos.GetLength(1) != 3)
                    {
                        probes[i].LandmarkPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos2D", nirsLst[i]),true);
                    }
                }
                else if(nirs.io.groupexists(fileId, String.Format("{0}/probe/landmarkPos", nirsLst[i]))){

                    probes[i].LandmarkPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos", nirsLst[i]));
                    if (probes[i].LandmarkPos.GetLength(0) == 3 & probes[i].LandmarkPos.GetLength(1) != 3)
                    {
                        probes[i].LandmarkPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos", nirsLst[i]),true);
                    }
                }
                

                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/sourcePos3D", nirsLst[i])))
                {
                    probes[i].SrcPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos3D", nirsLst[i]));
                    if(probes[i].SrcPos3D.GetLength(0)==3 & probes[i].SrcPos3D.GetLength(1) != 3)
                    {
                        probes[i].SrcPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos3D", nirsLst[i]), true);
                    }
                }
                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/detectorPos3D", nirsLst[i])))
                {
                    probes[i].DetPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos3D", nirsLst[i]));
                    if (probes[i].DetPos3D.GetLength(0) == 3 & probes[i].DetPos3D.GetLength(1) != 3)
                    {
                        probes[i].DetPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos3D", nirsLst[i]),true);
                    }
                }
                if (nirs.io.groupexists(fileId, String.Format("{0}/probe/landmarkPos3D", nirsLst[i])))
                {
                    if (nirs.io.groupexists(fileId, String.Format("{0}/probe/landmarkPos3D", nirsLst[i])))
                    {
                        probes[i].LandmarkPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos3D", nirsLst[i]));
                        if (probes[i].LandmarkPos3D.GetLength(0) == 3 & probes[i].LandmarkPos3D.GetLength(1) != 3)
                        {
                            probes[i].LandmarkPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/landmarkPos3D", nirsLst[i]), true);
                        }
                    }
                }
               

                probes[i].numDet = probes[i].DetPos.GetLength(0);
                probes[i].numSrc = probes[i].SrcPos.GetLength(0);
                probes[i].numWavelengths = wav.Length;


                probes[i].SourceLabels = new string[probes[i].SrcPos.GetLength(0)];
                for (int j = 0; j < probes[i].SrcPos.GetLength(0); j++)
                {
                    if (nirs.io.groupexists(fileId, String.Format("{0}/probe/sourceLabels{1}", nirsLst[i], j + 1)))
                    {
                        probes[i].SourceLabels[j] = nirs.io.ReadDataString(fileId, String.Format("{0}/probe/sourceLabels{1}", nirsLst[i], j + 1));
                    }
                    else
                    {
                        probes[i].SourceLabels[j] = string.Format("Source-{0}", j + 1);
                    }
                }
                probes[i].DetectorLabels = new string[probes[i].DetPos.GetLength(0)];
                for (int j = 0; j < probes[i].DetPos.GetLength(0); j++)
                {
                    if (nirs.io.groupexists(fileId, String.Format("{0}/probe/detectorLabels{1}", nirsLst[i], j + 1)))
                    {
                        probes[i].DetectorLabels[j] = nirs.io.ReadDataString(fileId, String.Format("{0}/probe/detectorLabels{1}", nirsLst[i], j + 1));
                    }
                    else
                    {
                        probes[i].DetectorLabels[j] = string.Format("Detector-{0}", j + 1);
                    }
                }

                if (probes[i].LandmarkPos != null)
                {
                    probes[i].LandmarkLabels = new string[probes[i].LandmarkPos.GetLength(0)];
                    for (int j = 0; j < probes[i].LandmarkPos.GetLength(0); j++)
                    {
                        if (nirs.io.groupexists(fileId, String.Format("{0}/probe/landmarkLabels{1}", nirsLst[i], j + 1)))
                        {
                            probes[i].LandmarkLabels[j] = nirs.io.ReadDataString(fileId, String.Format("{0}/probe/landmarkLabels{1}", nirsLst[i], j + 1));
                        }
                        else
                        {
                            probes[i].LandmarkLabels[j] = string.Format("Landmark-{0}", j + 1);
                        }
                    }
                }



            }

            // Now, load the data
            for (int i = 0; i < dataLst.Count; i++)
            {

                double[] wav = new double[0];

                data[i] = new core.Data();
                data[i].description = dataLst[i];


                for (int j = 0; j < nirsLst.Count; j++)
                {
                    if (dataLst[i].Contains(string.Format("{0}/data", nirsLst[j])))
                    {
                        data[i].probe = probes[j];
                        data[i].demographics = demographics[j];

                        if (nirs.io.groupexists(fileId, String.Format("{0}/probe/wavelengths", nirsLst[j])))
                        {
                            wav = nirs.io.ReadDataVector(fileId, String.Format("{0}/probe/wavelengths", nirsLst[j]));
                        }
                        else
                        {
                            wav = new double[0];
                        }

                        data[i].stimulus = stimuli[j];
                        data[i].auxillaries = aux[i];

                    }
                }

                double[] t = nirs.io.ReadDataVector(fileId, String.Format("{0}/time", dataLst[i]));
                data[i].time = new List<double>();
                for (int ii = 0; ii < t.Length; ii++)
                {
                    data[i].time.Add(t[ii]);
                }


                double[,] d = nirs.io.ReadDataArray(fileId, String.Format("{0}/dataTimeSeries", dataLst[i]));
                if (d.GetLength(0) == t.Length)
                {
                    d = nirs.io.ReadDataArray(fileId, String.Format("{0}/dataTimeSeries", dataLst[i]), true);
                }

                data[i].data = new List<double>[d.GetLength(0)];
                for (int ii = 0; ii < d.GetLength(0); ii++)
                {
                    data[i].data[ii] = new List<double>();
                    for (int jj = 0; jj < d.GetLength(1); jj++)
                    {
                        data[i].data[ii].Add(d[ii, jj]);
                    }
                }

                data[i].numsamples = data[i].time.Count;
                data[i].probe.numChannels = data[i].data.Length;
                data[i].probe.ChannelMap = new ChannelMap[data[i].probe.numChannels];

                for (int j = 0; j < data[i].probe.numChannels; j++)
                {
                    data[i].probe.ChannelMap[j] = new ChannelMap();
                    data[i].probe.ChannelMap[j].sourceindex = (int)nirs.io.ReadDataValue(fileId,
                                                                            String.Format("{0}/measurementList{1}/sourceIndex", dataLst[i], j + 1)) - 1;
                    data[i].probe.ChannelMap[j].detectorindex = (int)nirs.io.ReadDataValue(fileId,
                                                                String.Format("{0}/measurementList{1}/detectorIndex", dataLst[i], j + 1)) - 1;

                    if (wav.Length > 0)
                    {
                        data[i].probe.ChannelMap[j].wavelength = wav[(int)nirs.io.ReadDataValue(fileId,
                                                                    String.Format("{0}/measurementList{1}/wavelengthIndex", dataLst[i], j + 1)) - 1];
                    }
                    
                    int datatypeIdx = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementList{1}/dataType", dataLst[i], j + 1));


                    data[i].probe.ChannelMap[j].datatype = (datatype)Enum.ToObject(typeof(datatype), datatypeIdx);

                    if (wav.Length>0)
                    {
                        data[i].probe.ChannelMap[j].datasubtype = String.Format("{0}nm", data[i].probe.ChannelMap[j].wavelength);
                    }
                    else
                    {
                        data[i].probe.ChannelMap[j].datasubtype = nirs.io.ReadDataString(fileId,
                            String.Format("{0}/measurementList{1}/dataTypeLabel", dataLst[i], j + 1));
                    }

                    data[i].probe.ChannelMap[j].channelname = String.Format("Src{0}-Det{1}", data[i].probe.ChannelMap[j].sourceindex + 1,
                                                                            data[i].probe.ChannelMap[j].detectorindex + 1);

                    //{optional}
                    if (nirs.io.groupexists(fileId, String.Format("{0}/measurementList{1}/moduleIndex", dataLst[i], j + 1)))
                    {
                        data[i].probe.ChannelMap[j].moduleIndex = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementList{1}/moduleIndex", dataLst[i], j + 1));
                    }
                    if (nirs.io.groupexists(fileId, String.Format("{0}/measurementList{1}/detectorGain", dataLst[i], j + 1)))
                    {
                        data[i].probe.ChannelMap[j].DetectorGain = nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementList{1}/detectorGain", dataLst[i], j + 1));
                    }
                    if (nirs.io.groupexists(fileId, String.Format("{0}/measurementList{1}/sourcePower", dataLst[i], j + 1)))
                    {
                        data[i].probe.ChannelMap[j].SourcePower= nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementList{1}/sourcePower", dataLst[i], j + 1));
                    }
           


                }




            }


            return data;

            // data#/probe/useLocalIndex [int] = 0


        }


        private static bool groupexists(hid_t fileLoc, string name)
        {
            hid_t dId = 0;
            try
            {
                dId = H5D.open(fileLoc, name);
            }
            catch
            {
                dId = 0;
            }
            return (dId > 0);

        }


        private static bool IsHDF5String(hid_t fileLoc, string name)
        {
            hid_t dset = H5D.open(fileLoc, name);
            hid_t type = H5D.get_type(dset);

            H5T.class_t cl=H5T.get_class(type);


            return (cl== H5T.class_t.STRING) ;
        }

        private static string ReadDataString(hid_t fileLoc, string name)
        {

            hid_t dset = H5D.open(fileLoc, name);
             hid_t type = H5D.get_type(dset);
            // H5T.is_variable_str(type);
            IntPtr size= H5T.get_size(type);

            hid_t fspace = H5D.get_space(dset);
            hid_t mem_type = H5T.copy(type);
            H5T.set_size(mem_type, size);


            byte[] buffer = new byte[size.ToInt32()];
            GCHandle hnd = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
            H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();

            H5T.close(mem_type);

            // remove the last "/0"
            if (buffer[buffer.Length - 1] == 0)
            {
                byte[] buffer2 = new byte[buffer.Length - 1];
                for (int i = 0; i < buffer2.Length; i++)
                {
                    buffer2[i] = buffer[i];
                }


                return ASCIIEncoding.ASCII.GetString(buffer2);
            }
            return ASCIIEncoding.ASCII.GetString(buffer);
        }


        private static string ReadDataCharArray(hid_t fileLoc, string name)
        {

            hid_t dset = H5D.open(fileLoc, name);
            hid_t fspace = H5D.get_space(dset);
            hid_t count = H5S.get_simple_extent_ndims(fspace);

            hid_t type = H5D.get_type(dset);

            
                hsize_t[] dims = new hsize_t[count];
                hsize_t[] maxdims = new hsize_t[count];

                H5S.get_simple_extent_dims(fspace, dims, maxdims);
                H5S.close(fspace);


                byte[] rdata = new byte[dims[0]];

                hid_t mem_type = H5T.copy(type);
                H5T.set_size(mem_type, new IntPtr(1));

                GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
                H5D.read(dset, mem_type, H5S.ALL,
                H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

                hnd.Free();

                H5T.close(mem_type);

                char[] val = new char[dims[0]];
                for (int i = 0; i < (int)dims[0]; i += 2)
                {
                    val[i] = BitConverter.ToChar(rdata, i);

                }


                return val.ToString();
            
            
        }



        private static double ReadDataValue(hid_t fileLoc, string name)
        {
            hid_t dset = H5D.open(fileLoc, name);
            byte[] rdata = new byte[8];

            hid_t mem_type = H5T.copy(H5T.NATIVE_DOUBLE);
            H5T.set_size(mem_type, new IntPtr(8));

            GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
            H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();

            H5T.close(mem_type);

            double val = BitConverter.ToDouble(rdata, 0);
            return val;
        }


        private static double[,] ReadDataArray(hid_t fileLoc, string name,bool transpose=false)
        {

            hid_t dset = H5D.open(fileLoc, name);
            hid_t fspace = H5D.get_space(dset);
            hid_t count = H5S.get_simple_extent_ndims(fspace);

            hid_t type = H5D.get_type(dset);

            hsize_t[] dims = new hsize_t[count];
            hsize_t[] maxdims = new hsize_t[count];

            H5S.get_simple_extent_dims(fspace, dims, maxdims);
            H5S.close(fspace);


            byte[] rdata = new byte[dims[0] * dims[1] * 8];

            hid_t mem_type = H5T.copy(H5T.NATIVE_DOUBLE);
            H5T.set_size(mem_type, new IntPtr(8));

            GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
            H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();

            H5T.close(mem_type);

            if (transpose) {
                double[,] val = new double[dims[1], dims[0]];
                int cnt = 0;
                for (int i = 0; i < (int)dims[0]; i++)
                {
                    for (int j = 0; j < (int)dims[1]; j++)
                    {
                        val[j, i] = BitConverter.ToDouble(rdata, cnt * 8);
                        cnt++;
                    }
                }
                return val;
            }
            else
            {
                double[,] val = new double[dims[0], dims[1]];
                int cnt = 0;
                for (int i = 0; i < (int)dims[0]; i++)
                {
                    for (int j = 0; j < (int)dims[1]; j++)
                    {
                        val[i, j] = BitConverter.ToDouble(rdata, cnt * 8);
                        cnt++;
                    }
                }
                return val;
            }

            
        }

        private static double[] ReadDataVector(hid_t fileLoc, string name)
        {
            hid_t dset = H5D.open(fileLoc, name);
            hid_t fspace = H5D.get_space(dset);
            hid_t count = H5S.get_simple_extent_ndims(fspace);

            hid_t type = H5D.get_type(dset);

            hsize_t[] dims = new hsize_t[count];
            hsize_t[] maxdims = new hsize_t[count];

            H5S.get_simple_extent_dims(fspace, dims, maxdims);
            H5S.close(fspace);


            byte[] rdata = new byte[dims[0] * 8];

            hid_t mem_type = H5T.copy(H5T.NATIVE_DOUBLE);
            H5T.set_size(mem_type, new IntPtr(8));

            GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
            H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();
            H5T.close(mem_type);

            double[] val = new double[dims[0]];
            for (int i = 0; i < (int)dims[0]; i++)
            {
                val[i] = BitConverter.ToDouble(rdata, i * 8);
            }

            return val;
        }


    }
}



