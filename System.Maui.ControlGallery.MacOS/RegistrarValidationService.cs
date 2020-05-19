using System.Maui;
using System.Maui.ControlGallery.MacOS;
using System.Maui.Controls;
using System.Maui.Platform.MacOS;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace System.Maui.ControlGallery.MacOS
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
				return true;

			var renderer = Platform.MacOS.Platform.CreateRenderer(element);

			if (renderer == null 
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load proper MacOS renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}