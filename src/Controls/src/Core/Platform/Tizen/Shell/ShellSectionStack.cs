using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellSectionStack : EBox, IAppearanceObserver, IDisposable
	{
		ShellNavBar _navBar = null;
		Page _currentPage = null;
		SimpleViewStack _viewStack = null;
		IShellSectionRenderer _shellSectionView;

		bool _disposed = false;
		bool _navBarIsVisible = true;

		public ShellSectionStack(ShellSection section, IMauiContext context) : base(context?.Context?.BaseLayout)
		{
			ShellSection = section;
			MauiContext = context;
			InitializeComponent();
		}

		protected IMauiContext MauiContext { get; private set; }

		protected EvasObject NativeParent
		{
			get => MauiContext?.Context?.BaseLayout;
		}

		public virtual bool NavBarIsVisible
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

		protected virtual IShellSectionRenderer CreateShellSectionView(ShellSection section)
		{
			return new ShellSectionView(section, MauiContext);
		}

		void InitializeComponent()
		{
			SetAlignment(-1, -1);
			SetWeight(1, 1);
			SetLayoutCallback(OnLayout);

			_viewStack = new SimpleViewStack(NativeParent);
			if (Device.Idiom == TargetIdiom.Phone)
			{
				_viewStack.BackgroundColor = ElmSharp.Color.White;
			}
			_viewStack.Show();
			PackEnd(_viewStack);

			_navBar = new ShellNavBar(MauiContext);
			_navBar.Show();
			PackEnd(_navBar);

			IShellSectionController controller = ShellSection;
			controller.NavigationRequested += OnNavigationRequested;
			controller.AddDisplayedPageObserver(this, UpdateDisplayedPage);
			((IShellController)Shell.Current).AddAppearanceObserver(this, ShellSection);
			((IShellController)Shell.Current).AddFlyoutBehaviorObserver(_navBar);

			_shellSectionView = CreateShellSectionView(ShellSection);
			_shellSectionView.NativeView.Show();
			_viewStack.Push(_shellSectionView.NativeView);

			Device.BeginInvokeOnMainThread(() =>
			{
				(_shellSectionView.NativeView as Widget)?.SetFocus(true);
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

			var titleColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarTitleColor;
			var backgroundColor = appearance?.BackgroundColor;
			var foregroundColor = appearance?.ForegroundColor;

			_navBar.TitleColor = titleColor.IsDefault() ? ShellView.DefaultTitleColor : titleColor.ToNativeEFL();
			_navBar.BackgroundColor = backgroundColor.IsDefault() ? ShellView.DefaultBackgroundColor : backgroundColor.ToNativeEFL();
			_navBar.ForegroundColor = foregroundColor.IsDefault() ? ShellView.DefaultForegroundColor : foregroundColor.ToNativeEFL();
		}


		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				_navBar.Title = (sender as Page)?.Title;
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
			var nativePage = request.Page.ToNative(MauiContext);
			if (nativePage == null)
			{
				request.Task = Task.FromException<bool>(new ArgumentException("Can't found page on stack", nameof(request.Page)));
				return;
			}
			_viewStack.Remove(nativePage);
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
			var nativePage = request.Page.ToNative(MauiContext);
			_viewStack.Push(nativePage);
			request.Task = Task.FromResult(true);
			Device.BeginInvokeOnMainThread(() =>
			{
				(nativePage as Widget)?.SetFocus(true);
			});
		}

		void InsertRequest(object sender, Internals.NavigationRequestedEventArgs request)
		{
			var before = request.BeforePage.ToNative(MauiContext);
			if (before == null)
			{
				request.Task = Task.FromException<bool>(new ArgumentException("Can't found page on stack", nameof(request.BeforePage)));
				return;
			}
			var page = request.Page.ToNative(MauiContext);
			_viewStack.Insert(before, page);
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
			if (NavBarIsVisible)
			{
				var navBound = bound;
				navBarHeight = DPExtensions.ConvertToScaledPixel(_navBar.GetDefaultHeight());
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
