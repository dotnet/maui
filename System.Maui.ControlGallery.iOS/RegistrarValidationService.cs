using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls;
using System.Maui.Internals;
using System.Maui.Platform.iOS;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace System.Maui.ControlGallery.iOS
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
				|| (element is MasterDetailPage && Device.Idiom == TargetIdiom.Tablet && !(renderer is TabletMasterDetailRenderer))
				|| (element is MasterDetailPage && Device.Idiom == TargetIdiom.Phone && !(renderer is PhoneMasterDetailRenderer))
				)
			{
				message = $"Failed to load proper iOS renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}