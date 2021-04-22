using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		List<IWindow> _windows = new List<IWindow>();
		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		IWindow IApplication.CreateWindow(IActivationState activationState)
		{
			IWindow window = CreateWindow(activationState);
			_windows.Add(window);

			if (window.View is NavigableElement ne)
				ne.NavigationProxy.Inner = NavigationProxy;

			if (window is Element elementWindow)
			{
				elementWindow.Parent = this;
				InternalChildren.Add(elementWindow);
				OnChildAdded(elementWindow);
			}

			return window;
		}

		protected virtual IWindow CreateWindow(IActivationState activationState)
		{
			throw new NotImplementedException();
		}
	}
}