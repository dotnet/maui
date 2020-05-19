using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls;
using System.Maui.Platform.UWP;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null || element is OpenGLView)
				return true;

			var renderer = Platform.UWP.Platform.CreateRenderer(element);

			if (renderer == null 
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load proper UWP renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}