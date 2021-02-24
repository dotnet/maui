using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmSharp;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Watch
{
	public class ShellSectionNavigationRenderer : IShellItemRenderer
	{
		SimpleViewStack _viewStack;
		IShellItemRenderer _rootPageRenderer;

		public ShellSectionNavigationRenderer(ShellSection item)
		{
			ShellSection = item;
			(ShellSection as IShellSectionController).NavigationRequested += OnNavigationRequested;
			InitializeComponent();
		}

		public ShellSection ShellSection { get; protected set; }

		public BaseShellItem Item => ShellSection;

		public EvasObject NativeView => _viewStack;

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_rootPageRenderer?.Dispose();
				_viewStack?.Unrealize();
				(ShellSection as IShellSectionController).NavigationRequested -= OnNavigationRequested;
			}
		}

		void InitializeComponent()
		{
			_viewStack = new SimpleViewStack(Forms.NativeParent);
			_viewStack.Show();

			_rootPageRenderer = ShellRendererFactory.Default.CreateItemRenderer(ShellSection);
			_viewStack.Push(_rootPageRenderer.NativeView);

			Device.BeginInvokeOnMainThread(() =>
			{
				(_rootPageRenderer.NativeView as Widget)?.SetFocus(true);
			});
		}

		void OnInsertRequest(NavigationRequestedEventArgs request)
		{
			var before = Platform.GetRenderer(request.BeforePage)?.NativeView ?? null;
			if (before == null)
			{
				request.Task = Task.FromException<bool>(new ArgumentException("Can't found page on stack", nameof(request.BeforePage)));
				return;
			}
			var renderer = Platform.GetOrCreateRenderer(request.Page);
			_viewStack.Insert(before, renderer.NativeView);
			request.Task = Task.FromResult(true);
		}

		void OnPushRequest(NavigationRequestedEventArgs request)
		{
			var renderer = Platform.GetOrCreateRenderer(request.Page);
			_viewStack.Push(renderer.NativeView);
			request.Task = Task.FromResult(true);
			Device.BeginInvokeOnMainThread(() =>
			{
				(renderer.NativeView as Widget)?.SetFocus(true);
			});
		}

		void OnPopRequest(NavigationRequestedEventArgs request)
		{
			_viewStack.Pop();
			request.Task = Task.FromResult(true);
		}

		void OnPopToRootRequest(NavigationRequestedEventArgs request)
		{
			_viewStack.PopToRoot();
			request.Task = Task.FromResult(true);
		}

		void OnRemoveRequest(NavigationRequestedEventArgs request)
		{
			var renderer = Platform.GetRenderer(request.Page);
			if (renderer == null)
			{
				request.Task = Task.FromException<bool>(new ArgumentException("Can't found page on stack", nameof(request.Page)));
				return;
			}
			_viewStack.Remove(renderer.NativeView);
			request.Task = Task.FromResult(true);
		}

		void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			switch (e.RequestType)
			{
				case NavigationRequestType.Insert:
					OnInsertRequest(e);
					break;
				case NavigationRequestType.Push:
					OnPushRequest(e);
					break;
				case NavigationRequestType.Pop:
					OnPopRequest(e);
					break;
				case NavigationRequestType.PopToRoot:
					OnPopToRootRequest(e);
					break;
				case NavigationRequestType.Remove:
					OnRemoveRequest(e);
					break;
			}
		}
	}
}
