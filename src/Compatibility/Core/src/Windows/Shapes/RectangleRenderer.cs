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

using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using FormsRectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

#if WINDOWS
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WRectangle = System.Windows.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class RectangleRenderer : ShapeRenderer<FormsRectangle, WRectangle>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<FormsRectangle> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WRectangle());
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateRadiusX();
				UpdateRadiusY();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == FormsRectangle.RadiusXProperty.PropertyName)
				UpdateRadiusX();
			else if (args.PropertyName == FormsRectangle.RadiusYProperty.PropertyName)
				UpdateRadiusY();
		}

		void UpdateRadiusX()
		{
			Control.RadiusX = Element.RadiusX;
		}

		void UpdateRadiusY()
		{
			Control.RadiusY = Element.RadiusY;
		}
	}
}