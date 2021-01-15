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

    public struct HDF5info
    {
        public string field;
        public string HDFclass;
        public string description;
    }

//TODO
//data.measurementList.sourcePower
//data.measurementList.detectorGain
//data.measurementList.moduleIndex
//probe.wavelengthsEmission
//probe.frequencies
//probe.timeDelays
//probe.timeDelayWidths
//probe.momentOrders
//probe.correlationTimeDelays
//probe.correlationTimeDelayWidths
//probe.useLocalIndex

    public static partial class io
    {  // methods devoted to file I/O

        public static List<HDF5info> SNIRFinfo(string filename)
        {
            List<HDF5info> fields = new List<HDF5info>();
            hid_t fileId = H5F.open(filename, H5F.ACC_RDONLY);

            hid_t gId =H5G.open(fileId, "/");
            string fullname = "";
            fields = ScanInfo(gId, fields,fullname);

            return fields;

        }

        private static List<HDF5info> ScanInfo(hid_t gId, List<HDF5info> fields,string fullname)
        {
            IntPtr MAX_NAME = new IntPtr(1024);
            System.Text.StringBuilder group_name = new System.Text.StringBuilder();
            System.Text.StringBuilder member_name = new System.Text.StringBuilder();
            IntPtr len = H5I.get_name(gId, group_name, MAX_NAME);

            hsize_t nobj = new hsize_t();
            H5G.get_num_objs(gId, ref nobj);
            
            for(int i=0; i<(int)nobj; i++)
            {
                member_name = new System.Text.StringBuilder();
                member_name.Capacity = 1024;
                IntPtr len2 = H5G.get_objname_by_idx(gId, (ulong)i, member_name, MAX_NAME);
                int objtype = H5G.get_objtype_by_idx(gId, (ulong)i);

                if(objtype == 0) //group
                {
                    hid_t gId2 = H5G.open(gId, member_name.ToString());
                    fields = ScanInfo(gId2,fields,string.Format("{0}/{1}", fullname, member_name));


                }
                else if (objtype == 1) //Object is a dataset.
                {
                    HDF5info hDF5Info = new HDF5info();
                    hid_t dset = H5D.open(gId, member_name.ToString());
                    hid_t fspace = H5D.get_space(dset);
                  
                    hid_t count = H5S.get_simple_extent_ndims(fspace);
                    hid_t type = H5D.get_type(dset);


                    hDF5Info.HDFclass = getH5Tstring(type);
                    hDF5Info.field = string.Format("{0}/{1}", fullname, member_name);
                    if(H5T.get_class(type) == H5T.class_t.STRING)
                    {
                        hDF5Info.description = nirs.io.ReadDataString(gId, string.Format("{0}",member_name));
                    }
                    else if (H5T.get_class(type) == H5T.class_t.FLOAT |
                            H5T.get_class(type) == H5T.class_t.INTEGER )
                    {

                        hsize_t[] dims = new hsize_t[count];
                        hsize_t[] maxdims = new hsize_t[count];
                        H5S.get_simple_extent_dims(fspace, dims, maxdims);
                        if(dims.Length==1 & dims[0]==1)
                        {
                            var  val = nirs.io.ReadDataValue(gId, string.Format("{0}", member_name));
                            hDF5Info.description = string.Format("{0}", val);
                        }
                        else if (dims.Length == 1 & dims[0] > 1)
                        {
                            hDF5Info.description = string.Format("Vector <{0} x 1>", dims[0]);
                        }
                        else
                        {
                            hDF5Info.description = string.Format("Array <{0} x {1}>", dims[0], dims[1]);

                            if(hDF5Info.field.Contains("dataTimeSeries") & dims[0] > dims[1])
                            {
                                hDF5Info.description += " TRANSPOSE WARNING ";
                            }
                            if (hDF5Info.field.Contains("Pos") & dims[1]!=3)
                            {
                                hDF5Info.description += " TRANSPOSE WARNING ";
                            }
                            if (hDF5Info.field.Contains("stim") & hDF5Info.field.Contains("data") & dims[1] != 3)
                            {
                                hDF5Info.description += " TRANSPOSE WARNING ";
                            }
                        }

                    }
                    else
                    {
                        hDF5Info.description = "";
                    }
                    fields.Add(hDF5Info);
                }

            }


            H5G.close(gId);


           return fields;
        }

        private static string getH5Tstring(hid_t typeT)
        {
            // TODO
            
            return String.Format("{0}", H5T.get_class(typeT));
        }


    }

    

}