using System;
using Gtk;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;

namespace Maui.SimpleSampleApp
{

	public class SimpleSampleGtkApplication : MauiGtkApplication
	{

		protected override MauiApp CreateMauiApp()
		{
			return Startup.CreateMauiApp();
		}

		public SimpleSampleGtkApplication() : base()
		{
			// TopContainerOverride = OnTopContainerOverride;
		}

		[Obsolete("TopContainerOverride is dismissed")]
		Widget OnTopContainerOverride(Widget nativePage)
		{
			var b = new Box(Orientation.Vertical, 0)
			{
				Expand = true,
				Margin = 5,

			};

			var txt = $"{typeof(Startup).Namespace}";
			var t = new Label(txt);
			t.SetBackgroundColor(Colors.White);
			t.SetForegroundColor(Colors.Coral);
			var but = new Button() { Label = "Gtk Test" };

			b.PackStart(t, false, false, 0);
			b.PackStart(but, false, false, 0);

			b.PackStart(nativePage, true, true, 0);

			return b;
		}

	}

}