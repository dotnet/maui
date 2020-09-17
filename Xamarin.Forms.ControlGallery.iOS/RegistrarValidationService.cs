using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Xamarin.Forms.ControlGallery.iOS
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

			if (renderer == null 
				|| renderer.GetType().Name == "DefaultRenderer"
				|| (element is FlyoutPage && Device.Idiom == TargetIdiom.Tablet && !(renderer is TabletMasterDetailRenderer))
				|| (element is FlyoutPage && Device.Idiom == TargetIdiom.Phone && !(renderer is PhoneMasterDetailRenderer))
				)
			{
				message = $"Failed to load proper iOS renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}