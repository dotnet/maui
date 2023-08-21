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

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class Issue3076Button : Button
	{
		public static readonly BindableProperty HorizontalContentAlignmentProperty =
			BindableProperty.Create("HorizontalContentAlignemnt", typeof(TextAlignment), typeof(Issue3076Button), TextAlignment.Center);

		public TextAlignment HorizontalContentAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalContentAlignmentProperty); }
			set { SetValue(HorizontalContentAlignmentProperty, value); }
		}
	}
}