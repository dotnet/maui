using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IScrollView
	{
		bool ScrollBarsInitialized { get; set; }
		bool ScrollbarFadingEnabled { get; set; }
		void AwakenScrollBars();
	}
}