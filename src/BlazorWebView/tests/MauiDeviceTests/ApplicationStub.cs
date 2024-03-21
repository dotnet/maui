using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ApplicationStub : Application
	{
		Window _window;

		public ApplicationStub() : base()
		{
		}

		public void SetWindow(Window window) => _window = window;

		protected override Window CreateWindow(IActivationState activationState)
		{
			return _window ?? base.CreateWindow(activationState);
		}
	}
}
