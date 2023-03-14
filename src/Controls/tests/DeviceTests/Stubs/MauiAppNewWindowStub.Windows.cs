using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class MauiAppNewWindowStub : IApplication
	{
		readonly IWindow _window;
		Window Window => _window as Window;

		MauiWinUIWindow _plaformWindow;
		IElementHandler _handler;

		public MauiAppNewWindowStub(IWindow window)
		{
			_window = window;
			Window.HandlerChanged += OnWindowHandlerChanged;
		}

		void OnWindowHandlerChanged(object sender, EventArgs e)
		{
			if (_window.Handler?.PlatformView is not MauiWinUIWindow platformWindow)
				return;

			if (_plaformWindow is null)
			{
				_plaformWindow = platformWindow;
				_window.Created();
				_plaformWindow.Activated += OnActivated;
				_plaformWindow.Closed += OnClosed;
			}
		}

		public IReadOnlyList<IWindow> Windows => new List<IWindow>() { _window };

		public IElementHandler Handler
		{
			get => _handler;
			set
			{
				_handler = value;

				if (value is not null)
					_window.Created();
			}
		}

		void OnClosed(object sender, UI.Xaml.WindowEventArgs args)
		{
			if (_plaformWindow is not null)
			{
				_plaformWindow.Activated -= OnActivated;
				_plaformWindow.Closed -= OnClosed;
				_plaformWindow = null;
				_window.Destroying();
			}
		}

		void OnActivated(object sender, UI.Xaml.WindowActivatedEventArgs args)
		{
			_window.Activated();
		}

		public IElement Parent => null;

		public AppTheme UserAppTheme { get; set; }

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
