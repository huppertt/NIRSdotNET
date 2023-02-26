using System;
using System.Collections.Generic;
namespace nirs
{
    namespace modules
    {
        public class Cmodules
        {
            public string name;

            public Cmodules()
            {
                name = "Generic";
            }

            public List<nirs.core.Data> run(List<nirs.core.Data> datas)
            {
                return datas;
            }


        }
    }
}
