
// This file has been generated by the GUI designer. Do not modify.
namespace NIRSrecorder
{
	public partial class ConnectDevices
	{
		private global::Gtk.VBox vbox5;

		private global::Gtk.Frame frame6;

		private global::Gtk.Alignment GtkAlignment2;

		private global::Gtk.HBox hbox14;

		private global::Gtk.ComboBox combobox_avail;

		private global::Gtk.Button button_connect;

		private global::Gtk.Button button_ident;

		private global::Gtk.Label GtkLabel2;

		private global::Gtk.Frame frame8;

		private global::Gtk.Alignment GtkAlignment3;

		private global::Gtk.HBox hbox13;

		private global::Gtk.ComboBox combobox_connected;

		private global::Gtk.Button button_discont;

		private global::Gtk.Button button234;

		private global::Gtk.Label GtkLabel5;

		private global::Gtk.Frame frame10;

		private global::Gtk.Alignment GtkAlignment4;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.TextView textview_info;

		private global::Gtk.Label GtkLabel6;

		private global::Gtk.Button buttonCancel;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget NIRSrecorder.ConnectDevices
			this.Name = "NIRSrecorder.ConnectDevices";
			this.Title = global::Mono.Unix.Catalog.GetString("Connect Devices");
			this.WindowPosition = ((global::Gtk.WindowPosition)(2));
			this.BorderWidth = ((uint)(13));
			this.Resizable = false;
			this.AllowGrow = false;
			this.DestroyWithParent = true;
			// Internal child NIRSrecorder.ConnectDevices.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox5 = new global::Gtk.VBox();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			this.vbox5.BorderWidth = ((uint)(5));
			// Container child vbox5.Gtk.Box+BoxChild
			this.frame6 = new global::Gtk.Frame();
			this.frame6.Name = "frame6";
			this.frame6.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame6.Gtk.Container+ContainerChild
			this.GtkAlignment2 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment2.Name = "GtkAlignment2";
			this.GtkAlignment2.LeftPadding = ((uint)(12));
			// Container child GtkAlignment2.Gtk.Container+ContainerChild
			this.hbox14 = new global::Gtk.HBox();
			this.hbox14.Name = "hbox14";
			this.hbox14.Spacing = 6;
			// Container child hbox14.Gtk.Box+BoxChild
			this.combobox_avail = global::Gtk.ComboBox.NewText();
			this.combobox_avail.AppendText(global::Mono.Unix.Catalog.GetString("---------------------"));
			this.combobox_avail.Name = "combobox_avail";
			this.combobox_avail.Active = 0;
			this.hbox14.Add(this.combobox_avail);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox14[this.combobox_avail]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox14.Gtk.Box+BoxChild
			this.button_connect = new global::Gtk.Button();
			this.button_connect.CanFocus = true;
			this.button_connect.Name = "button_connect";
			this.button_connect.UseUnderline = true;
			this.button_connect.Label = global::Mono.Unix.Catalog.GetString("Connect");
			this.hbox14.Add(this.button_connect);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox14[this.button_connect]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox14.Gtk.Box+BoxChild
			this.button_ident = new global::Gtk.Button();
			this.button_ident.CanFocus = true;
			this.button_ident.Name = "button_ident";
			this.button_ident.UseUnderline = true;
			this.button_ident.Label = global::Mono.Unix.Catalog.GetString("Identify");
			this.hbox14.Add(this.button_ident);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox14[this.button_ident]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.GtkAlignment2.Add(this.hbox14);
			this.frame6.Add(this.GtkAlignment2);
			this.GtkLabel2 = new global::Gtk.Label();
			this.GtkLabel2.Name = "GtkLabel2";
			this.GtkLabel2.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Avaliable Devices</b>");
			this.GtkLabel2.UseMarkup = true;
			this.frame6.LabelWidget = this.GtkLabel2;
			this.vbox5.Add(this.frame6);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.frame6]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.frame8 = new global::Gtk.Frame();
			this.frame8.Name = "frame8";
			this.frame8.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame8.Gtk.Container+ContainerChild
			this.GtkAlignment3 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment3.Name = "GtkAlignment3";
			this.GtkAlignment3.LeftPadding = ((uint)(12));
			// Container child GtkAlignment3.Gtk.Container+ContainerChild
			this.hbox13 = new global::Gtk.HBox();
			this.hbox13.Name = "hbox13";
			this.hbox13.Spacing = 6;
			// Container child hbox13.Gtk.Box+BoxChild
			this.combobox_connected = global::Gtk.ComboBox.NewText();
			this.combobox_connected.AppendText(global::Mono.Unix.Catalog.GetString("---------------------"));
			this.combobox_connected.Name = "combobox_connected";
			this.combobox_connected.Active = 0;
			this.hbox13.Add(this.combobox_connected);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox13[this.combobox_connected]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox13.Gtk.Box+BoxChild
			this.button_discont = new global::Gtk.Button();
			this.button_discont.CanFocus = true;
			this.button_discont.Name = "button_discont";
			this.button_discont.UseUnderline = true;
			this.button_discont.Label = global::Mono.Unix.Catalog.GetString("Disconnect");
			this.hbox13.Add(this.button_discont);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox13[this.button_discont]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			// Container child hbox13.Gtk.Box+BoxChild
			this.button234 = new global::Gtk.Button();
			this.button234.CanFocus = true;
			this.button234.Name = "button234";
			this.button234.UseUnderline = true;
			this.button234.Label = global::Mono.Unix.Catalog.GetString("Identify");
			this.hbox13.Add(this.button234);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox13[this.button234]));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			this.GtkAlignment3.Add(this.hbox13);
			this.frame8.Add(this.GtkAlignment3);
			this.GtkLabel5 = new global::Gtk.Label();
			this.GtkLabel5.Name = "GtkLabel5";
			this.GtkLabel5.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Connected Devices</b>");
			this.GtkLabel5.UseMarkup = true;
			this.frame8.LabelWidget = this.GtkLabel5;
			this.vbox5.Add(this.frame8);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.frame8]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.frame10 = new global::Gtk.Frame();
			this.frame10.Name = "frame10";
			this.frame10.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame10.Gtk.Container+ContainerChild
			this.GtkAlignment4 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment4.Name = "GtkAlignment4";
			this.GtkAlignment4.LeftPadding = ((uint)(12));
			// Container child GtkAlignment4.Gtk.Container+ContainerChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.textview_info = new global::Gtk.TextView();
			this.textview_info.CanFocus = true;
			this.textview_info.Name = "textview_info";
			this.textview_info.Editable = false;
			this.GtkScrolledWindow.Add(this.textview_info);
			this.GtkAlignment4.Add(this.GtkScrolledWindow);
			this.frame10.Add(this.GtkAlignment4);
			this.GtkLabel6 = new global::Gtk.Label();
			this.GtkLabel6.Name = "GtkLabel6";
			this.GtkLabel6.LabelProp = global::Mono.Unix.Catalog.GetString("<b>System</b>");
			this.GtkLabel6.UseMarkup = true;
			this.frame10.LabelWidget = this.GtkLabel6;
			this.vbox5.Add(this.frame10);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.frame10]));
			w17.Position = 2;
			w1.Add(this.vbox5);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(w1[this.vbox5]));
			w18.Position = 0;
			// Internal child NIRSrecorder.ConnectDevices.ActionArea
			global::Gtk.HButtonBox w19 = this.ActionArea;
			w19.Name = "dialog1_ActionArea";
			w19.Spacing = 10;
			w19.BorderWidth = ((uint)(5));
			w19.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Accept");
			this.AddActionWidget(this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w20 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w19[this.buttonCancel]));
			w20.Expand = false;
			w20.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 400;
			this.DefaultHeight = 300;
			this.Show();
			this.button_connect.Clicked += new global::System.EventHandler(this.Cont);
			this.button_ident.Clicked += new global::System.EventHandler(this.IDcont);
			this.button_discont.Clicked += new global::System.EventHandler(this.Discont);
			this.button234.Clicked += new global::System.EventHandler(this.IDdiscont);
			this.buttonCancel.Clicked += new global::System.EventHandler(this.ClickedOK);
		}
	}
}
