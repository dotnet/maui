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

using Android.Content;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using APath = Android.Graphics.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class EllipseRenderer : ShapeRenderer<Ellipse, EllipseView>
	{
		public EllipseRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new EllipseView(Context));
			}

			base.OnElementChanged(args);
		}
	}

	public class EllipseView : ShapeView
	{
		public EllipseView(Context context) : base(context)
		{
			UpdateShape();
		}

		void UpdateShape()
		{
			var path = new APath();
			path.AddCircle(0, 0, 1, APath.Direction.Cw);
			UpdateShape(path);
		}
	}
}