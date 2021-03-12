namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public interface IRegistrarValidationService
	{
		bool Validate(VisualElement element, out string message);
	}
}
