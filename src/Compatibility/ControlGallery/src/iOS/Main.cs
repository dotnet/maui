//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public class Application
	{
		static void Main(string[] args)
		{
			UIApplication.Main(args, typeof(CustomApplication), typeof(AppDelegate));
		}
	}
}
