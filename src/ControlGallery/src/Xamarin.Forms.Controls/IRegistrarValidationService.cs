namespace Xamarin.Forms.Controls
{
	public interface IRegistrarValidationService
	{
		bool Validate(VisualElement element, out string message);
	}
}
