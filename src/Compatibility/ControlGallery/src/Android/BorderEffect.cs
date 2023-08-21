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

using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			Control.SetBackgroundColor(global::Android.Graphics.Color.Aqua);

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			Control.SetBackgroundColor(global::Android.Graphics.Color.Beige);
		}
	}
}