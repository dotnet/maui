namespace Microsoft.Maui.Controls;

public interface IWindowCreator
{
	Window CreateWindow(IActivationState? activationState);
}
