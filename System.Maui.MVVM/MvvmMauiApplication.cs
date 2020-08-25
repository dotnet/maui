using System;
using System.Collections.Generic;
using System.Linq;
using System.Maui.Platform;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

namespace System.Maui.Sample.Android
{
	public class MvvmMauiApplication
	{
		static Lazy<MvvmMauiApplication> _current = new Lazy<MvvmMauiApplication>(OnCreateMauiApplication);
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