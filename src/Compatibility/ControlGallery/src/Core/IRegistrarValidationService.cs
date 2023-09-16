namespace Microsoft.Maui.Controls.ControlGallery
{
	public interface IRegistrarValidationService
	{
		bool Validate(VisualElement element, out string message);
	}
}
