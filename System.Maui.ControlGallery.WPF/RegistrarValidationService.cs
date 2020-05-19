using System.Maui;
using System.Maui.ControlGallery.WPF;
using System.Maui.Controls;
using System.Maui.Platform.WPF;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace System.Maui.ControlGallery.WPF
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null || element is OpenGLView)
				return true;

			var renderer = Platform.WPF.Platform.GetOrCreateRenderer(element);

			if (renderer == null 
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load proper WPF renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}