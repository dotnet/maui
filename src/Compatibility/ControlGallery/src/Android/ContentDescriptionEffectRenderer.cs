using System.ComponentModel;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using AViews = Android.Views;

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
			var platformView = Control as AViews.View;

			if (platformView != null && viewGroup != null && viewGroup.ChildCount > 0)
			{
				platformView = viewGroup.GetChildAt(0);
			}

			if (platformView == null)
			{
				return;
			}

			var info = AccessibilityNodeInfoCompat.Obtain(platformView);
			ViewCompat.OnInitializeAccessibilityNodeInfo(platformView, info);

			System.Diagnostics.Debug.WriteLine(info.ContentDescription);
			System.Diagnostics.Debug.WriteLine(platformView.ContentDescription);

			Element.SetValue(
				ContentDescriptionEffectProperties.NameAndHelpTextProperty,
				info.ContentDescription);

			Element.SetValue(
				ContentDescriptionEffectProperties.ContentDescriptionProperty,
				platformView.ContentDescription);
		}

	}
}
