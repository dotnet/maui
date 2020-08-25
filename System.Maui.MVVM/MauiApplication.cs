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
	public class MauiApplication
	{
		static Lazy<MauiApplication> _current = new Lazy<MauiApplication>(OnCreateMauiApplication);
		private MauiApplication()
        {
			Registrar.Handlers.Register<Entry, EntryRenderer>();
			Registrar.Handlers.Register<Label, LabelRenderer>();
		}

		public static MauiApplication Current => _current.Value;

		static MauiApplication OnCreateMauiApplication()
        {
			return new MauiApplication();
        }

		public void InitWindow(object newWindow)
		{

		}
	}
}