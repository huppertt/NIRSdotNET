#define HDF5_VER1_10
using System;
using HDF.PInvoke;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public static bool SNIRFValidation(string filename)
        {
            List<nirs.HDF5info> info = nirs.io.SNIRFinfo(filename);
            List<string> invalid = new List<string>();
            return SNIRFValidate(info,ref invalid);
        }

        public static bool SNIRFValidation(List<nirs.HDF5info> info)
        {
            List<string> invalid = new List<string>();
            return SNIRFValidate(info, ref invalid);
        }



        public static bool SNIRFValidate(List<nirs.HDF5info> info, ref List<string> invalid)
        {
            bool flag = false; 

            List<Regex> Required = new List<Regex>();
            List<Regex> Optional = new List<Regex>();

            Required.Add(new Regex(@"/formatVersion"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/dataTimeSeries"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/time"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/sourceIndex"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/detectorIndex"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/wavelengthIndex"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/dataType"));
            Required.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/dataTypeIndex"));
             Required.Add(new Regex(@"/nirs\d*/probe/wavelengths"));
            Required.Add(new Regex(@"/nirs\d*/probe/sourcePos\d*"));
            Required.Add(new Regex(@"/nirs\d*/probe/detectorPos\d*"));

            Optional.Add(new Regex(@"/nirs\d*/metaDataTags/\w*"));
            Optional.Add(new Regex(@"/nirs\d*/data\w*/measurementList\d*/ssourcePower"));
            Optional.Add(new Regex(@"/nirs\d*/data\w*/measurementList\d*/detectorGain"));
            Optional.Add(new Regex(@"/nirs\d*/data\w*/measurementList\d*/moduleIndex"));
            Optional.Add(new Regex(@"/nirs\d*/data\d*/measurementList\d*/dataTypeLabel"));
            Optional.Add(new Regex(@"/nirs\d*/stim\w*/name"));
            Optional.Add(new Regex(@"/nirs\d*/stim\w*/data"));

            Optional.Add(new Regex(@"/nirs\d*/aux\d*/name"));
            Optional.Add(new Regex(@"/nirs\d*/aux\d*/dataTimeSeries"));
            Optional.Add(new Regex(@"/nirs\d*/aux\d*/time"));
            Optional.Add(new Regex(@"/nirs\d*/aux\d*/timeOffset"));
            Optional.Add(new Regex(@"/nirs\d*/probe/wavelengthsEmission"));
            Optional.Add(new Regex(@"/nirs\d*/probe/sourcePos2D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/sourcePos3D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/detectorPos2D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/detectorPos3D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/frequencies"));
            Optional.Add(new Regex(@"/nirs\d*/probe/timeDelays"));
            Optional.Add(new Regex(@"/nirs\d*/probe/timeDelayWidths"));
            Optional.Add(new Regex(@"/nirs\d*/probe/momentOrders"));
            Optional.Add(new Regex(@"/nirs\d*/probe/correlationTimeDelays"));
            Optional.Add(new Regex(@"/nirs\d*/probe/correlationTimeDelayWidths"));
            Optional.Add(new Regex(@"/nirs\d*/probe/sourceLabels"));
            Optional.Add(new Regex(@"/nirs\d*/probe/detectorLabels"));
            Optional.Add(new Regex(@"/nirs\d*/probe/landmarkPos2D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/landmarkPos"));
            Optional.Add(new Regex(@"/nirs\d*/probe/landmarkPos3D"));
            Optional.Add(new Regex(@"/nirs\d*/probe/landmarkLabels"));
            Optional.Add(new Regex(@"/nirs\d*/probe/useLocalIndex"));


            bool[] IsValid = new bool[info.Count];
            for(int i=0; i<info.Count; i++)
            {
                IsValid[i] = false;
            }
            bool[] RequiredFound = new bool[Required.Count];
            for(int i=0; i<Required.Count; i++)
            {
                RequiredFound[i] = false;
            }

            bool[] OptionalFound = new bool[Optional.Count];
            for(int i = 0; i<Optional.Count; i++)
            {
                OptionalFound[i] = false;
            }

       

            for (int i=0; i<info.Count; i++)
            {
                for(int j=0; j<Required.Count; j++)
                {


                    bool result = Required[j].IsMatch(info[i].field);
                    IsValid[i] = (IsValid[i] | result);
                    RequiredFound[j] = (RequiredFound[j] | result);
                }
                for (int j = 0; j < Optional.Count; j++)
                {
                    bool result = Optional[j].IsMatch(info[i].field);
                    IsValid[i] = (IsValid[i] | result);
                    OptionalFound[j] = (OptionalFound[j] | result);
                }
            }

            flag = true;
            for (int j = 0; j < Required.Count; j++)
            {
                flag = (flag & RequiredFound[j]);
            }

            for (int i = 0; i < info.Count; i++)
            {
                if (!IsValid[i])
                {
                    invalid.Add(info[i].field);
                }
            }


                return flag;

        }
    }
}
