
// This file has been generated by the GUI designer. Do not modify.
namespace rtfNIRS
{
	public partial class PipelineManager
	{
		private global::Gtk.UIManager UIManager;

		private global::Gtk.Action FileAction;

		private global::Gtk.Action LoadPipelineAction;

		private global::Gtk.Action SavePipelineAction;

		private global::Gtk.Action LoadDefaultPipelineAction;

		private global::Gtk.Action BasicPreprocessingAction;

		private global::Gtk.Action FirstLevelGLMAction;

		private global::Gtk.Action PlugInsAction;

		private global::Gtk.Action LoadPlugInsAction;

		private global::Gtk.Action gotoFirstAction;

		private global::Gtk.Action cancelAction;

		private global::Gtk.Action goUpAction;

		private global::Gtk.Action goDownAction;

		private global::Gtk.MenuBar menubar1;

		private global::Gtk.Fixed fixed7;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Frame frame2;

		private global::Gtk.Alignment GtkAlignment7;

		private global::Gtk.TreeView treeview4;

		private global::Gtk.Label GtkLabel9;

		private global::Gtk.VBox vbox3;

		private global::Gtk.Fixed fixed2;

		private global::Gtk.Toolbar toolbar1;

		private global::Gtk.Fixed fixed1;

		private global::Gtk.Frame frame1;

		private global::Gtk.Alignment GtkAlignment6;

		private global::Gtk.TreeView treeview3;

		private global::Gtk.Label GtkLabel8;

		private global::Gtk.Fixed fixed9;

		private global::Gtk.Frame frame3;

		private global::Gtk.Alignment GtkAlignment9;

		private global::Gtk.HBox hbox7;

		private global::Gtk.TextView textview5;

		private global::Gtk.Fixed fixed11;

		private global::Gtk.Table table1;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.Label GtkLabel11;

		private global::Gtk.Button button239;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.Button buttonOk;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget rtfNIRS.PipelineManager
			this.UIManager = new global::Gtk.UIManager();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup("Default");
			this.FileAction = new global::Gtk.Action("FileAction", "File", null, null);
			this.FileAction.ShortLabel = "File";
			w1.Add(this.FileAction, null);
			this.LoadPipelineAction = new global::Gtk.Action("LoadPipelineAction", "Load Pipeline", null, null);
			this.LoadPipelineAction.ShortLabel = "Load Pipeline";
			w1.Add(this.LoadPipelineAction, null);
			this.SavePipelineAction = new global::Gtk.Action("SavePipelineAction", "Save Pipeline", null, null);
			this.SavePipelineAction.ShortLabel = "Save Pipeline";
			w1.Add(this.SavePipelineAction, null);
			this.LoadDefaultPipelineAction = new global::Gtk.Action("LoadDefaultPipelineAction", "Load Default Pipeline", null, null);
			this.LoadDefaultPipelineAction.ShortLabel = "Load Default Pipeline";
			w1.Add(this.LoadDefaultPipelineAction, null);
			this.BasicPreprocessingAction = new global::Gtk.Action("BasicPreprocessingAction", "Basic Preprocessing", null, null);
			this.BasicPreprocessingAction.ShortLabel = "Basic Preprocessing";
			w1.Add(this.BasicPreprocessingAction, null);
			this.FirstLevelGLMAction = new global::Gtk.Action("FirstLevelGLMAction", "First-level GLM", null, null);
			this.FirstLevelGLMAction.ShortLabel = "First-level GLM";
			w1.Add(this.FirstLevelGLMAction, null);
			this.PlugInsAction = new global::Gtk.Action("PlugInsAction", "PlugIns", null, null);
			this.PlugInsAction.ShortLabel = "PlugIns";
			w1.Add(this.PlugInsAction, null);
			this.LoadPlugInsAction = new global::Gtk.Action("LoadPlugInsAction", "Load PlugIns", null, null);
			this.LoadPlugInsAction.ShortLabel = "Load PlugIns";
			w1.Add(this.LoadPlugInsAction, null);
			this.gotoFirstAction = new global::Gtk.Action("gotoFirstAction", "Add Module", null, "gtk-goto-first");
			this.gotoFirstAction.ShortLabel = "Add Module";
			w1.Add(this.gotoFirstAction, null);
			this.cancelAction = new global::Gtk.Action("cancelAction", "Remove Module", null, "gtk-cancel");
			this.cancelAction.ShortLabel = "Remove Module";
			w1.Add(this.cancelAction, null);
			this.goUpAction = new global::Gtk.Action("goUpAction", "Move Up", null, "gtk-go-up");
			this.goUpAction.ShortLabel = "Move Up";
			w1.Add(this.goUpAction, null);
			this.goDownAction = new global::Gtk.Action("goDownAction", "Move Down", null, "gtk-go-down");
			this.goDownAction.ShortLabel = "Move Down";
			w1.Add(this.goDownAction, null);
			this.UIManager.InsertActionGroup(w1, 0);
			this.AddAccelGroup(this.UIManager.AccelGroup);
			this.Name = "rtfNIRS.PipelineManager";
			this.Title = "fNIRS Analysis Pipeline Manager";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(20));
			this.Gravity = ((global::Gdk.Gravity)(5));
			// Internal child rtfNIRS.PipelineManager.VBox
			global::Gtk.VBox w2 = this.VBox;
			w2.Name = "dialog1_VBox";
			w2.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString(@"<ui><menubar name='menubar1'><menu name='FileAction' action='FileAction'><menuitem name='LoadPipelineAction' action='LoadPipelineAction'/><menuitem name='SavePipelineAction' action='SavePipelineAction'/><menu name='LoadDefaultPipelineAction' action='LoadDefaultPipelineAction'><menuitem name='BasicPreprocessingAction' action='BasicPreprocessingAction'/><menuitem name='FirstLevelGLMAction' action='FirstLevelGLMAction'/></menu></menu><menu name='PlugInsAction' action='PlugInsAction'><menuitem name='LoadPlugInsAction' action='LoadPlugInsAction'/></menu></menubar></ui>");
			this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget("/menubar1")));
			this.menubar1.Name = "menubar1";
			w2.Add(this.menubar1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(w2[this.menubar1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.fixed7 = new global::Gtk.Fixed();
			this.fixed7.HeightRequest = 47;
			this.fixed7.Name = "fixed7";
			this.fixed7.HasWindow = false;
			w2.Add(this.fixed7);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(w2[this.fixed7]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.frame2 = new global::Gtk.Frame();
			this.frame2.Name = "frame2";
			this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame2.Gtk.Container+ContainerChild
			this.GtkAlignment7 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment7.Name = "GtkAlignment7";
			this.GtkAlignment7.LeftPadding = ((uint)(12));
			// Container child GtkAlignment7.Gtk.Container+ContainerChild
			this.treeview4 = new global::Gtk.TreeView();
			this.treeview4.WidthRequest = 350;
			this.treeview4.HeightRequest = 350;
			this.treeview4.CanFocus = true;
			this.treeview4.Name = "treeview4";
			this.treeview4.EnableSearch = false;
			this.GtkAlignment7.Add(this.treeview4);
			this.frame2.Add(this.GtkAlignment7);
			this.GtkLabel9 = new global::Gtk.Label();
			this.GtkLabel9.Name = "GtkLabel9";
			this.GtkLabel9.LabelProp = "<b>Loaded Modules</b>";
			this.GtkLabel9.UseMarkup = true;
			this.frame2.LabelWidget = this.GtkLabel9;
			this.hbox1.Add(this.frame2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.frame2]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.fixed2 = new global::Gtk.Fixed();
			this.fixed2.HeightRequest = 140;
			this.fixed2.Name = "fixed2";
			this.fixed2.HasWindow = false;
			this.vbox3.Add(this.fixed2);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.fixed2]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString(@"<ui><toolbar name='toolbar1'><toolitem name='gotoFirstAction' action='gotoFirstAction'/><toolitem name='cancelAction' action='cancelAction'/><toolitem name='goUpAction' action='goUpAction'/><toolitem name='goDownAction' action='goDownAction'/></toolbar></ui>");
			this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget("/toolbar1")));
			this.toolbar1.Name = "toolbar1";
			this.toolbar1.Orientation = ((global::Gtk.Orientation)(1));
			this.toolbar1.ShowArrow = false;
			this.vbox3.Add(this.toolbar1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.toolbar1]));
			w9.Position = 1;
			// Container child vbox3.Gtk.Box+BoxChild
			this.fixed1 = new global::Gtk.Fixed();
			this.fixed1.HeightRequest = 140;
			this.fixed1.Name = "fixed1";
			this.fixed1.HasWindow = false;
			this.vbox3.Add(this.fixed1);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.fixed1]));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			this.hbox1.Add(this.vbox3);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vbox3]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.frame1 = new global::Gtk.Frame();
			this.frame1.Name = "frame1";
			this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame1.Gtk.Container+ContainerChild
			this.GtkAlignment6 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment6.Name = "GtkAlignment6";
			this.GtkAlignment6.LeftPadding = ((uint)(12));
			// Container child GtkAlignment6.Gtk.Container+ContainerChild
			this.treeview3 = new global::Gtk.TreeView();
			this.treeview3.WidthRequest = 350;
			this.treeview3.HeightRequest = 350;
			this.treeview3.CanFocus = true;
			this.treeview3.Name = "treeview3";
			this.treeview3.EnableSearch = false;
			this.GtkAlignment6.Add(this.treeview3);
			this.frame1.Add(this.GtkAlignment6);
			this.GtkLabel8 = new global::Gtk.Label();
			this.GtkLabel8.Name = "GtkLabel8";
			this.GtkLabel8.LabelProp = "<b>Avaliable Modules</b>";
			this.GtkLabel8.UseMarkup = true;
			this.frame1.LabelWidget = this.GtkLabel8;
			this.hbox1.Add(this.frame1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.frame1]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w15.Position = 0;
			// Container child vbox2.Gtk.Box+BoxChild
			this.fixed9 = new global::Gtk.Fixed();
			this.fixed9.HeightRequest = 22;
			this.fixed9.Name = "fixed9";
			this.fixed9.HasWindow = false;
			this.vbox2.Add(this.fixed9);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.fixed9]));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.frame3 = new global::Gtk.Frame();
			this.frame3.Name = "frame3";
			this.frame3.ShadowType = ((global::Gtk.ShadowType)(0));
			this.frame3.BorderWidth = ((uint)(20));
			// Container child frame3.Gtk.Container+ContainerChild
			this.GtkAlignment9 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment9.Name = "GtkAlignment9";
			this.GtkAlignment9.LeftPadding = ((uint)(12));
			// Container child GtkAlignment9.Gtk.Container+ContainerChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.textview5 = new global::Gtk.TextView();
			this.textview5.WidthRequest = 131;
			this.textview5.CanFocus = true;
			this.textview5.Name = "textview5";
			this.textview5.WrapMode = ((global::Gtk.WrapMode)(2));
			this.hbox7.Add(this.textview5);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.textview5]));
			w17.Position = 0;
			// Container child hbox7.Gtk.Box+BoxChild
			this.fixed11 = new global::Gtk.Fixed();
			this.fixed11.WidthRequest = 74;
			this.fixed11.HeightRequest = 158;
			this.fixed11.Name = "fixed11";
			this.fixed11.HasWindow = false;
			this.hbox7.Add(this.fixed11);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.fixed11]));
			w18.Position = 1;
			// Container child hbox7.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table1.WidthRequest = 317;
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(9));
			this.table1.BorderWidth = ((uint)(5));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.LabelProp = "Parameter";
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.LabelProp = "Value";
			this.table1.Add(this.label6);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.label6]));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.LabelProp = "Description";
			this.table1.Add(this.label7);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1[this.label7]));
			w21.LeftAttach = ((uint)(2));
			w21.RightAttach = ((uint)(3));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox7.Add(this.table1);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.table1]));
			w22.PackType = ((global::Gtk.PackType)(1));
			w22.Position = 2;
			w22.Fill = false;
			this.GtkAlignment9.Add(this.hbox7);
			this.frame3.Add(this.GtkAlignment9);
			this.GtkLabel11 = new global::Gtk.Label();
			this.GtkLabel11.Name = "GtkLabel11";
			this.GtkLabel11.LabelProp = "<b>Description</b>";
			this.GtkLabel11.UseMarkup = true;
			this.frame3.LabelWidget = this.GtkLabel11;
			this.vbox2.Add(this.frame3);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.frame3]));
			w25.Position = 2;
			w25.Expand = false;
			w25.Fill = false;
			w2.Add(this.vbox2);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(w2[this.vbox2]));
			w26.Position = 2;
			// Internal child rtfNIRS.PipelineManager.ActionArea
			global::Gtk.HButtonBox w27 = this.ActionArea;
			w27.Name = "dialog1_ActionArea";
			w27.Spacing = 10;
			w27.BorderWidth = ((uint)(5));
			w27.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.button239 = new global::Gtk.Button();
			this.button239.CanFocus = true;
			this.button239.Name = "button239";
			this.button239.UseStock = true;
			this.button239.UseUnderline = true;
			this.button239.Label = "gtk-help";
			this.AddActionWidget(this.button239, -11);
			global::Gtk.ButtonBox.ButtonBoxChild w28 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w27[this.button239]));
			w28.Expand = false;
			w28.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget(this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w29 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w27[this.buttonCancel]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget(this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w30 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w27[this.buttonOk]));
			w30.Position = 2;
			w30.Expand = false;
			w30.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 985;
			this.DefaultHeight = 897;
			this.Show();
			this.gotoFirstAction.Activated += new global::System.EventHandler(this.AddModule);
			this.cancelAction.Activated += new global::System.EventHandler(this.RemoveModule);
			this.goUpAction.Activated += new global::System.EventHandler(this.MoveUp);
			this.goDownAction.Activated += new global::System.EventHandler(this.ModuleDown);
			this.treeview4.RowActivated += new global::Gtk.RowActivatedHandler(this.SelectAvaliable);
			this.treeview3.RowActivated += new global::Gtk.RowActivatedHandler(this.SelectAvaliable);
			this.button239.Clicked += new global::System.EventHandler(this.HelpDlg);
			this.buttonCancel.Clicked += new global::System.EventHandler(this.CancelButton);
			this.buttonOk.Clicked += new global::System.EventHandler(this.AcceptButton);
		}
	}
}