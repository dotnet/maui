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
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Bugzilla38989._38989CustomViewCell), typeof(_38989CustomViewCellRenderer))]

namespace Xamarin.Forms.ControlGallery.Android
{
	public class _38989CustomViewCellRenderer : Xamarin.Forms.Platform.Android.ViewCellRenderer
	{
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			var nativeView = convertView;

			if (nativeView == null)
				nativeView = (context as Activity).LayoutInflater.Inflate(Resource.Layout.Layout38989, null);

			return nativeView;
		}
	}
}