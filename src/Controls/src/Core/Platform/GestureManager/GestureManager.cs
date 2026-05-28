#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal class GestureManager
	{
		readonly IControlsView _view;
		object? _containerView;
		object? _platformView;
		object? _handler;
		bool _didHaveWindow;

		public bool IsConnected => GesturePlatformManager != null;
		public IGesturePlatformManager? GesturePlatformManager { get; private set; }

		public GestureManager(IControlsView view)
		{
			_view = view;
			view.HandlerChanging += OnHandlerChanging;
			view.HandlerChanged += OnHandlerChanged;
			view.WindowChanged += OnWindowChanged;
			view.PlatformContainerViewChanged += OnPlatformContainerViewChanged;

			SetupGestureManager();
		}

		void OnPlatformContainerViewChanged(object? sender, EventArgs e) =>
			SetupGestureManager();

		void OnWindowChanged(object? sender, EventArgs e) =>
			SetupGestureManager();

		void OnHandlerChanged(object? sender, EventArgs e) =>
			SetupGestureManager();

		void OnHandlerChanging(object? sender, HandlerChangingEventArgs e) =>
			DisconnectGestures();

		void DisconnectGestures()
		{
			GesturePlatformManager?.Dispose();
			GesturePlatformManager = null;
			_handler = null;
			_didHaveWindow = false;
			_containerView = null;
			_platformView = null;
		}

		void SetupGestureManager()
		{
			var handler = _view.Handler;

			if (handler == null ||
				(_didHaveWindow && _view.Window == null))
			{
				DisconnectGestures();
				return;
			}

			if (_containerView != handler.ContainerView ||
				_platformView != handler.PlatformView ||
				_handler != handler)
			{
				DisconnectGestures();
			}

			// The connected Gesture Manager is already setup and watching the correct view
			if (GesturePlatformManager != null)
				return;

			var factory = handler.MauiContext?.Services.GetService<IGesturePlatformManagerFactory>();
			if (factory is null)
			{
				GesturePlatformManager = new GesturePlatformManager(handler);
			}
			else
			{
				GesturePlatformManager = factory.CreateGesturePlatformManager(handler)
					?? throw new InvalidOperationException($"{nameof(IGesturePlatformManagerFactory)}.{nameof(IGesturePlatformManagerFactory.CreateGesturePlatformManager)} cannot return null.");
			}

			_handler = handler;
			_containerView = handler.ContainerView;
			_platformView = handler.PlatformView;
			_didHaveWindow = _view.Window != null;
		}
	}
}
