using System;
using System.Collections.Generic;


namespace NIRS_Plugins
{
    public struct parameters
    {
        public object value;
        public string name;
        public string description;
        public Type datatype;
    }


    public interface IPlugin : ICloneable
    {
        string Name { get; }
        string ShortDescription { get; }
        string Description { get; }
        string Citation { get; }
        parameters[] Parameters { get; set; }
        Type DataIn_type { get; }
        Type DataOut_type { get; }

        object Run(object dataIn);


    }

}
