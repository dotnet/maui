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
using Path = Microsoft.Maui.Controls.Shapes.Path;
using Microsoft.Maui.Controls.Platform;

#if WINDOWS
using WPath = Microsoft.UI.Xaml.Shapes.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WPath = System.Windows.Shapes.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PathRenderer : ShapeRenderer<Path, WPath>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Path> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WPath());
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateData();
				UpdateRenderTransform();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Path.DataProperty.PropertyName)
				UpdateData();
			else if (args.PropertyName == Path.RenderTransformProperty.PropertyName)
				UpdateRenderTransform();
		}

		void UpdateData()
		{
			Control.Data = Element.Data.ToPlatform();
		}

		void UpdateRenderTransform()
		{
			if (Element.RenderTransform != null)
				Control.RenderTransform = Element.RenderTransform.ToWindows();
		}
	}
}