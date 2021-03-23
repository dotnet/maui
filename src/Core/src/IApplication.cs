namespace Microsoft.Maui
{
	public interface IApplication
	{
		IWindow CreateWindow(IActivationState activationState);
	}
}