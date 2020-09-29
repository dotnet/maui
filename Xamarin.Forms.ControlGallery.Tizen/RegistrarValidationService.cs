using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Tizen;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Tizen;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Xamarin.Forms.ControlGallery.Tizen
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null || element is OpenGLView)
				return true;

			var renderer = Platform.Tizen.Platform.GetOrCreateRenderer(element);

			if (renderer == null
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load proper Tizen renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}