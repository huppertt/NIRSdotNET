using System;
using System.Collections.Generic;
namespace nirs
{
    namespace modules
    {
        public class CPipeline
        {
            public List<nirs.modules.Cmodules> modules;

            public CPipeline()
            {
                modules = new List<Cmodules>();

            }

            public List<nirs.core.Data> run(List<nirs.core.Data> datas)
            {

                for(int i=0; i < modules.Count; i++)
                {
                    datas = modules[i].run(datas);
                }
                return datas;

            }

        }
    }
}
