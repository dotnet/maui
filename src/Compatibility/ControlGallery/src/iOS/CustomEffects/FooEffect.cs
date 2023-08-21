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

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects.FooEffect), nameof(Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects.FooEffect))]

namespace Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects
{
	public class FooEffect : PlatformEffect
	{
		public FooEffect()
		{
		}

		protected override void OnAttached()
		{
		}

		protected override void OnDetached()
		{

		}
	}
}