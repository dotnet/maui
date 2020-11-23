using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation.Metadata;
using Windows.Media.Devices.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms.Internals;

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
		List<Page> FormsNavigationStack;

		ObservableCollection<ShellContent> ShellContentMenuItems;
		public ShellSectionRenderer()
		{
			Xamarin.Forms.Shell.VerifyShellUWPFlagEnabled(nameof(ShellSectionRenderer));
			MenuItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ShellSectionMenuItemTemplate"];
			IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
			IsSettingsVisible = false;
			AlwaysShowHeader = false;
			PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;
			ItemInvoked += OnMenuItemInvoked;
			ShellContentMenuItems = new ObservableCollection<ShellContent>();
			MenuItemsSource = ShellContentMenuItems;
			AutoSuggestBox = new Windows.UI.Xaml.Controls.AutoSuggestBox() { Width = 300 };
			AutoSuggestBox.TextChanged += OnSearchBoxTextChanged;
			AutoSuggestBox.QuerySubmitted += OnSearchBoxQuerySubmitted;
			AutoSuggestBox.SuggestionChosen += OnSearchBoxSuggestionChosen;

			Frame = new Windows.UI.Xaml.Controls.Frame();
			Content = Frame;
			this.SizeChanged += OnShellSectionRendererSizeChanged;
			Resources["NavigationViewTopPaneBackground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultBackgroundColor);
			Resources["TopNavigationViewItemForeground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
			Resources["TopNavigationViewItemForegroundSelected"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
			Resources["NavigationViewSelectionIndicatorForeground"] = new Windows.UI.Xaml.Media.SolidColorBrush(ShellRenderer.DefaultForegroundColor);
			FormsNavigationStack = new List<Page>();
		}

		void OnShellSectionRendererSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			if (Page != null)
				Page.ContainerArea = new Rectangle(0, 0, e.NewSize.Width, e.NewSize.Height);
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var shellContent = args.InvokedItemContainer?.DataContext as ShellContent;
			var shellItem = ShellSection.RealParent as ShellItem;

			if (shellContent == null)
				return;

			if (shellItem.RealParent is Shell shell &&
				shellItem.RealParent is IShellController controller)
			{
				var result = controller.ProposeNavigation(ShellNavigationSource.ShellContentChanged, shellItem, ShellSection, shellContent, null, true);
				if (result)
				{
					ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, shellContent);
				}
			}
		}

		internal void NavigateToShellSection(ShellSection section)
		{
			_ = section ?? throw new ArgumentNullException(nameof(section));

			if (section != ShellSection)
			{
				if (ShellSection != null)
				{
					ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
					ShellSectionController.ItemsCollectionChanged -= OnShellSectionRendererCollectionChanged;
					ShellSection = null;
				}

				ShellSection = section;
				ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
				ShellSectionController.ItemsCollectionChanged += OnShellSectionRendererCollectionChanged;
			}

			if (section.CurrentItem != SelectedItem)
			{
				IsPaneVisible = ShellSectionController.GetItems().Count > 1;
			}

			SyncMenuItems();

			var shellContent = ShellSection.CurrentItem;
			Page nextPage = (ShellSection as IShellSectionController)
					.PresentedPage ?? ((IShellContentController)shellContent)?.GetOrCreateContent();

			OnShellSectionChanged();
			NavigateToContent(new NavigationRequestedEventArgs(nextPage, true), ShellSection);
		}

		void SyncMenuItems()
		{
			var newItems = ShellSectionController.GetItems();
			foreach (var item in newItems)
			{
				if (!ShellContentMenuItems.Contains(item))
					ShellContentMenuItems.Add(item);
			}

			SelectedItem = ShellSection?.CurrentItem;

			for (var i = ShellContentMenuItems.Count - 1; i >= 0; i--)
			{
				var item = ShellContentMenuItems[i];
				if (!newItems.Contains(item))
					ShellContentMenuItems.RemoveAt(i);
			}
		}

		void OnShellSectionRendererCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			SyncMenuItems();
		}

		void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				NavigateToShellSection(ShellSection);
			}
		}

		public virtual void NavigateToContent(NavigationRequestedEventArgs args, ShellSection shellSection)
		{
			Page nextPage = null;
			ShellContent shellContent = shellSection.CurrentItem;
			if (args.RequestType == NavigationRequestType.PopToRoot)
			{
				nextPage = (shellContent as IShellContentController).GetOrCreateContent();
			}
			else
			{
				nextPage = (ShellSection as IShellSectionController)
					.PresentedPage ?? ((IShellContentController)shellContent)?.GetOrCreateContent();
			}

			if (CurrentContent != null && Page != null)
			{
				Page.PropertyChanged -= OnPagePropertyChanged;
				((IShellContentController)CurrentContent).RecyclePage(Page);
			}

			CurrentContent = shellContent;
			if (nextPage != null)
			{
				Page = nextPage;
				Page.PropertyChanged += OnPagePropertyChanged;
				switch (args.RequestType)
				{
					case NavigationRequestType.Insert:
						OnInsertRequested(args);
						break;
					case NavigationRequestType.Pop:
						OnPopRequested(args);
						break;
					case NavigationRequestType.Unknown:
						break;
					case NavigationRequestType.Push:
						OnPushRequested(args);
						break;
					case NavigationRequestType.PopToRoot:
						OnPopToRootRequested(args);
						break;
					case NavigationRequestType.Remove:
						OnRemoveRequested(args);
						break;
				}

				UpdateSearchHandler(Shell.GetSearchHandler(Page));
				var wrapper = (ShellPageWrapper)(Frame.Content);
				if (wrapper.Page == null)
				{
					wrapper.Page = Page;
				}

				wrapper.LoadPage();
				FormsNavigationStack = ShellSection.Stack.ToList();
			}
		}

		protected virtual void OnPopRequested(NavigationRequestedEventArgs e)
		{
			Frame.GoBack(GetTransitionInfo(e));
		}

		protected virtual void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			while (Frame.BackStackDepth > 2)
			{
				Frame.BackStack.RemoveAt(1);
			}
			Frame.GoBack(GetTransitionInfo(e));
		}

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			Frame.Navigate(typeof(ShellPageWrapper), GetTransitionInfo(e));
		}

		protected virtual void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			var pageIndex = ShellSection.Stack.ToList().IndexOf(e.Page);
			var transition = GetTransitionInfo(e);
			if (pageIndex == Frame.BackStack.Count - 1)
				Frame.Navigate(typeof(ShellPageWrapper), transition);
			else
				Frame.BackStack.Insert(pageIndex, new PageStackEntry(typeof(ShellPageWrapper), null, transition));
		}

		protected virtual void OnRemoveRequested(NavigationRequestedEventArgs e)
		{
			var pageIndex = FormsNavigationStack.IndexOf(e.Page);
			if (pageIndex == Frame.BackStack.Count - 1)
				Frame.GoBack(GetTransitionInfo(e));
			else
				Frame.BackStack.RemoveAt(pageIndex);
		}

		protected virtual void OnShellSectionChanged()
		{
			Frame.Navigate(typeof(ShellPageWrapper), GetTransitionInfo(ShellNavigationSource.ShellSectionChanged));
		}

		protected virtual NavigationTransitionInfo GetTransitionInfo(NavigationRequestedEventArgs e)
		{
			return GetTransitionInfo((ShellNavigationSource)e.RequestType);
		}

		protected virtual NavigationTransitionInfo GetTransitionInfo(ShellNavigationSource navSource)
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

		void OnSearchBoxTextChanged(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason != Windows.UI.Xaml.Controls.AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				_currentSearchHandler.Query = sender.Text;
		}

		void OnSearchBoxSuggestionChosen(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			((ISearchHandlerController)_currentSearchHandler).ItemSelected(args.SelectedItem);
		}

		void OnSearchBoxQuerySubmitted(Windows.UI.Xaml.Controls.AutoSuggestBox sender, Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
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
