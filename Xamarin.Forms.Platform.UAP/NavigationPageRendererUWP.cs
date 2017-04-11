using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Platform.UWP
{
	public partial class NavigationPageRenderer : IToolBarForegroundBinder
	{
		SystemNavigationManager _navManager;

		public void BindForegroundColor(AppBar appBar)
		{
			SetAppBarForegroundBinding(appBar);
		}

		public void BindForegroundColor(AppBarButton button)
		{
			SetAppBarForegroundBinding(button);
		}

		void SetAppBarForegroundBinding(FrameworkElement element)
		{
			element.SetBinding(Control.ForegroundProperty,
				new Windows.UI.Xaml.Data.Binding { Path = new PropertyPath("TitleBrush"), Source = _container, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}

		void UpdateToolbarPlacement()
		{
			if (_container == null)
			{
				return;
			}

			_container.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateShowTitle()
		{
			((ITitleProvider)this).ShowTitle = _parentTabbedPage == null && _parentMasterDetailPage == null;
		}

		static object GetDefaultColor()
		{
			return Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
		}

		void UpdateBackButton()
		{
			bool showBackButton = Element.InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
			_container.ShowBackButton = showBackButton;

			if (_navManager != null)
			{
				_navManager.AppViewBackButtonVisibility = showBackButton ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
			}
		}

		async void UpdateTitleOnParents()
		{
			if (Element == null)
				return;

			ITitleProvider render = null;
			if (_parentTabbedPage != null)
			{
				render = Platform.GetRenderer(_parentTabbedPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentTabbedPage.CurrentPage == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (_parentMasterDetailPage != null)
			{
				render = Platform.GetRenderer(_parentMasterDetailPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentMasterDetailPage.Detail == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (render != null && render.ShowTitle)
			{
				render.Title = _currentPage.Title;
				render.BarBackgroundBrush = GetBarBackgroundBrush();
				render.BarForegroundBrush = GetBarForegroundBrush();
			}

			if (_showTitle || (render != null && render.ShowTitle))
			{
				var platform = Element.Platform as Platform;
				if (platform != null)
				{
					await platform.UpdateToolbarItems();
				}
			}
		}
	}
}
