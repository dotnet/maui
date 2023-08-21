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
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat
{
	// This version of FrameRenderer is here for backward compatibility with anyone referencing 
	// FrameRenderer from this namespace
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.FrameRenderer instead")]
	public class FrameRenderer : FastRenderers.FrameRenderer
	{
		public FrameRenderer(Context context) : base(context)
		{

		}
	}
}