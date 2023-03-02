using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gtk;

namespace rtfNIRS
{
    public partial class PipelineManager : Gtk.Dialog
    {

        public List<NIRS_Plugins.IPlugin> plugins;
        public List<NIRS_Plugins.IPlugin> pluginsLoaded;
        public ListStore avaliablelist;
        public ListStore loadedlist;
        public ListStore paramlist;

        private bool selected_loaded;
        private int selected_int;

        public PipelineManager()
        {
            this.Build();
            
            plugins = new List<NIRS_Plugins.IPlugin>();
            pluginsLoaded = new List<NIRS_Plugins.IPlugin>();

            string folder = "/Users/theodorehuppert/Desktop/rtNIRS_PlugIns";

            string[] files = System.IO.Directory.GetFiles(folder);
            foreach (string file in files)
            {
                if (file.EndsWith(".dll"))
                {

                    Assembly.LoadFile(System.IO.Path.GetFullPath(file));
                }
            }
            Type interfaceType = typeof(NIRS_Plugins.IPlugin);
            //Fetch all types that implement the interface IPlugin and are a class
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<String> ty = new List<String>();
            foreach (Assembly assem in assemblies)
            {
                if (!assem.IsDynamic)
                {
                    System.Type[] types = assem.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        if (type.GetInterface(nameof(NIRS_Plugins.IPlugin)) != null)
                        {
                            //  NIRS_Plugins.IPlugin plugin = (NIRS_Plugins.IPlugin)Activator.CreateInstance(type);
                            if (!ty.Contains(type.ToString()))
                            {
                                ty.Add(type.ToString());
                                plugins.Add((NIRS_Plugins.IPlugin)Activator.CreateInstance(type));
                            }
                        }

                    }
                }
            }

            

            // Update the data table
            Gtk.TreeViewColumn treeViewColumn1 = new TreeViewColumn();
            treeViewColumn1.Title = "Module";

            Gtk.TreeViewColumn treeViewColumn2 = new TreeViewColumn();
            treeViewColumn2.Title = "Description";

            Gtk.CellRendererText cell1 = new Gtk.CellRendererText();
            treeViewColumn1.PackStart(cell1, true);
            treeViewColumn1.AddAttribute(cell1, "text", 0);

            Gtk.CellRendererText cell2 = new Gtk.CellRendererText();
            treeViewColumn2.PackStart(cell2, true);
            treeViewColumn2.AddAttribute(cell2, "text", 1);

            treeview3.AppendColumn(treeViewColumn1);
            treeview3.AppendColumn(treeViewColumn2);


            avaliablelist = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));
            treeview3.Model = avaliablelist;



            Gtk.TreeViewColumn treeViewColumn1b = new TreeViewColumn();
            treeViewColumn1b.Title = "Module";

            Gtk.TreeViewColumn treeViewColumn2b = new TreeViewColumn();
            treeViewColumn2b.Title = "Description";

          
            Gtk.CellRendererText cell1b = new Gtk.CellRendererText();
            treeViewColumn1b.PackStart(cell1b, true);
            treeViewColumn1b.AddAttribute(cell1b, "text", 0);

            Gtk.CellRendererText cell2b = new Gtk.CellRendererText();
            treeViewColumn2b.PackStart(cell2b, true);
            treeViewColumn2b.AddAttribute(cell2b, "text", 1);

            treeview4.AppendColumn(treeViewColumn1b);
            treeview4.AppendColumn(treeViewColumn2b);



            loadedlist = new ListStore(typeof(string), typeof(string));
            treeview4.Model = loadedlist;


            for (int i = 0; i < plugins.Count; i++)
            {
                avaliablelist.AppendValues(plugins[i].Name, plugins[i].Description, plugins[i].DataIn_type.ToString(), plugins[i].DataOut_type.ToString());
            }

            pluginsLoaded = MainClass.win.loadedOfflineplugins;

            for (int i = 0; i < pluginsLoaded.Count; i++)
            {
                loadedlist.AppendValues(pluginsLoaded[i].Name, pluginsLoaded[i].Description);
            }




            return;
        }

        protected void SelectAvaliable(object o, RowActivatedArgs args)
        {
            int module = args.Path.Indices[0];



            NIRS_Plugins.parameters[] parameters;
            if (((TreeView)o).Name.Equals("treeview3"))
            {
                textview5.Buffer.Text = "(Avaliable) Pipeline Module: " + plugins[module].Name + '\r';
                textview5.Buffer.Text += "Description: " + plugins[module].Description + '\r';
                textview5.Buffer.Text += "Citation: " + plugins[module].Citation + '\r' + '\r' + '\r';
                textview5.Buffer.Text += "----------------------------------------------" + '\r' + '\r';
                textview5.Buffer.Text += "Input Data Type: " + plugins[module].DataIn_type.ToString() + '\r';
                textview5.Buffer.Text += "Output Data Type: " + plugins[module].DataOut_type.ToString() + '\r';
                parameters = plugins[module].Parameters;
                treeview4.Selection.UnselectAll();
                selected_loaded = false;
                selected_int = module;
            }
            else
            {
                textview5.Buffer.Text = "(Loaded) Pipeline Module: " + pluginsLoaded[module].Name + '\r';
                textview5.Buffer.Text += "Description: " + pluginsLoaded[module].Description + '\r';
                textview5.Buffer.Text += "Citation: " + pluginsLoaded[module].Citation + '\r' + '\r' + '\r';
                textview5.Buffer.Text += "----------------------------------------------" + '\r' + '\r';
                textview5.Buffer.Text += "Input Data Type: " + pluginsLoaded[module].DataIn_type.ToString() + '\r';
                textview5.Buffer.Text += "Output Data Type: " + pluginsLoaded[module].DataOut_type.ToString() + '\r';
                parameters = pluginsLoaded[module].Parameters;
                treeview3.Selection.UnselectAll();
                selected_loaded = true;
                selected_int = module;
            }

            foreach(Widget widget in table1.Children)
            {
                table1.Remove(widget);
            }
            table1.Resize((uint)(Math.Max(5, parameters.Length + 1)), 3);

            Gtk.Label labela = new Label("<b>Parameter</b>");
            labela.UseMarkup = true;
            table1.Attach(labela, 0, 1, 0,1);
            labela.Show();
            Gtk.Label labelb = new Label("<b>Value</b>");
            labelb.UseMarkup = true;
            table1.Attach(labelb, 1, 2, 0, 1);
            labelb.Show();
            Gtk.Label labelc = new Label("<b>Description</b>");
            labelc.UseMarkup = true;
            table1.Attach(labelc, 2, 3, 0, 1);
            labelc.Show();


            for (int i = 0; i < parameters.Length; i++)
            {

                int index = i;

                Gtk.Label label = new Label(String.Format("<b>{0}</b>", parameters[i].name));
                label.UseMarkup = true;
                table1.Attach(label, 0, 1, (uint)(i + 1), (uint)(i + 2));
                label.Show();
                Gtk.Label label2 = new Label(parameters[i].description);
                table1.Attach(label2, 2, 3, (uint)(i + 1), (uint)(i + 2));
                label2.Show();

                if (parameters[i].datatype == typeof(double))
                {
                    Gtk.Entry entry = new Entry(String.Format("{0}", parameters[i].value));
                    entry.IsEditable = true;
                    entry.Activated += (sender, e) => Entry_Changed(sender, e, index);
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }
                else if (parameters[i].datatype == typeof(double[]))
                {
                    string values = "";
                    foreach(double d in (double[])parameters[i].value)
                    {
                        values += d.ToString() + " ";
                    }
                    Gtk.Entry entry = new Entry(values);
                    entry.IsEditable = true;
                    entry.Activated += (sender, e) => Entry_Changed(sender, e, index);
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }
                else if (parameters[i].datatype == typeof(int))
                {
                    Gtk.Entry entry = new Entry(String.Format("{0}", parameters[i].value));
                    entry.IsEditable = true;
                    entry.Activated += (sender, e) => Entry_Changed(sender, e, index);
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }
                else if (parameters[i].datatype == typeof(string))
                {
                    Gtk.Entry entry = new Entry(String.Format("{0}", parameters[i].value));
                    entry.IsEditable = true;
                    entry.Activated += (sender, e) => Entry_Changed(sender, e, index);
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }
                else if (parameters[i].datatype == typeof(bool))
                {
                    Gtk.CheckButton entry = new CheckButton();
                    entry.Active = (bool)parameters[i].value;
                    entry.Toggled += (sender, e) => Entry_Changed(sender, e, index);
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }
                else if (parameters[i].datatype.IsEnum)
                {
                    Array val = Enum.GetValues(parameters[i].datatype);
                    Gtk.ComboBox entry = new ComboBox(Enum.GetNames(parameters[i].datatype));
                    int ii = 0;
                    for (ii = 0; ii < val.Length; ii++)
                    {
                        if ((int)parameters[i].value == (int)val.GetValue(ii))
                        {
                            break;
                        }
                    }
                    entry.Active = ii;
                    entry.Changed += (sender, e) => Entry_Changed(sender, e, index);
                    //
                    table1.Attach(entry, 1, 2, (uint)(i + 1), (uint)(i + 2));
                    entry.Show();
                }

            }
        }


        private void Entry_Changed(object sender, EventArgs e,int paramIdx)
        {
            //  throw new NotImplementedException();

            object val=null;
            if (sender.GetType() == typeof(Gtk.Entry))
            {
               val = ((Entry)sender).Text;
            }
            else if (sender.GetType() == typeof(Gtk.ComboBox))
            {
                val = ((ComboBox)sender).ActiveText;
            }
            else if (sender.GetType() == typeof(Gtk.CheckButton))
            {
                val = ((CheckButton)sender).Active;
            }

            if (val == null)
            {
                return;
            }

            Type type =null; 
            if (selected_loaded)
            {
                type = pluginsLoaded[selected_int].Parameters[paramIdx].datatype;
            }
            else
            {
                type = plugins[selected_int].Parameters[paramIdx].datatype;
            }

            if (type == typeof(double))
            {
                val = Convert.ToDouble(val);
            }else if (type == typeof(int))
            {
                val = Convert.ToInt16(val);
            }else if (type.IsEnum)
            {
                val = Enum.Parse(type, (string)val);
            }else if (type == typeof(double[]))
            {
                val = ((string)val).Trim();
                val = ((string)val).Split(' ');
                double[] val2 = new double[((string[])val).Length];
                for(int ii=0; ii<val2.Length; ii++)
                {
                    val2[ii]=Convert.ToDouble((val as string[])[ii]);
                }
                val = val2;
                
            }
            
            if (selected_loaded)
            {
                pluginsLoaded[selected_int].Parameters[paramIdx].value = val;
            }
            else
            {
               plugins[selected_int].Parameters[paramIdx].value = val;
            }

           return;
        }




        protected void AddModule(object sender, EventArgs e)
        {
            // Add the plugin to the pipeline
            TreeIter treeIter = new TreeIter();
            treeview3.Selection.GetSelected(out treeIter);
            TreePath tpath=avaliablelist.GetPath(treeIter);
            foreach(int i in tpath.Indices)
            {
                loadedlist.AppendValues(plugins[i].Name, plugins[i].Description);
                pluginsLoaded.Add((NIRS_Plugins.IPlugin)plugins[i].Clone());
            }
            
            return;
        }

        protected void RemoveModule(object sender, EventArgs e)
        {
            // Add the plugin to the pipeline
            TreeIter treeIter = new TreeIter();
            if(!treeview4.Selection.GetSelected(out treeIter))
            {
                return;
            }
            TreePath tpath = loadedlist.GetPath(treeIter);
            int[] idx = tpath.Indices;

            foreach (int i in idx)
            {

                pluginsLoaded.RemoveAt(i);
            }
            loadedlist.Remove(ref treeIter);
            return;
        }
        protected void MoveUp(object sender, EventArgs e)
        {
            TreeIter treeIter = new TreeIter();
            if (!treeview4.Selection.GetSelected(out treeIter))
            {
                return;
            }
            TreePath tpath = loadedlist.GetPath(treeIter);
            int[] idx = tpath.Indices;
            foreach (int i in idx)
            {
                if (i > 0)
                {
                    pluginsLoaded.Insert(i - 1, pluginsLoaded[i]);
                    pluginsLoaded.RemoveAt(i + 1);
                }
            }
            loadedlist.Clear();
            for(int i=0; i<pluginsLoaded.Count; i++)
            {
                loadedlist.AppendValues(pluginsLoaded[i].Name, pluginsLoaded[i].Description, pluginsLoaded[i].DataIn_type.ToString(), pluginsLoaded[i].DataOut_type.ToString());
            }

            
            
        }

        protected void ModuleDown(object sender, EventArgs e)
        {
            TreeIter treeIter = new TreeIter();
            if (!treeview4.Selection.GetSelected(out treeIter))
            {
                return;
            }
            TreePath tpath = loadedlist.GetPath(treeIter);
            int[] idx = tpath.Indices;
            foreach (int i in idx)
            {
                if (i < pluginsLoaded.Count)
                {
                    pluginsLoaded.Insert(i + 2, pluginsLoaded[i]);
                    pluginsLoaded.RemoveAt(i);
                }
            }
            loadedlist.Clear();
            for (int i = 0; i < pluginsLoaded.Count; i++)
            {
                loadedlist.AppendValues(pluginsLoaded[i].Name, pluginsLoaded[i].Description, pluginsLoaded[i].DataIn_type.ToString(), pluginsLoaded[i].DataOut_type.ToString());
            }

        }

        protected void HelpDlg(object sender, EventArgs e)
        {
        }

        protected void CancelButton(object sender, EventArgs e)
        {
            this.Destroy();
        }

        protected void AcceptButton(object sender, EventArgs e)
        {

            MainClass.win.loadedOfflineplugins = pluginsLoaded;
            this.Destroy();
        }
    }
}

