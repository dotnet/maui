using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
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