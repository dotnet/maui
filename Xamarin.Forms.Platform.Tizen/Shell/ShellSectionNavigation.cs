using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellSectionNavigation : Native.Box, IAppearanceObserver
	{
		ShellNavBar _navBar = null;
		ShellSection _section = null;
		Page _currentPage = null;
		Page _rootPage = null;

		LinkedList<EvasObject> _navigationStack = new LinkedList<EvasObject>();
		Dictionary<Page, EvasObject> _pageToNative = new Dictionary<Page, EvasObject>();
		Dictionary<EvasObject, Page> _nativeToPage = new Dictionary<EvasObject, Page>();

		bool _disposed = false;
		bool _navBarIsVisible = true;
		const int _defaultNavBarHeight = 110;
		int _navBarHeight = _defaultNavBarHeight;

		public ShellSectionNavigation(IFlyoutController flyoutController, ShellSection section) : base(Forms.NativeParent)
		{
			_section = section;
			_section.PropertyChanged += OnSectionPropertyChanged;
			_rootPage = ((IShellContentController)_section.CurrentItem).GetOrCreateContent();

			_navBar = new ShellNavBar(flyoutController);
			_navBar.Show();

			var renderer = CreateShellSection(section);
			renderer.Control.Show();
			_navigationStack.AddLast(renderer.Control);
			_pageToNative[_rootPage] = renderer.Control;
			_nativeToPage[renderer.Control] = _rootPage;

			IShellSectionController controller = _section as IShellSectionController;
			controller.NavigationRequested += OnNavigationRequested;
			controller.AddDisplayedPageObserver(this, UpdateDisplayedPage);

			PackEnd(_navBar);
			PackEnd(renderer.Control);
			LayoutUpdated += OnLayoutUpdated;
			((IShellController)_section.Parent.Parent).AddAppearanceObserver(this, _section);
		}

		~ShellSectionNavigation()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
				UpdateLayout();
			}
		}

		public EvasObject CurrentNative
		{
			get
			{
				return _navigationStack.Last();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_section != null)
				{
					IShellSectionController controller = _section as IShellSectionController;
					controller.NavigationRequested -= OnNavigationRequested;
					controller.RemoveDisplayedPageObserver(this);

					_section.PropertyChanged -= OnSectionPropertyChanged;
					_section = null;
				}
				if (_currentPage != null)
				{
					_currentPage.PropertyChanged -= OnPagePropertyChanged;
				}
				Unrealize();
			}
			_disposed = true;
		}

		protected virtual ShellSectionRenderer CreateShellSection(ShellSection section)
		{
			return new ShellSectionRenderer(section);
		}

		void UpdateDisplayedPage(Page page)
		{
			if (_currentPage != null)
			{
				_currentPage.PropertyChanged -= OnPagePropertyChanged;
			}
			_currentPage = page;
			_currentPage.PropertyChanged += OnPagePropertyChanged;

			_navBar.Title = page.Title;
			_navBar.SearchHandler = Shell.GetSearchHandler(page);
			_navBar.TitleView = Shell.GetTitleView(page);
			_navBar.CurrentPage = page;
			NavBarIsVisible = Shell.GetNavBarIsVisible(page);

			if (((IShellContentController)_section.CurrentItem).GetOrCreateContent() != page)
				_navBar.HasBackButton = true;
			else
				_navBar.HasBackButton = false;
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				return;

			if (!appearance.TitleColor.IsDefault)
			{
				_navBar.TitleColor = appearance.TitleColor.ToNative();
			}
			else
			{
				_navBar.TitleColor = ShellRenderer.DefaultTitleColor.ToNative();
			}

			if (!appearance.BackgroundColor.IsDefault)
			{
				_navBar.BackgroundColor = appearance.BackgroundColor.ToNative();
			}
			else
			{
				_navBar.BackgroundColor = ShellRenderer.DefaultBackgroundColor.ToNative();
			}

			if (!appearance.ForegroundColor.IsDefault)
			{
				_navBar.ForegroundColor = appearance.ForegroundColor.ToNative();
			}
			else
			{
				_navBar.ForegroundColor = ShellRenderer.DefaultForegroundColor.ToNative();
			}
		}

		void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentItem")
			{
				var native = _pageToNative[_rootPage];
				_pageToNative.Remove(_rootPage);
				_rootPage = ((IShellContentController)_section.CurrentItem).GetOrCreateContent();
				_pageToNative[_rootPage] = native;
				_nativeToPage[native] = _rootPage;
			}
		}

		EvasObject GetOrCreatePage(Page page)
		{
			Native.Page native = Platform.GetOrCreateRenderer(page).NativeView as Native.Page;
			_pageToNative[page] = native;
			_nativeToPage[native] = page;
			native.BackgroundColor = (page.BackgroundColor != Xamarin.Forms.Color.Default ? page.BackgroundColor.ToNative() : ElmSharp.Color.White);
			PackEnd(native);
			return native;
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
		}

		void RemoveRequest(object sender, Internals.NavigationRequestedEventArgs e)
		{
			if (_pageToNative.ContainsKey(e.Page))
			{
				EvasObject del = _pageToNative[e.Page];
				EvasObject top = CurrentNative;

				if (del == CurrentNative)
				{
					PopRequest(sender, e);
				}
				else
				{
					RemovePage(del);
					UpdateTaskCompletionSource(e, true);
				}
			}
		}

		internal void PopRequest(object sender, Internals.NavigationRequestedEventArgs e)
		{
			CurrentNative?.Hide();
			RemovePage(CurrentNative);

			UpdateLayout();
			UpdateTaskCompletionSource(e, true);
		}

		void RemovePage(EvasObject del)
		{
			_pageToNative.Remove(_nativeToPage[del]);
			_nativeToPage.Remove(del);
			_navigationStack.Remove(del);

			del.Hide();
			del.Unrealize();
		}

		void PopToRootRequest(object sender, Internals.NavigationRequestedEventArgs e)
		{
			CurrentNative?.Hide();

			var root = _pageToNative[_rootPage];
			foreach (var pair in _pageToNative)
			{
				if (pair.Value != root)
					pair.Value.Unrealize();
			}
			_pageToNative.Clear();
			_nativeToPage.Clear();
			_navigationStack.Clear();

			_navigationStack.AddLast(root);
			_pageToNative[_rootPage] = root;
			_nativeToPage[root] = _rootPage;

			UpdateLayout();
			UpdateTaskCompletionSource(e, true);
		}

		void PushRequest(object sender, Internals.NavigationRequestedEventArgs e)
		{
			CurrentNative?.Hide();

			EvasObject native = GetOrCreatePage(e.Page);
			_navigationStack.AddLast(native);

			UpdateLayout();
			UpdateTaskCompletionSource(e, true);
		}

		void UpdateTaskCompletionSource(Internals.NavigationRequestedEventArgs e, bool result)
		{
			var tcs = new TaskCompletionSource<bool>();
			e.Task = tcs.Task;
			tcs.SetResult(result);
		}

		void InsertRequest(object sender, Internals.NavigationRequestedEventArgs e)
		{
			if (_pageToNative.ContainsKey(e.BeforePage))
			{
				var before = _navigationStack.Find(_pageToNative[e.BeforePage]);
				EvasObject after = GetOrCreatePage(e.Page);
				_navigationStack.AddBefore(before, after);

				UpdateTaskCompletionSource(e, true);
			}
			else
			{
				PushRequest(sender, e);
			}
		}

		void UpdateLayout()
		{
			OnLayoutUpdated(this, new LayoutEventArgs() { Geometry = Geometry });
		}

		void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			if (_navBarIsVisible)
			{
				_navBarHeight = _defaultNavBarHeight;
				_navBar.Show();
				_navBar.Move(e.Geometry.X, e.Geometry.Y);
				_navBar.Resize(e.Geometry.Width, _navBarHeight);
			}
			else
			{
				_navBarHeight = 0;
				_navBar.Hide();
			}
			CurrentNative.Show();
			CurrentNative.Move(e.Geometry.X, e.Geometry.Y + _navBarHeight);
			CurrentNative.Resize(e.Geometry.Width, e.Geometry.Height - _navBarHeight);
		}
	}
}
