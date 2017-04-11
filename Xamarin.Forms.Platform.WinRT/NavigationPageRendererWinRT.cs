namespace Xamarin.Forms.Platform.WinRT
{
	public partial class NavigationPageRenderer
	{
		void UpdateShowTitle()
		{
			if (Device.Idiom == TargetIdiom.Phone && _parentTabbedPage != null)
				((ITitleProvider)this).ShowTitle = false;
			else
				((ITitleProvider)this).ShowTitle = true;
		}

		static object GetDefaultColor()
		{
			return Windows.UI.Xaml.Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
		}

		void UpdateBackButton()
		{
			bool showBackButton = Element.InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
			_container.ShowBackButton = showBackButton;
		}

		void UpdateToolbarPlacement()
		{
			// Currently we don't support toolbar (CommandBar) placement on Windows 8.1
		}

		void UpdateTitleOnParents()
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
		}
	}
}
