using System;
using nirs;
using System.Xml;
using System.Collections.Generic;

namespace NIRS_Plugins
{


    [Serializable]
    public class ImportData : IPlugin
    {

        private parameters[] local_params;


        public ImportData()
        {
            local_params = new parameters[1];

            local_params[0] = new parameters();
            local_params[0].datatype = typeof(string);
            local_params[0].name = "Name";
            local_params[0].value = "new_data";
            local_params[0].description = "Name of variable for data derivative";

        }


        public string Name
        {
            get
            {
                return "ImportData";
            }
        }

        public string Description
        {
            get
            {
                return "Pulls a data derivative into a pipeline";
            }
        }
        public string ShortDescription
        {
            get
            {
                return "Import data";
            }
        }
        public string Citation
        {
            get
            {
                return "none";
            }
        }

        public parameters[] Parameters
        {
            get
            {
                return local_params;
            }
            set
            {
                local_params = Parameters;
            }

        }
        public Type DataIn_type
        {
            get
            {
                return typeof(nirs.core.Data);
            }
        }
        public Type DataOut_type
        {
            get
            {
                return typeof(nirs.core.Data);
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        // This is the actual code that is called by the plugin
        public object Run(object data)
        {
            data = ((nirs.core.Data)data).Clone();
            return data;
        }


    }

}