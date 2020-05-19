using System.Maui;
using System.Maui.ControlGallery.GTK;
using System.Maui.Controls;
using System.Maui.Platform.GTK;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace System.Maui.ControlGallery.GTK
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
				return true;

			var renderer = Platform.GTK.Platform.CreateRenderer(element);

			if (renderer == null 
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load proper GTK renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}