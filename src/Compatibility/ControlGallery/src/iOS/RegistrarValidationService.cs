using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	[Preserve(AllMembers = true)]
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
				return true;

			var renderer = Platform.iOS.Platform.CreateRenderer(element);

#pragma warning disable CS0618 // Type or member is obsolete
			if (element is MasterDetailPage)
			{
				if (renderer == null
					|| renderer.GetType().Name == "DefaultRenderer"
					|| (Device.Idiom == TargetIdiom.Tablet && !(renderer is TabletMasterDetailRenderer))
					|| (Device.Idiom == TargetIdiom.Phone && !(renderer is PhoneMasterDetailRenderer))
					)
				{
					message = $"Failed to load proper iOS renderer for {element.GetType().Name}";
					return false;
				}

				return true;
			}
#pragma warning restore CS0618 // Type or member is obsolete

			if (renderer == null
				|| renderer.GetType().Name == "DefaultRenderer"
				|| (element is FlyoutPage && Device.Idiom == TargetIdiom.Tablet && !(renderer is TabletFlyoutPageRenderer))
				|| (element is FlyoutPage && Device.Idiom == TargetIdiom.Phone && !(renderer is PhoneFlyoutPageRenderer))
				)
			{
				message = $"Failed to load proper iOS renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}