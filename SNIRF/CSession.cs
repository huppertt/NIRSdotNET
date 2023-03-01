using System;
using System.Collections.Generic;
using Gtk;
namespace nirs
{
    public partial class core
    {

        public struct derivative
        {
            public string name;
            public object data;
        }

        public class CSession
        {
            public string SessionName;
            public List<nirs.core.Data> datas;
            public List<derivative> data_derivatives;
            public int selected;

            public CSession()
            {
                SessionName = "New Session";
                datas = new List<core.Data>();
                data_derivatives = new List<derivative>();
                selected = -1;
            }

            public List<string> GetTypes()
            {
                List<string> types = new List<string>();
                List<string> localtypes = datas[selected].probe.getTypes();

                foreach(string type in localtypes)
                {
                    types.Add("raw " + type);
                }

                Guid uuid = datas[selected].UUID;

                for (int i = 0; i < data_derivatives.Count; i++)
                {
                    for (int j = 0; j < ((List<nirs.core.Data>)data_derivatives[i].data).Count; j++)
                    {
                        if (uuid == ((nirs.core.Data)((List<nirs.core.Data>)data_derivatives[i].data)[j]).UUID)
                        {
                            List<string> localtypes2 = ((nirs.core.Data)((List<nirs.core.Data>)data_derivatives[i].data)[j]).probe.getTypes();
                            foreach (string type in localtypes2)
                            {
                                types.Add(data_derivatives[i].name + " " + type);
                            }

                        }
                    }
                }

                return types;
            }

           

            public void draw(Gdk.Drawable da, bool autoscale = false, double tMin = 0, bool manualscaleMin = false, double manualMin = -999999, bool manualscaleMax = false, double manualMax = 99999999)
            {

                if (datas.Count == 0)
                {
                    return;
                }
                datas[selected].draw(da, autoscale, tMin, manualscaleMin, manualMin, manualscaleMax, manualMax);
            }



            public void draw(Gdk.Drawable da, string datasubtype, bool autoscale = false, double tMin = 0, bool manualscaleMin = false, double manualMin = -999999, bool manualscaleMax = false, double manualMax = 99999999)
            {
                if (datas.Count == 0)
                {
                    return;
                }

              
                List<string> alltypes = GetTypes();

                List<string> localtypes = datas[selected].probe.getTypes();
                if (localtypes.Contains(datasubtype))
                {
                    datas[selected].draw(da, datasubtype, autoscale, tMin, manualscaleMin, manualMin, manualscaleMax, manualMax);
                    return;
                }

                foreach (string type in localtypes)
                {
                    if (datasubtype == "raw " + type)
                    {
                        datas[selected].draw(da, type, autoscale, tMin, manualscaleMin, manualMin, manualscaleMax, manualMax);

                        return;
                    }
                }


                if (!alltypes.Contains(datasubtype))
                {
                    return;
                }

                Guid uuid = datas[selected].UUID;

                for (int i = 0; i < data_derivatives.Count; i++)
                {
                    for (int j = 0; j < ((List<nirs.core.Data>)data_derivatives[i].data).Count; j++)
                    {
                        if (uuid == ((nirs.core.Data)((List<nirs.core.Data>)data_derivatives[i].data)[j]).UUID)
                        {
                            List<string> localtypes2 = ((nirs.core.Data)((List<nirs.core.Data>)data_derivatives[i].data)[j]).probe.getTypes();
                            foreach (string type in localtypes2)
                            {
                                if(datasubtype==data_derivatives[i].name + " " + type)
                                {
                                    ((nirs.core.Data)((List<nirs.core.Data>)data_derivatives[i].data)[j]).draw(da, type, autoscale, tMin, manualscaleMin, manualMin, manualscaleMax, manualMax);
                                    return;
                                }

                            }
                            
                        }
                    }
                }
                return;


            }

            public void Add_DataDerivative(object datad, string name)
            {
                

                List<string> types = GetTypes();
                if (types.Contains(name))
                {
                    Replace_DataDerivatives(datad, name);
                }
                else
                {
                    derivative local_derivative = new derivative();
                    local_derivative.data = datad;
                    local_derivative.name = name;
                    data_derivatives.Add(local_derivative);
                }
                return;
            }

            public string[] Get_DataDerivativeNames()
            {
                string[] names = new string[data_derivatives.Count];
                for (int i = 0; i < names.Length; i++)
                {

                    names[i] = data_derivatives[i].name;
                }
                return names;
            }

            public bool Remove_DataDerivtive(string name)
            {
                for (int i = 0; i < data_derivatives.Count; i++)
                {
                    if (data_derivatives[i].name == name)
                    {
                        data_derivatives.RemoveAt(i);
                        return true;
                    }

                }
                return false;
            }

            public object Get_DataDerivative(string name)
            {
                
                for (int i = 0; i < data_derivatives.Count; i++)
                {
                    if (data_derivatives[i].name == name)
                    {
                        return data_derivatives[i].data;
                    }

                }
                return null;
            }

            public bool Replace_DataDerivatives(object data,string name)
            {
                for (int i = 0; i < data_derivatives.Count; i++)
                {
                    if (data_derivatives[i].name == name)
                    {
                        derivative local_derivative = data_derivatives[i];
                        local_derivative.data = data;
                        data_derivatives[i] = local_derivative;
                        return true;
                    }

                }
                return false;
            }
        }
    }
}