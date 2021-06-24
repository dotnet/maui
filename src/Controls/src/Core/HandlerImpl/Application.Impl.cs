using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		readonly List<Window> _windows = new();

		IReadOnlyList<IWindow> IApplication.Windows => Windows;

		public IReadOnlyList<Window> Windows => _windows.AsReadOnly();

		IWindow IApplication.CreateWindow(IActivationState activationState)
		{
			var window = CreateWindow(activationState);

			if (!_windows.Contains(window))
				AddWindow(window);

			return window;
		}

		void AddWindow(Window window)
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

		protected virtual Window CreateWindow(IActivationState activationState)
		{
			if (Windows.Count > 0)
				return Windows[0];

			throw new NotImplementedException($"Either set {nameof(MainPage)} or override {nameof(Application.CreateWindow)}.");
		}
	}
}