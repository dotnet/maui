using System.ComponentModel;
using AViews = Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;

[assembly: ExportEffect(typeof(ContentDescriptionEffectRenderer), ContentDescriptionEffect.EffectName)]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
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
