using System;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class MauiAppNewWindowStub : IApplication
	{
		readonly IWindow _window;

		public MauiAppNewWindowStub(IWindow window)
		{
			_window = window;
		}

		public IReadOnlyList<IWindow> Windows => new List<IWindow>() { _window };

		public IElementHandler Handler { get; set; }

		public IElement Parent => null;

		public void CloseWindow(IWindow window)
		{
			Handler?.Invoke(nameof(IApplication.CloseWindow), window);
		}

		public IWindow CreateWindow(IActivationState activationState)
		{
			return _window;
		}

		public void OpenWindow(IWindow window)
		{
			throw new NotImplementedException();
		}

		public void ThemeChanged()
		{
			throw new NotImplementedException();
		}
	}
}
