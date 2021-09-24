using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ApplicationStub : IApplication
	{
		readonly List<IWindow> _windows = new List<IWindow>();

		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public IWindow CreateWindow(IActivationState state)
		{
			_windows.Add(new WindowStub());

			return _windows[0];
		}

		public void ThemeChanged() { }
	}
}