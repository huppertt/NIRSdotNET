
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;

	private global::Gtk.Action FileAction;

	private global::Gtk.Action ViewAction;

	private global::Gtk.Action LoadDataAction;

	private global::Gtk.Action LoadNirsAction;

	private global::Gtk.Action LoadSnirfAction;

	private global::Gtk.Action LoadNIRxAction;

	private global::Gtk.Action SaveDataAction;

	private global::Gtk.Action SaveNirsAction;

	private global::Gtk.Action SaveSnirfAction;

	private global::Gtk.Action ExitAction;

	private global::Gtk.ToggleAction TwoDimensionalAction;

	private global::Gtk.ToggleAction TenTwentyViewAction;

	private global::Gtk.VBox vbox3;

	private global::Gtk.MenuBar menubar2;

	private global::Gtk.MenuBar menubar1;

	private global::Gtk.HBox hbox1;

	private global::Gtk.DrawingArea drawingarea_main;

	private global::Gtk.VSeparator vseparator1;

	private global::Gtk.VBox vbox1;

	private global::Gtk.DrawingArea drawingareaSDG;

	private global::Gtk.HSeparator hseparator1;

	private global::Gtk.ComboBox combobox1;

	private global::Gtk.Notebook notebook1;

	private global::Gtk.ScrolledWindow GtkScrolledWindow;

	private global::Gtk.TextView textFileInfo;

	private global::Gtk.Label label1;

	private global::Gtk.ScrolledWindow GtkScrolledWindow1;

	private global::Gtk.TextView textEventInfo;

	private global::Gtk.Label label3;

	private global::Gtk.Label label2;

	protected virtual void Build()
	{
		global::Stetic.Gui.Initialize(this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup("Default");
		this.FileAction = new global::Gtk.Action("FileAction", global::Mono.Unix.Catalog.GetString("File"), null, null);
		this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString("File");
		w1.Add(this.FileAction, null);
		this.ViewAction = new global::Gtk.Action("ViewAction", global::Mono.Unix.Catalog.GetString("View"), null, null);
		this.ViewAction.ShortLabel = global::Mono.Unix.Catalog.GetString("View");
		w1.Add(this.ViewAction, null);
		this.LoadDataAction = new global::Gtk.Action("LoadDataAction", global::Mono.Unix.Catalog.GetString("Load Data"), null, null);
		this.LoadDataAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Load Data");
		w1.Add(this.LoadDataAction, null);
		this.LoadNirsAction = new global::Gtk.Action("LoadNirsAction", global::Mono.Unix.Catalog.GetString("Load *.nirs"), null, null);
		this.LoadNirsAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Load *.nirs");
		w1.Add(this.LoadNirsAction, null);
		this.LoadSnirfAction = new global::Gtk.Action("LoadSnirfAction", global::Mono.Unix.Catalog.GetString("Load *.snirf"), null, null);
		this.LoadSnirfAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Load *.snirf");
		w1.Add(this.LoadSnirfAction, null);
		this.LoadNIRxAction = new global::Gtk.Action("LoadNIRxAction", global::Mono.Unix.Catalog.GetString("Load NIRx"), null, null);
		this.LoadNIRxAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Load NIRx");
		w1.Add(this.LoadNIRxAction, null);
		this.SaveDataAction = new global::Gtk.Action("SaveDataAction", global::Mono.Unix.Catalog.GetString("Save Data"), null, null);
		this.SaveDataAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Save Data");
		w1.Add(this.SaveDataAction, null);
		this.SaveNirsAction = new global::Gtk.Action("SaveNirsAction", global::Mono.Unix.Catalog.GetString("Save *.nirs"), null, null);
		this.SaveNirsAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Save *.nirs");
		w1.Add(this.SaveNirsAction, null);
		this.SaveSnirfAction = new global::Gtk.Action("SaveSnirfAction", global::Mono.Unix.Catalog.GetString("Save *.snirf"), null, null);
		this.SaveSnirfAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Save *.snirf");
		w1.Add(this.SaveSnirfAction, null);
		this.ExitAction = new global::Gtk.Action("ExitAction", global::Mono.Unix.Catalog.GetString("Exit"), null, null);
		this.ExitAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Exit");
		w1.Add(this.ExitAction, null);
		this.TwoDimensionalAction = new global::Gtk.ToggleAction("TwoDimensionalAction", global::Mono.Unix.Catalog.GetString("Two-Dimensional"), null, null);
		this.TwoDimensionalAction.Active = true;
		this.TwoDimensionalAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Two-Dimensional");
		w1.Add(this.TwoDimensionalAction, null);
		this.TenTwentyViewAction = new global::Gtk.ToggleAction("TenTwentyViewAction", global::Mono.Unix.Catalog.GetString("Ten-Twenty View"), null, null);
		this.TenTwentyViewAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Ten-Twenty View");
		w1.Add(this.TenTwentyViewAction, null);
		this.UIManager.InsertActionGroup(w1, 0);
		this.AddAccelGroup(this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox3 = new global::Gtk.VBox();
		this.vbox3.Name = "vbox3";
		this.vbox3.Spacing = 6;
		// Container child vbox3.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString(@"<ui><menubar name='menubar2'><menu name='FileAction' action='FileAction'><menu name='LoadDataAction' action='LoadDataAction'><menuitem name='LoadNirsAction' action='LoadNirsAction'/><menuitem name='LoadSnirfAction' action='LoadSnirfAction'/><menuitem name='LoadNIRxAction' action='LoadNIRxAction'/></menu><menu name='SaveDataAction' action='SaveDataAction'><menuitem name='SaveNirsAction' action='SaveNirsAction'/><menuitem name='SaveSnirfAction' action='SaveSnirfAction'/></menu><menuitem name='ExitAction' action='ExitAction'/></menu><menu name='ViewAction' action='ViewAction'><menuitem name='TwoDimensionalAction' action='TwoDimensionalAction'/><menuitem name='TenTwentyViewAction' action='TenTwentyViewAction'/></menu></menubar></ui>");
		this.menubar2 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget("/menubar2")));
		this.menubar2.Name = "menubar2";
		this.vbox3.Add(this.menubar2);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.menubar2]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox3.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString(@"<ui><menubar name='menubar1'><menu><menu><menuitem/><menuitem/><menuitem/></menu><menu><menuitem/><menuitem/></menu><menuitem/></menu><menu><menuitem/><menuitem/></menu><menu><menu><menuitem/><menuitem/><menuitem/></menu><menu><menuitem/><menuitem/></menu><menuitem/></menu><menu><menuitem/><menuitem/></menu></menubar></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox3.Add(this.menubar1);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.menubar1]));
		w3.Position = 1;
		w3.Expand = false;
		// Container child vbox3.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.drawingarea_main = new global::Gtk.DrawingArea();
		this.drawingarea_main.WidthRequest = 197;
		this.drawingarea_main.Name = "drawingarea_main";
		this.hbox1.Add(this.drawingarea_main);
		global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.drawingarea_main]));
		w4.Position = 0;
		// Container child hbox1.Gtk.Box+BoxChild
		this.vseparator1 = new global::Gtk.VSeparator();
		this.vseparator1.Name = "vseparator1";
		this.hbox1.Add(this.vseparator1);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vseparator1]));
		w5.Position = 1;
		w5.Expand = false;
		w5.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.vbox1 = new global::Gtk.VBox();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.drawingareaSDG = new global::Gtk.DrawingArea();
		this.drawingareaSDG.WidthRequest = 225;
		this.drawingareaSDG.Name = "drawingareaSDG";
		this.vbox1.Add(this.drawingareaSDG);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.drawingareaSDG]));
		w6.Position = 0;
		// Container child vbox1.Gtk.Box+BoxChild
		this.hseparator1 = new global::Gtk.HSeparator();
		this.hseparator1.Name = "hseparator1";
		this.vbox1.Add(this.hseparator1);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator1]));
		w7.Position = 1;
		w7.Expand = false;
		w7.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.combobox1 = global::Gtk.ComboBox.NewText();
		this.combobox1.Name = "combobox1";
		this.vbox1.Add(this.combobox1);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.combobox1]));
		w8.Position = 2;
		w8.Expand = false;
		w8.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.notebook1 = new global::Gtk.Notebook();
		this.notebook1.CanFocus = true;
		this.notebook1.Name = "notebook1";
		this.notebook1.CurrentPage = 0;
		// Container child notebook1.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.textFileInfo = new global::Gtk.TextView();
		this.textFileInfo.CanFocus = true;
		this.textFileInfo.Name = "textFileInfo";
		this.GtkScrolledWindow.Add(this.textFileInfo);
		this.notebook1.Add(this.GtkScrolledWindow);
		// Notebook tab
		this.label1 = new global::Gtk.Label();
		this.label1.Name = "label1";
		this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("File Info");
		this.notebook1.SetTabLabel(this.GtkScrolledWindow, this.label1);
		this.label1.ShowAll();
		// Container child notebook1.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.textEventInfo = new global::Gtk.TextView();
		this.textEventInfo.CanFocus = true;
		this.textEventInfo.Name = "textEventInfo";
		this.GtkScrolledWindow1.Add(this.textEventInfo);
		this.notebook1.Add(this.GtkScrolledWindow1);
		global::Gtk.Notebook.NotebookChild w12 = ((global::Gtk.Notebook.NotebookChild)(this.notebook1[this.GtkScrolledWindow1]));
		w12.Position = 1;
		// Notebook tab
		this.label3 = new global::Gtk.Label();
		this.label3.Name = "label3";
		this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Events");
		this.notebook1.SetTabLabel(this.GtkScrolledWindow1, this.label3);
		this.label3.ShowAll();
		// Notebook tab
		global::Gtk.Label w13 = new global::Gtk.Label();
		w13.Visible = true;
		this.notebook1.Add(w13);
		this.label2 = new global::Gtk.Label();
		this.label2.Name = "label2";
		this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("MetaData");
		this.notebook1.SetTabLabel(w13, this.label2);
		this.label2.ShowAll();
		this.vbox1.Add(this.notebook1);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.notebook1]));
		w14.Position = 3;
		this.hbox1.Add(this.vbox1);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vbox1]));
		w15.Position = 2;
		w15.Expand = false;
		w15.Fill = false;
		this.vbox3.Add(this.hbox1);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
		w16.Position = 2;
		this.Add(this.vbox3);
		if ((this.Child != null))
		{
			this.Child.ShowAll();
		}
		this.DefaultWidth = 589;
		this.DefaultHeight = 498;
		this.Show();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
		this.combobox1.Changed += new global::System.EventHandler(this.ChangeData);
	}
}
