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

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class MacTwitterDemo : FlyoutPage
	{
		public MacTwitterDemo()
		{
			InitializeComponent();
			lstTweets.ItemsSource = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
		}
	}
}
