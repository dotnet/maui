using System;
using System.ComponentModel;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Xamarin.Forms.Platform.UWP
{
	// Renders the actual page area where the contents gets rendered, as well as set of optional top-bar menu items and search box.
	[Windows.UI.Xaml.Data.Bindable]
	public class ShellSectionRenderer : Microsoft.UI.Xaml.Controls.NavigationView, IAppearanceObserver
	{
		Windows.UI.Xaml.Controls.Frame Frame { get; }
		Page Page;
		ShellContent CurrentContent;
		ShellSection ShellSection;
		IShellSectionController ShellSectionController => ShellSection;

		public ShellSectionRenderer()
		{
			Xamarin.Forms.Shell.VerifyShellUWPFlagEnabled(nameof(ShellSectionRenderer));
			MenuItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ShellSectionMenuItemTemplate"];
			IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
			IsSettingsVisible = false;
			AlwaysShowHeader = false;
			PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;
			ItemInvoked += MenuItemInvoked;

			AutoSuggestBox = new Windows.UI.Xaml.Controls.AutoSuggestBox() { Width = 300 };
			AutoSuggestBox.TextChanged += SearchBox_TextChanged;
			AutoSuggestBox.QuerySubmitted += SearchBox_QuerySubmitted;
			AutoSuggestBox.SuggestionChosen += SearchBox_SuggestionChosen;

			Frame = new Windows.UI.Xaml.Controls.Frame();
			Content = Frame;
			this.SizeChanged += ShellSectionRenderer_SizeChanged;
			Resources["NavigationViewTopPaneBackground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultBackgroundColor);
			Resources["TopNavigationViewItemForeground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
			Resources["TopNavigationViewItemForegroundSelected"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
			Resources["NavigationViewSelectionIndicatorForeground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
		}

		void ShellSectionRenderer_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			Page.ContainerArea = new Rectangle(0, 0, e.NewSize.Width, e.NewSize.Height);
		}

		void MenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var shellContent = args.InvokedItemContainer?.DataContext as ShellContent;
			var shellItem = ShellSection.RealParent as ShellItem;

			if(shellItem.RealParent is IShellController controller)
			{
				var result = controller.ProposeNavigation(ShellNavigationSource.Pop, shellItem, ShellSection, shellContent, null, true);
				if (result)
				{
					ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, shellContent);
				}
			}
		}

		internal void NavigateToShellSection(ShellNavigationSource source, ShellSection section, bool animate = true)
		{
			_ = section ?? throw new ArgumentNullException(nameof(section));

			if (ShellSection != null)
			{
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionRendererCollectionChanged;
				ShellSection = null;
				MenuItemsSource = null;
			}

			ShellSection = section;
			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
			SelectedItem = null;
			IsPaneVisible = ShellSectionController.GetItems().Count > 1;
			MenuItemsSource = ShellSectionController.GetItems();
			ShellSectionController.ItemsCollectionChanged += OnShellSectionRendererCollectionChanged;
			SelectedItem = section.CurrentItem;
			NavigateToContent(source, section.CurrentItem, animate);
		}

		void OnShellSectionRendererCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// This shouldn't be necessary, but MenuItemsSource doesn't appear to be listening for INCC
			// Revisit once using WinUI instead.
			MenuItemsSource = null;
			MenuItemsSource = ShellSectionController?.GetItems();
		}

		void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				NavigateToContent(ShellNavigationSource.ShellSectionChanged, ShellSection.CurrentItem);
			}
		}

		internal void NavigateToContent(ShellNavigationSource source, ShellContent shellContent, bool animate = true)
		{
			if (CurrentContent != null && Page != null)
			{
				Page.PropertyChanged -= OnPagePropertyChanged;
				((IShellContentController)CurrentContent).RecyclePage(Page);
			}
			CurrentContent = shellContent;
			if (shellContent != null)
			{
				Page = ((IShellContentController)shellContent).GetOrCreateContent();
				Page.PropertyChanged += OnPagePropertyChanged;

				Frame.Navigate((ContentPage)Page, GetTransitionInfo(source));
				UpdateSearchHandler(Shell.GetSearchHandler(Page));
			}
		}

		NavigationTransitionInfo GetTransitionInfo(ShellNavigationSource navSource)
		{
			switch (navSource)
			{
				case ShellNavigationSource.Push:
					return new SlideNavigationTransitionInfo(); // { Effect = SlideNavigationTransitionEffect.FromRight }; Requires SDK 17763
				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
					return new SlideNavigationTransitionInfo(); // { Effect = SlideNavigationTransitionEffect.FromLeft }; Requires SDK 17763
				case ShellNavigationSource.ShellSectionChanged:
					return null;
			}
			return null;
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
			{
				UpdateSearchHandler(Shell.GetSearchHandler(Page));
			}
		}

		#region Search

		SearchHandler _currentSearchHandler;

		void UpdateSearchHandler(SearchHandler searchHandler)
		{
			if (_currentSearchHandler != null)
			{
				_currentSearchHandler.PropertyChanged -= SearchHandler_PropertyChanged;
			}
			_currentSearchHandler = searchHandler;
			if (AutoSuggestBox == null)
				return;
			if (searchHandler != null)
			{
				searchHandler.PropertyChanged += SearchHandler_PropertyChanged;
				AutoSuggestBox.Visibility = searchHandler.SearchBoxVisibility == SearchBoxVisibility.Hidden ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
				AutoSuggestBox.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
				AutoSuggestBox.PlaceholderText = searchHandler.Placeholder;
				AutoSuggestBox.IsEnabled = searchHandler.IsSearchEnabled;
				AutoSuggestBox.ItemsSource = _currentSearchHandler.ItemsSource;
				ToggleSearchBoxVisibility();
				UpdateQueryIcon();
				IsPaneVisible = true;
			}
			else
			{
				IsPaneVisible = ShellSectionController.GetItems().Count > 1;
				AutoSuggestBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		void SearchHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (AutoSuggestBox == null)
				return;
			if (e.PropertyName == SearchHandler.PlaceholderProperty.PropertyName)
			{
				AutoSuggestBox.PlaceholderText = _currentSearchHandler.Placeholder;
			}
			else if (e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
			{
				AutoSuggestBox.IsEnabled = _currentSearchHandler.IsSearchEnabled;
			}
			else if (e.PropertyName == SearchHandler.ItemsSourceProperty.PropertyName)
			{
				AutoSuggestBox.ItemsSource = _currentSearchHandler.ItemsSource;
			}
			else if (e.PropertyName == SearchHandler.QueryProperty.PropertyName)
			{
				AutoSuggestBox.Text = _currentSearchHandler.Query;
			}
			else if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName)
			{
				ToggleSearchBoxVisibility();
			}
			else if (e.PropertyName == SearchHandler.QueryIconProperty.PropertyName)
			{
				UpdateQueryIcon();
			}
		}

		void ToggleSearchBoxVisibility()
		{
			AutoSuggestBox.Visibility = _currentSearchHandler == null || _currentSearchHandler.SearchBoxVisibility == SearchBoxVisibility.Hidden ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
			if (_currentSearchHandler != null && _currentSearchHandler.SearchBoxVisibility != SearchBoxVisibility.Hidden)
			{
				if (_currentSearchHandler.SearchBoxVisibility == SearchBoxVisibility.Expanded)
				{
					// TODO: Expand search
				}
				else
				{
					// TODO: Collapse search
				}
			}
		}

		void UpdateQueryIcon()
		{
			if (_currentSearchHandler != null)
			{
				if (_currentSearchHandler.QueryIcon is FileImageSource fis)
					AutoSuggestBox.QueryIcon = new BitmapIcon() { UriSource = new Uri("ms-appx:///" + fis.File) };
				else
					AutoSuggestBox.QueryIcon = new SymbolIcon(Symbol.Find);
			}
		}

		void SearchBox_TextChanged(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason != Windows.UI.Xaml.Controls.AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				_currentSearchHandler.Query = sender.Text;
		}

		void SearchBox_SuggestionChosen(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			((ISearchHandlerController)_currentSearchHandler).ItemSelected(args.SelectedItem);
		}

		void SearchBox_QuerySubmitted(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			((ISearchHandlerController)_currentSearchHandler).QueryConfirmed();
		}

		#endregion Search

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance) => UpdateAppearance(appearance);

		void UpdateAppearance(ShellAppearance appearance)
		{
			var tabBarBackgroundColor = ShellRenderer.DefaultBackgroundColor;
			var tabBarForegroundColor = ShellRenderer.DefaultForegroundColor;
			if (appearance != null)
			{
				var a = (IShellAppearanceElement)appearance;
				tabBarBackgroundColor = a.EffectiveTabBarBackgroundColor.ToWindowsColor();
				tabBarForegroundColor = a.EffectiveTabBarForegroundColor.ToWindowsColor();
			}

			UpdateBrushColor("NavigationViewTopPaneBackground", tabBarBackgroundColor);
			UpdateBrushColor("TopNavigationViewItemForeground", tabBarForegroundColor);
			UpdateBrushColor("TopNavigationViewItemForegroundSelected", tabBarForegroundColor);
			UpdateBrushColor("NavigationViewSelectionIndicatorForeground", tabBarForegroundColor);
		}

		void UpdateBrushColor(string resourceKey, Windows.UI.Color color)
		{
			if (Resources[resourceKey] is Windows.UI.Xaml.Media.SolidColorBrush sb)
				sb.Color = color;
		}

		#endregion
	}
}
