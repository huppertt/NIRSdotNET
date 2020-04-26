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

        public static core.Data[] readSNIRF(string filename)
        {



            hid_t fileId = H5F.open(filename, H5F.ACC_RDONLY);
            core.Data[] data = new core.Data[0];

            string nirsfld = "/nirs";

            int idx = 0;
            while (true)
            {
                if (idx == 0) { nirsfld = "/nirs"; }
                else { nirsfld = String.Format("/nirs{0}", idx); }

                if (nirs.io.groupexists(fileId, String.Format("{0}/formatVersion", nirsfld)))
                {

                    string formatVersion = nirs.io.ReadDataString(fileId, String.Format("{0}/formatVersion", nirsfld));
                    int dataCount = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/dataCount", nirsfld));
                    core.Data[] dataT = new core.Data[data.Length + dataCount];
                    for (int i = 0; i < data.Length; i++)
                    {
                        dataT[i] = data[i];
                    }
                    data = dataT;


                    for (int i = 0; i < dataCount; i++)
                    {
                        data[i] = new core.Data();
                        data[i].time = nirs.io.ReadDataVector(fileId, String.Format("{0}/data{1}/time", nirsfld, i));

                        data[i].data = nirs.io.ReadDataArray(fileId, String.Format("{0}/data{1}/dataTimeSeries", nirsfld, i));


                        double[] wav = nirs.io.ReadDataVector(fileId, String.Format("{0}/probe/wavelengths", nirsfld));


                        int mlCount = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementListCount", nirsfld));
                        data[i].probe.numChannels = mlCount;
                        data[i].numsamples = data[i].time.Length;

                        data[i].probe.ChannelMap = new ChannelMap[mlCount];
                        for (int j = 0; j < mlCount; j++)
                        {
                            data[i].probe.ChannelMap[j] = new ChannelMap();

                            data[i].probe.ChannelMap[j].sourceindex = (int)nirs.io.ReadDataValue(fileId,
                                                                        String.Format("{0}/measurementList{1}/sourceIndex", nirsfld, j)) - 1;
                            data[i].probe.ChannelMap[j].detectorindex = (int)nirs.io.ReadDataValue(fileId,
                                                                        String.Format("{0}/measurementList{1}/detectorIndex", nirsfld, j)) - 1;
                            data[i].probe.ChannelMap[j].wavelength = wav[(int)nirs.io.ReadDataValue(fileId,
                                                                        String.Format("{0}/measurementList{1}/wavelengthIndex", nirsfld, j)) - 1];

                            int datatypeIdx = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/measurementList{1}/dataType", nirsfld, j));


                            data[i].probe.ChannelMap[j].datatype = (datatype)Enum.ToObject(typeof(datatype), datatypeIdx);

                            data[i].probe.ChannelMap[j].datasubtype = String.Format("{0}nm", data[i].probe.ChannelMap[j].wavelength);
                            data[i].probe.ChannelMap[j].channelname = String.Format("Src{0}-Det{1}", data[i].probe.ChannelMap[j].sourceindex + 1,
                                                                                    data[i].probe.ChannelMap[j].detectorindex + 1);

                            //{optional}
                            //data_#/measurementList_#/sourcePower [int]
                            //data_#/measurementList_#/detectorGain [int]
                            //data_#/measurementList_#/moduleIndex [int]

                        }

                        data[i].probe.numSrc = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/probe/sourceCount", nirsfld));
                        data[i].probe.numDet = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/probe/detectorCount", nirsfld));
                        data[i].probe.SrcPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos", nirsfld));
                        data[i].probe.DetPos = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos", nirsfld));


                        if (nirs.io.groupexists(fileId, String.Format("{0}/probe/sourcePos3D", nirsfld)))
                        {
                            data[i].probe.SrcPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/sourcePos3D", nirsfld));
                        }
                        if (nirs.io.groupexists(fileId, String.Format("{0}/probe/detectorPos3D", nirsfld)))
                        {
                            data[i].probe.DetPos3D = nirs.io.ReadDataArray(fileId, String.Format("{0}/probe/detectorPos3D", nirsfld));
                        }

                        // data#/probe/sourceLabels [string] 
                        // data#/probe/detectorLabels [string]


                        // data#/probe/landmark [numeric array]
                        // data#/probe/landmarkLabels [string]
                        // data#/probe/useLocalIndex [int] = 0

                        if (nirs.io.groupexists(fileId, String.Format("{0}/metaDataTagCount", nirsfld)))
                        {
                            int metaCount = (int)nirs.io.ReadDataValue(fileId, String.Format("{0}/metaDataTagCount", nirsfld));
                            data[i].demographics = new Dictionary();
                            for (int j = 0; j < metaCount; j++)
                            {
                                string name = nirs.io.ReadDataString(fileId, String.Format("{0}/metaDataTag{1}/name", nirsfld, j));
                                string val = nirs.io.ReadDataString(fileId, String.Format("{0}//metaDataTag{1}/value", nirsfld, j));
                                data[i].demographics.set(name, val);
                            }
                        }




                    }
                }
                else
                {
                    break;
                }
                idx++;
            }
            return data;
        }

                   



              
        private static bool groupexists(hid_t fileLoc, string name)
        {
            hid_t dId = 0;
            try
            {
               dId = H5D.open(fileLoc, name);
            }
            catch{
                dId = 0;
            }
            return (dId > 0);

        }


         private static string ReadDataString(hid_t fileLoc, string name){

            hid_t dset = H5D.open(fileLoc, name);
            hid_t fspace = H5D.get_space(dset);
            hid_t count = H5S.get_simple_extent_ndims(fspace);

            hid_t type = H5D.get_type(dset);

            hsize_t[] dims = new hsize_t[count];
            hsize_t[] maxdims = new hsize_t[count];

            H5S.get_simple_extent_dims(fspace, dims, maxdims);
            H5S.close(fspace);


            byte[] rdata = new byte[dims[0]];

            hid_t mem_type = H5T.copy(H5T.NATIVE_CHAR);
            H5T.set_size(mem_type, new IntPtr(1));

            GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
                H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();

            H5T.close(mem_type);

            char[] val = new char[dims[0]];
            for (int i = 0; i < (int)dims[0]; i+=2)
            {
                val[i]=BitConverter.ToChar(rdata, i);
              
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


        private static double[,] ReadDataArray(hid_t fileLoc, string name)
        {

            hid_t dset = H5D.open(fileLoc, name);
            hid_t fspace = H5D.get_space(dset);
            hid_t count = H5S.get_simple_extent_ndims(fspace);

            hid_t type = H5D.get_type(dset);

            hsize_t[] dims = new hsize_t[count];
            hsize_t[] maxdims = new hsize_t[count];

            H5S.get_simple_extent_dims(fspace, dims, maxdims);
            H5S.close(fspace);


            byte[] rdata = new byte[dims[0]*dims[1] * 8];

            hid_t mem_type = H5T.copy(H5T.NATIVE_DOUBLE);
            H5T.set_size(mem_type, new IntPtr(8));

            GCHandle hnd = GCHandle.Alloc(rdata, GCHandleType.Pinned);
            H5D.read(dset, mem_type, H5S.ALL,
                H5S.ALL, H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();
           
            H5T.close(mem_type);

            double[,] val = new double[dims[0],dims[1]];
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


            byte[] rdata = new byte[dims[0]*8];

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
                val[i] = BitConverter.ToDouble(rdata, i*8);
            }
          
            return val;
        }


    }
}



