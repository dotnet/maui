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
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Platform;
using AViews = Android.Views;

[assembly: ExportEffect(typeof(ContentDescriptionEffectRenderer), ContentDescriptionEffect.EffectName)]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class ContentDescriptionEffectRenderer : PlatformEffect
	{
		protected override void OnAttached()
		{
		}

		protected override void OnDetached()
		{
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			System.Diagnostics.Debug.WriteLine("OnElementPropertyChanged" + args.PropertyName);

			var viewGroup = Control as AViews.ViewGroup;
			var nativeView = Control as AViews.View;

			if (nativeView != null && viewGroup != null && viewGroup.ChildCount > 0)
			{
				nativeView = viewGroup.GetChildAt(0);
			}

			if (nativeView == null)
			{
				return;
			}

			var info = AccessibilityNodeInfoCompat.Obtain(nativeView);
			ViewCompat.OnInitializeAccessibilityNodeInfo(nativeView, info);

			System.Diagnostics.Debug.WriteLine(info.ContentDescription);
			System.Diagnostics.Debug.WriteLine(nativeView.ContentDescription);

			Element.SetValue(
				ContentDescriptionEffectProperties.NameAndHelpTextProperty,
				info.ContentDescription);

			Element.SetValue(
				ContentDescriptionEffectProperties.ContentDescriptionProperty,
				nativeView.ContentDescription);
		}

	}
}
