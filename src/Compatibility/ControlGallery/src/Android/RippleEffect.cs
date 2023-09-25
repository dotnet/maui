using Android.Util;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using AView = Android.Views.View;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.ControlGallery.Android.RippleEffect), nameof(Microsoft.Maui.Controls.ControlGallery.Android.RippleEffect))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	[Preserve(AllMembers = true)]
	public class RippleEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				if (Container is AView view)
				{
					view.Clickable = true;
					view.Focusable = true;

					using (var outValue = new TypedValue())
					{
						// TODO Do we need to see why this isn't generated
						//view.Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, outValue, true);
						view.SetBackgroundResource(outValue.ResourceId);
					}
				}
			}
			catch
			{

			}
		}

		protected override void OnDetached()
		{

		}
	}
}
