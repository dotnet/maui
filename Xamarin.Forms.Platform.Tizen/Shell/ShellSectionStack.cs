using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellSectionStack : EBox, IAppearanceObserver, IDisposable
	{
		ShellNavBar _navBar = null;
		Page _currentPage = null;
		SimpleViewStack _viewStack = null;
		ShellSectionRenderer _shellSectionRenderer;

		bool _disposed = false;
		bool _navBarIsVisible = true;

		public ShellSectionStack(ShellSection section) : base(Forms.NativeParent)
		{
			ShellSection = section;
			InitializeComponent();
		}

		public bool NavBarIsVisible
		{
			get
			{
				return _navBarIsVisible;
			}
			set
			{
				_navBarIsVisible = value;
				OnLayout();
			}
		}

		ShellSection ShellSection { get; }

		~ShellSectionStack()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				(Shell.Current as IShellController).RemoveAppearanceObserver(this);
				(Shell.Current as IShellController).RemoveFlyoutBehaviorObserver(_navBar);
				if (ShellSection != null)
				{
					IShellSectionController controller = ShellSection;
					controller.NavigationRequested -= OnNavigationRequested;
					controller.RemoveDisplayedPageObserver(this);
				}
				if (_currentPage != null)
				{
					_currentPage.PropertyChanged -= OnPagePropertyChanged;
				}
				if (_navBar != null)
				{
					_navBar.Dispose();
					_navBar = null;
				}
				Unrealize();
			}
			_disposed = true;
		}

		protected virtual ShellSectionRenderer CreateShellSectionRenderer(ShellSection section)
		{
			return new ShellSectionRenderer(section);
		}

		void InitializeComponent()
		{
			SetAlignment(-1, -1);
			SetWeight(1, 1);
			SetLayoutCallback(OnLayout);

			_viewStack = new SimpleViewStack(Forms.NativeParent);
			if (Device.Idiom == TargetIdiom.Phone)
			{
				_viewStack.BackgroundColor = ElmSharp.Color.White;
			}
			_viewStack.Show();
			PackEnd(_viewStack);

			_navBar = new ShellNavBar();
			_navBar.Show();
			PackEnd(_navBar);

			IShellSectionController controller = ShellSection;
			controller.NavigationRequested += OnNavigationRequested;
			controller.AddDisplayedPageObserver(this, UpdateDisplayedPage);
			((IShellController)Shell.Current).AddAppearanceObserver(this, ShellSection);
			((IShellController)Shell.Current).AddFlyoutBehaviorObserver(_navBar);

			_shellSectionRenderer = CreateShellSectionRenderer(ShellSection);
			_shellSectionRenderer.NativeView.Show();
			_viewStack.Push(_shellSectionRenderer.NativeView);

			Device.BeginInvokeOnMainThread(() =>
			{
				(_shellSectionRenderer.NativeView as Widget)?.SetFocus(true);
			});
		}

		void UpdateDisplayedPage(Page page)
		{
			// this callback is raised when DisplayPage was updated and it is raised ahead of push NavigationRequesed event
			if (_currentPage != null)
			{
				_currentPage.PropertyChanged -= OnPagePropertyChanged;
			}
			if (page == null)
				return;

			_currentPage = page;
			_currentPage.PropertyChanged += OnPagePropertyChanged;
			NavBarIsVisible = Shell.GetNavBarIsVisible(page);
			_navBar.SetPage(page);
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (_navBar == null)
				return;

			var titleColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarTitleColor ?? Xamarin.Forms.Color.Default;
			var backgroundColor = appearance?.BackgroundColor ?? Xamarin.Forms.Color.Default;
			var foregroundColor = appearance?.ForegroundColor ?? Xamarin.Forms.Color.Default;

			_navBar.TitleColor = titleColor.IsDefault ? ShellRenderer.DefaultTitleColor.ToNative() : titleColor.ToNative();
			_navBar.BackgroundColor = backgroundColor.IsDefault ? ShellRenderer.DefaultBackgroundColor.ToNative() : backgroundColor.ToNative();
			_navBar.ForegroundColor = foregroundColor.IsDefault ? ShellRenderer.DefaultForegroundColor.ToNative() : foregroundColor.ToNative();
		}


		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				_navBar.Title = (sender as Page)?.Title;
			}
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
			{
				_navBar.SearchHandler = Shell.GetSearchHandler(sender as Page);
			}
			else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
			{
				NavBarIsVisible = Shell.GetNavBarIsVisible(sender as Page);
			}
			else if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
			{
				_navBar.TitleView = Shell.GetTitleView(sender as Page);
			}
		}

		void OnNavigationRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			if (e.RequestType == Internals.NavigationRequestType.Push)
			{
				PushRequest(sender, e);
			}
			else if (e.RequestType == Internals.NavigationRequestType.Insert)
			{
				InsertRequest(sender, e);
			}
			else if (e.RequestType == Internals.NavigationRequestType.Pop)
			{
				PopRequest(sender, e);
			}
			else if (e.RequestType == Internals.NavigationRequestType.PopToRoot)
			{
				PopToRootRequest(sender, e);
			}
			else if (e.RequestType == Internals.NavigationRequestType.Remove)
			{
				RemoveRequest(sender, e);
			}
			UpdateHasBackButton();
		}

		void RemoveRequest(object sender, Internals.NavigationRequestedEventArgs request)
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

		void PopRequest(object sender, Internals.NavigationRequestedEventArgs request)
		{
			_viewStack.Pop();
			request.Task = Task.FromResult(true);
		}

		void PopToRootRequest(object sender, Internals.NavigationRequestedEventArgs request)
		{
			_viewStack.PopToRoot();
			request.Task = Task.FromResult(true);
		}

		void PushRequest(object sender, Internals.NavigationRequestedEventArgs request)
		{
			var renderer = Platform.GetOrCreateRenderer(request.Page);
			_viewStack.Push(renderer.NativeView);
			request.Task = Task.FromResult(true);
			Device.BeginInvokeOnMainThread(() =>
			{
				(renderer.NativeView as Widget)?.SetFocus(true);
			});
		}

		void InsertRequest(object sender, Internals.NavigationRequestedEventArgs request)
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

		void UpdateHasBackButton()
		{
			if (_viewStack.Stack.Count > 1)
				_navBar.HasBackButton = true;
			else
				_navBar.HasBackButton = false;
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			var bound = Geometry;
			int navBarHeight;
			if (_navBarIsVisible)
			{
				var navBound = bound;
				navBarHeight = Forms.ConvertToScaledPixel(_navBar.GetDefaultHeight());
				navBound.Height = navBarHeight;

				_navBar.Show();
				_navBar.Geometry = navBound;
				_navBar.RaiseTop();
			}
			else
			{
				navBarHeight = 0;
				_navBar.Hide();
			}

			bound.Y += navBarHeight;
			bound.Height -= navBarHeight;
			_viewStack.Geometry = bound;
		}
	}
}
