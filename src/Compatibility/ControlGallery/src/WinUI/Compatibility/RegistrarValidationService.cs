using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.WinUI;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Microsoft.Maui.Controls.ControlGallery.WinUI
{
	[System.Obsolete]
	public class RegistrarValidationService : IRegistrarValidationService
	{
		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
				return true;

#pragma warning disable CS0612 // Type or member is obsolete
			var renderer = Platform.UWP.Platform.CreateRenderer(element);
#pragma warning restore CS0612 // Type or member is obsolete

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