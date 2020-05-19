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
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls.Issues;
using System.Maui.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Bugzilla38989._38989CustomViewCell), typeof(_38989CustomViewCellRenderer))]

namespace System.Maui.ControlGallery.Android
{
	public class _38989CustomViewCellRenderer : System.Maui.Platform.Android.ViewCellRenderer
	{
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			var nativeView = convertView;

			if (nativeView == null)
				nativeView = (context.GetActivity()).LayoutInflater.Inflate(Resource.Layout.Layout38989, null);

			return nativeView;
		}
	}
}