using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmSharp;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen.Watch
{
	class SimpleViewStack : Box
	{
		EvasObject _lastTop;

		public SimpleViewStack(EvasObject parent) : base(parent)
		{
			InternalStack = new List<EvasObject>();
			SetLayoutCallback(OnLayout);
		}

		List<EvasObject> InternalStack { get; set; }

		public IReadOnlyList<EvasObject> Stack => InternalStack;

		public void Push(EvasObject view)
		{
			InternalStack.Add(view);
			PackEnd(view);
			UpdateTopView();
		}

		public void Pop()
		{
			if (_lastTop != null)
			{
				var tobeRemoved = _lastTop;
				InternalStack.Remove(tobeRemoved);
				UnPack(tobeRemoved);
				UpdateTopView();

				// if Pop was called by removed page,
				// Unrealize cause deletation of NativeCallback, it could be a cause of crash
				Device.BeginInvokeOnMainThread(() =>
				{
					tobeRemoved.Unrealize();
				});
			}
		}

		public void PopToRoot()
		{
			while (InternalStack.Count > 1)
			{
				Pop();
			}
		}

		public void Insert(EvasObject before, EvasObject view)
		{
			view.Hide();
			var idx = InternalStack.IndexOf(before);
			InternalStack.Insert(idx, view);
			PackEnd(view);
			UpdateTopView();
		}

		public void Remove(EvasObject view)
		{
			InternalStack.Remove(view);
			UnPack(view);
			UpdateTopView();
			Device.BeginInvokeOnMainThread(() =>
			{
				view?.Unrealize();
			});
		}

		void UpdateTopView()
		{
			if (_lastTop != InternalStack.LastOrDefault())
			{
				_lastTop?.Hide();
				_lastTop = InternalStack.LastOrDefault();
				_lastTop.Show();
				(_lastTop as Widget)?.SetFocus(true);
			}
		}

		void OnLayout()
		{
			foreach (var view in Stack)
			{
				view.Geometry = Geometry;
			}
		}

	}

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
