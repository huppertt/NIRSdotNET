using System;
using System.Collections.Generic;
using Gtk;
namespace nirs
{
    public partial class core
    {

        public class CSession
        {
            public string SessionName;
            public List<nirs.core.Data> datas;
            public ListStore data_derivatives;
            public int selected;

            public CSession()
            {
                SessionName = "New Session";
                datas = new List<core.Data>();
                data_derivatives = new ListStore(typeof(object), typeof(string));
                selected = -1;
            }

            public void Add_DataDerivative(object datad,string name)
            {

                data_derivatives.AppendValues(datad, name);

            }

        }
    }
}