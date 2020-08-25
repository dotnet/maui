using System;
using System.Collections.Generic;
using System.Linq;
using System.Maui.Boring.Android;
using System.Maui.Platform;
using System.Text;

using Xamarin.Forms;

namespace System.Maui.Sample.Android
{
	public class BoringMauiApplication
	{
		static Lazy<BoringMauiApplication> _current = new Lazy<BoringMauiApplication>(OnCreateMauiApplication);
		private BoringMauiApplication()
        {
			Registrar.Handlers.Register<BoringEntry, EntryRenderer>();
		}

		public static BoringMauiApplication Current => _current.Value;

		static BoringMauiApplication OnCreateMauiApplication()
        {
			return new BoringMauiApplication();
        }

		public void InitWindow(object newWindow)
		{

		}
	}
}