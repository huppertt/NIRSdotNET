using System;
using System.Collections.Generic;
namespace nirs
{
    public enum probedisplay
    {
        TwoDimensional = 1,
        TenTwenty = 2
      //  ThreeDimensional = 3   // TODO not implimented yet
    }



    public enum stimtypes
    {
        StimEvent = 0,
        StimVector = 1
    }

    public struct ChannelMap{
        public datatype datatype;
        public int sourceindex;  // number of the Source Optode
        public int detectorindex; // number of the Detector Optode
        public double wavelength; // wavelength.  use -1 for not applicable
        public string datasubtype; // string: e.g HbO or "690nm"
        public string channelname; // string e.g. "Src1-Det2 (690nm)" used in legends for drawing  
        public int dataindex;  // row number of data in the Data matrix.

        public double? DetectorGain;
        public double? SourcePower;
        public int? moduleIndex;
    }

    public struct Stimulus{
        public stimtypes type;
        public string name;
        public List<double> onsets;   // if event, then holds descret start times/ else sample times
        public List<double> duration;  // if not event then duration should be ignored
        public List<double> amplitude; // if event, then holds descret amp/ else magnitude of time course
        public List<string> metadatalabels;
    }

    public struct auxillary{
        public string name;
        public double[] time;
        public double[] data;
        public double? timeOffset;
    }


    public class Dictionary
    {
        private List<Object> Values;
        public List<string> Keys;
        public Dictionary()
        {
            Values = new List<object>();
            Keys = new List<string>();
        }
        public object get(string key)
        {
            int index = Keys.IndexOf(key);
            if (index == -1)
            {
                return null;
            }
            else
            {
                return Values[index];
            }
        }
        public void set(string key, object value)
        {
            int index = Keys.IndexOf(key);
            if (index == -1)
            {
                Values.Add(value);
                Keys.Add(key);
            }
            else
            {
                Values[index] = value;
            }
        }
        public int length(){
            return Values.Count;
        }

    }


}
