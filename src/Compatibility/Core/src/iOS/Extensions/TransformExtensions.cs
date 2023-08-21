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
using CoreGraphics;
using Microsoft.Maui.Controls.Shapes;
using ObjCRuntime;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class TransformExtensions
	{
		public static CGAffineTransform ToCGAffineTransform(this Transform transform)
		{
			if (transform == null)
				return CGAffineTransform.MakeIdentity();

			Matrix matrix = transform.Value;

			return new CGAffineTransform(
				new nfloat(matrix.M11),
				new nfloat(matrix.M12),
				new nfloat(matrix.M21),
				new nfloat(matrix.M22),
				new nfloat(matrix.OffsetX),
				new nfloat(matrix.OffsetY));
		}
	}
}