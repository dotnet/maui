namespace System.Maui.Controls
{
	public interface IRegistrarValidationService
	{
		bool Validate(VisualElement element, out string message);
	}
}
