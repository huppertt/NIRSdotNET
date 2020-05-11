#define HDF5_VER1_10
using System;
using HDF.PInvoke;


using System.Runtime.InteropServices;
using System.Collections.Generic;

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

        public static void writeSNIRF(core.Data[] data, string filename, int nirs_index = -1)
        {

            for(int i=0; i<data.Length; i++)
            {
                writeSNIRF(data[i],filename, nirs_index, i);
            }

        }


        public static void writeSNIRF(List<core.Data> data, string filename, int nirs_index = -1)
        {

            for (int i = 0; i < data.Count; i++)
            {
                writeSNIRF(data[i], filename, nirs_index, i);
            }

        }

        public static void writeSNIRF(core.Data data, string filename,int nirs_index=-1, int data_index=0)
        {
            hid_t tmp;
            hid_t fileId = H5F.create(filename, H5F.ACC_TRUNC);
            hid_t IDnirs;

            if (nirs_index == -1)
            {
               IDnirs = H5G.create(fileId, "/nirs");
            }else
            {
               IDnirs = H5G.create(fileId, String.Format("/nirs{0}",nirs_index));
            }
           
            // formatVersion-  [string] = "1.0"
            tmp = nirs.io.AddDataString(IDnirs,"formatVersion","1.0");

            tmp = nirs.io.AddDataValue(IDnirs, "dataCount", data_index+1);
            hid_t IDdata = H5G.create(IDnirs, String.Format("data{0}",data_index));

            double[,] d = new double[data.data.Length, data.data[0].Count];
            for(int i=0;i< data.data.Length; i++)
            {
                for(int j=0; j< data.data[i].Count; j++)
                {
                    d[i, j] = data.data[i][j];
                }
            }
            tmp = nirs.io.AddDataArray(IDdata, "dataTimeSeries", d);
            // data_#/time [numeric]  time x 1

            double[] time = new double[data.time.Count];
            for(int i=0; i<data.time.Count; i++)
            {
                time[i] = data.time[i];
            }
            tmp = nirs.io.AddDataVector(IDdata, "time", time);


            List<double> lambda = new List<double>();
            for (int i = 0; i < data.probe.numChannels; i++){
                if(!lambda.Contains(data.probe.ChannelMap[i].wavelength)){
                    lambda.Add(data.probe.ChannelMap[i].wavelength);
                }
            }



            hid_t[] IDmeas = new hid_t[data.probe.numChannels];
            tmp = nirs.io.AddDataValue(IDnirs, "measurementListCount", data.probe.numChannels);
            for (int ch = 0; ch < data.probe.numChannels; ch++)
            {
                IDmeas[ch] = H5G.create(IDnirs, String.Format("measurementList{0}",ch));
                //data_#/measurementList_#/sourceIndex [int; index from 1]
                tmp = nirs.io.AddDataValue(IDmeas[ch],"sourceIndex",data.probe.ChannelMap[ch].sourceindex + 1);
                //data_#/measurementList_#detectorIndex [int; index from 1]
                tmp = nirs.io.AddDataValue(IDmeas[ch], "detectorIndex", data.probe.ChannelMap[ch].detectorindex + 1);
                //data_#/measurementList_#/wavelengthIndex [int; index from 1]
                tmp = nirs.io.AddDataValue(IDmeas[ch], "wavelengthIndex",
                                           lambda.IndexOf(data.probe.ChannelMap[ch].wavelength) + 1);
                //data_#/measurementList_#/dataType [int]
                tmp = nirs.io.AddDataValue(IDmeas[ch], "dataType", (int)data.probe.ChannelMap[ch].datatype);
                //data_#/measurementList_#/dataTypeIndex [int; index from 1]
                tmp = nirs.io.AddDataValue(IDmeas[ch], "dataTypeIndex", data.probe.ChannelMap[ch].dataindex + 1);

                //{optional}
                //data_#/measurementList_#/sourcePower [int]
                if(data.probe.ChannelMap[ch].SourcePower!=null){
                    tmp = nirs.io.AddDataValue(IDmeas[ch], "sourcePower", data.probe.ChannelMap[ch].SourcePower.Value);
                }

                //data_#/measurementList_#/detectorGain [int]
                if(data.probe.ChannelMap[ch].DetectorGain!=null){
                    tmp = nirs.io.AddDataValue(IDmeas[ch], "detectorGain", data.probe.ChannelMap[ch].DetectorGain.Value);
                }
                //data_#/measurementList_#/moduleIndex [int]
                if(data.probe.ChannelMap[ch].moduleIndex!=null){
                    tmp = nirs.io.AddDataValue(IDmeas[ch], "moduleIndex", data.probe.ChannelMap[ch].moduleIndex.Value);
                }


                H5G.close(IDmeas[ch]);
            }

            hid_t[] IDstim = new hid_t[data.stimulus.Count];
            tmp = nirs.io.AddDataValue(IDdata, "stimCount", data.stimulus.Count);
            for (int st = 0; st < data.stimulus.Count; st++)
            {
                IDstim[st] = H5G.create(IDdata, String.Format("stim{0}", st));
                // data_#/stim#/name [string]

                tmp = nirs.io.AddDataString(IDstim[st], "name",
                                            data.stimulus[st].name);

                // data_#/stim#/data [numeric] <#events x 3>  onset,dur,amp

                int k = data.stimulus[st].amplitude.Count;
                int n = data.stimulus[st].onsets.Count;
                double[,] evt = new double[n,2 + k];

                for (int i = 0; i < n; i++){
                    evt[i, 0] = data.stimulus[st].onsets[i];
                    evt[i, 1] = data.stimulus[st].duration[i];
                    evt[i, 2] = data.stimulus[st].amplitude[i];
                    
                }
                tmp = nirs.io.AddDataArray(IDstim[st], "data", evt);

                H5G.close(IDstim[st]);
            }


            hid_t probeIdx = H5G.create(IDnirs, "probe"); // used to be IDdata

            double[] wav = new double[lambda.Count];
            for (int i = 0; i < lambda.Count; i++){
                wav[i] = lambda[i];
            }
            // data#/probe/wavelengths [numeric]
            tmp = nirs.io.AddDataVector(probeIdx, "wavelengths", wav);

            // data#/probe/wavelengthsEmissions [numeric] 

            // data#/probe/sourcePos [numeric] 
            tmp = nirs.io.AddDataArray(probeIdx, "sourcePos", data.probe.SrcPos);

            // data#/probe/detectorPos [numeric] 
            tmp = nirs.io.AddDataArray(probeIdx, "detectorPos", data.probe.DetPos);

            if (data.probe.SrcPos3D != null)
            {
                // data#/probe/sourcePos [numeric] 
                tmp = nirs.io.AddDataArray(probeIdx, "sourcePos3D", data.probe.SrcPos3D);
            }

            if (data.probe.DetPos3D != null)
            {
                // data#/probe/detectorPos [numeric] 
                tmp = nirs.io.AddDataArray(probeIdx, "detectorPos3D", data.probe.DetPos3D);
            }

            // data#/probe/sourceLabels [string] 
            tmp = nirs.io.AddDataValue(probeIdx, "sourceCount", data.probe.numSrc);
            if (data.probe.SourceLabels == null)
            {
                data.probe.SourceLabels = new string[data.probe.numSrc];
                for (int i = 0; i < data.probe.numSrc; i++)
                {
                    data.probe.SourceLabels[i] = string.Format("Source-{0}", i + 1);
                }
            }

            for (int i = 0; i < data.probe.numSrc; i++){
                tmp = nirs.io.AddDataString(probeIdx, String.Format("sourceLabels{0}", i),
                                            data.probe.SourceLabels[i]);
            }

            // data#/probe/detectorLabels [string]
            tmp = nirs.io.AddDataValue(probeIdx, "detectorCount", data.probe.numDet);

            if (data.probe.DetectorLabels == null)
            {
                data.probe.DetectorLabels = new string[data.probe.numDet];
                for (int i = 0; i < data.probe.numDet; i++)
                {
                    data.probe.DetectorLabels[i] = string.Format("Detector-{0}", i + 1);
                }
            }


            for (int i = 0; i < data.probe.numDet; i++)
            {
                tmp = nirs.io.AddDataString(probeIdx, String.Format("detectorLabels{0}", i),
                                            data.probe.DetectorLabels[i]);
            }
            // data#/probe/landmarkLabels [string]
            if (data.probe.LandmarkLabels!=null){
                for (int i = 0; i < data.probe.LandmarkLabels.Length; i++)
                {
                    tmp = nirs.io.AddDataString(probeIdx, String.Format("landmarkLabels{0}", i),
                                                data.probe.LandmarkLabels[i]);
                }
            }
            // data#/probe/landmark [numeric array]
            if (data.probe.LandmarkPos!=null){
                tmp = nirs.io.AddDataArray(probeIdx, "landmark", data.probe.LandmarkPos);
            }



            if (data.probe.LandmarkPos3D != null)
            {
                tmp = nirs.io.AddDataArray(probeIdx, "landmark3D", data.probe.LandmarkPos3D);
            }

            // data#/probe/useLocalIndex [int] = 0
            if(data.probe.uselocalIndex!=null){
                tmp = nirs.io.AddDataValue(probeIdx, "useLocalIndex", data.probe.uselocalIndex.Value);
            }


            // misc string pairs
            tmp = nirs.io.AddDataValue(IDnirs, "metaDataTagCount", data.demographics.Keys.Count);
            for (int i = 0; i < data.demographics.Keys.Count; i++){

                hid_t metaIdx = H5G.create(IDnirs, String.Format("metaDataTags{0}",i));
                tmp = nirs.io.AddDataString(metaIdx, "name",
                                         data.demographics.Keys[i]);
                tmp = nirs.io.AddDataString(metaIdx, "value",
                                        (string)data.demographics.get(data.demographics.Keys[i]));
            }


            if (data.auxillaries != null)
            {
                hid_t[] IDaux = new hid_t[data.auxillaries.Length];
                for (int st = 0; st < data.auxillaries.Length; st++)
                {
                    IDaux[st] = H5G.create(IDdata, String.Format("aux{0}", st));
                    // data_#/aux#/name [string]
                    tmp = nirs.io.AddDataString(IDaux[st], "name", data.auxillaries[st].name);
                    // data_#/aux#/dataTimeSeries
                    tmp = nirs.io.AddDataVector(IDaux[st], "dataTimeSeries", data.auxillaries[st].data);
                    // data_#/aux#/time
                    tmp = nirs.io.AddDataVector(IDaux[st], "time", data.auxillaries[st].time);
                    // data_#/aux#/timeOffset
                    if(data.auxillaries[st].timeOffset!=null){
                        tmp = nirs.io.AddDataValue(IDaux[st], "timeOffset", data.auxillaries[st].timeOffset.Value);

                    }


                    H5G.close(IDaux[st]);
                }
            }



            return;
        }
      
        /// Helper functions to add values, strings, and arrays

        private static hid_t AddDataArray(hid_t parentLoc,string name, double[,] data){
            // Write the data to the parent field

            hid_t type = H5T.NATIVE_DOUBLE;

            hsize_t[] dims = new hsize_t[2];
            dims[0] = (hsize_t)data.GetLength(0);
            dims[1] = (hsize_t)data.GetLength(1);

            hid_t spaceId = H5S.create_simple(2, dims, null);
            hid_t dataSetId = H5D.create(parentLoc, name, type, spaceId);


            // Write the integer data to the data set.
            GCHandle hnd = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.write(dataSetId, type, H5S.ALL, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(dataSetId);

            return dataSetId;
        }


        private static hid_t AddDataVector(hid_t parentLoc, string name, double[] data)
        {
            // Write the data to the /nirs/data field
            hid_t type = H5T.NATIVE_DOUBLE;

            hsize_t[] dims = new hsize_t[1];
            dims[0] = (hsize_t)data.Length;

            hid_t spaceId = H5S.create_simple(1, dims, null);
            hid_t dataSetId = H5D.create(parentLoc, name, type, spaceId);


            // Write the integer data to the data set.
            GCHandle hnd = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.write(dataSetId, type, H5S.ALL, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(dataSetId);

            return dataSetId;
        }


        private static hid_t AddDataValue(hid_t parentLoc, string name, double data)
        {
            // Write the data to the /nirs/data field
            hid_t type = H5T.NATIVE_DOUBLE;
            hsize_t[] dims = new hsize_t[1];
            dims[0] = 1;

            hid_t spaceId = H5S.create_simple(1, dims, null);
            hid_t dataSetId = H5D.create(parentLoc, name, type, spaceId);


            // Write the integer data to the data set.
            GCHandle hnd = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.write(dataSetId, type, H5S.ALL, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(dataSetId);

            return dataSetId;
        }
        private static hid_t AddDataValue(hid_t parentLoc, string name, int data)
        {
            // Write the data to the /nirs/data field
            hid_t type = H5T.NATIVE_INT;
            hsize_t[] dims = new hsize_t[1];
            dims[0] = 1;

            hid_t spaceId = H5S.create_simple(1, dims, null);
            hid_t dataSetId = H5D.create(parentLoc, name, type, spaceId);


            // Write the integer data to the data set.
            GCHandle hnd = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.write(dataSetId, type, H5S.ALL, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(dataSetId);
            
            return dataSetId;
        }

        private static hid_t AddDataString(hid_t parentLoc, string name, string data)
        {
            // Write the data to the /nirs/data field
            char[] thisstr = data.ToCharArray();

            hsize_t[] dims = new hsize_t[1];
            dims[0] = (hsize_t)thisstr.Length*2;

            //hid_t type = H5T.NATIVE_CHAR;
            hid_t type = H5T.C_S1;
            hid_t spaceId = H5S.create_simple(1, dims, null);
            hid_t dataSetId = H5D.create(parentLoc, name, type, spaceId);


            // Write the integer data to the data set.
            GCHandle hnd = GCHandle.Alloc(thisstr, GCHandleType.Pinned);
            H5D.write(dataSetId, type, H5S.ALL, H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(dataSetId);

            return dataSetId;
        }


    }
}



