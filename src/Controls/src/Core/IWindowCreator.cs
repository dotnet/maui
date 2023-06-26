namespace Microsoft.Maui.Controls;

public interface IWindowCreator
{
	Window CreateWindow(Application app, IActivationState? activationState);
}
