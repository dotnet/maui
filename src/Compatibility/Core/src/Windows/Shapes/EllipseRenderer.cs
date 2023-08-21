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

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Platform;

#if WINDOWS
using WEllipse = Microsoft.UI.Xaml.Shapes.Ellipse;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WEllipse = System.Windows.Shapes.Ellipse;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class EllipseRenderer : ShapeRenderer<Ellipse, WEllipse>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WEllipse());
			}

			base.OnElementChanged(args);
		}
	}
}
