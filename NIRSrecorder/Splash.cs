using System;
namespace NIRSrecorder
{
	public partial class Splash : Gtk.Window
	{
		public Gtk.Label label;

		public Splash() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build2();

			this.SetPosition(Gtk.WindowPosition.Center);
		//	splashtext = this.splashlabel;

			this.Show();
        }

        private void Build2()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget NIRSrecorder.Splash
			this.Name = "NIRSrecorder.Splash";
			this.Title = global::Mono.Unix.Catalog.GetString("Splash");
			this.WindowPosition = Gtk.WindowPosition.CenterAlways;

			var buffer = System.IO.File.ReadAllBytes(@"extra/Splash.gif");
			var pixbuf = new Gdk.Pixbuf(buffer);

			int w, h;
			h = pixbuf.Height;
			w = pixbuf.Width;

			Gtk.VBox vBox = new Gtk.VBox();

			Gtk.Fixed fix = new Gtk.Fixed();
			Gtk.Image im = new Gtk.Image();

			im.SetSizeRequest(w, h);
			im.Pixbuf = pixbuf;
			fix.Add(im);
			vBox.Add(fix);

			label = new Gtk.Label();
			vBox.Add(label);
			this.Add(vBox);


			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 400;
			this.DefaultHeight = 300;
			this.Show();
		}

        
	}
}
