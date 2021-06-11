using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		List<IWindow> _windows = new List<IWindow>();
		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		IWindow IApplication.CreateWindow(IActivationState activationState)
		{
			IWindow window = CreateWindow(activationState);

			AddWindow(window);

			return window;
		}

		void AddWindow(IWindow window)
		{
			_windows.Add(window);

			if (window is Element windowElement)
			{
				windowElement.Parent = this;
				InternalChildren.Add(windowElement);
				OnChildAdded(windowElement);
			}

			if (window is NavigableElement ne)
				ne.NavigationProxy.Inner = NavigationProxy;
		}

		protected virtual IWindow CreateWindow(IActivationState activationState)
		{
			if (MainPage is Page page)
				return new Window(page);

			throw new NotImplementedException($"Either set {nameof(MainPage)} or override {nameof(Application.CreateWindow)}.");
		}
	}
}