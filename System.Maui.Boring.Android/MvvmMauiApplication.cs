using System;
using System.Collections.Generic;
using System.Linq;
using System.Maui.Platform;
using System.Text;

using Xamarin.Forms;

namespace System.Maui.Sample.Android
{
	public class BoringMauiApplication
	{
		static Lazy<BoringMauiApplication> _current = new Lazy<BoringMauiApplication>(OnCreateMauiApplication);
		private MvvmMauiApplication()
        {
			Registrar.Handlers.Register<Entry, EntryRenderer>();
			Registrar.Handlers.Register<Label, LabelRenderer>();
		}

		public static MvvmMauiApplication Current => _current.Value;

		static MvvmMauiApplication OnCreateMauiApplication()
        {
			return new MvvmMauiApplication();
        }

		public void InitWindow(object newWindow)
		{

		}
	}
}