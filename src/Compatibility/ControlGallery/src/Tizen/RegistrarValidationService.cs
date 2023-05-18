using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Internals;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
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