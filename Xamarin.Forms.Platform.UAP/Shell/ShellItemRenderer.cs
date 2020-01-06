using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Internals;
using Windows.UI.Xaml;

using UwpGrid = Windows.UI.Xaml.Controls.Grid;
using UwpColumnDefinition = Windows.UI.Xaml.Controls.ColumnDefinition;
using UwpRowDefinition = Windows.UI.Xaml.Controls.RowDefinition;
using UwpGridLength = Windows.UI.Xaml.GridLength;
using UwpGridUnitType = Windows.UI.Xaml.GridUnitType;
using UwpDataTemplate = Windows.UI.Xaml.DataTemplate;
using UwpThickness = Windows.UI.Xaml.Thickness;
using UwpStyle = Windows.UI.Xaml.Style;
using Windows.UI.Xaml.Media;
using UwpApplication = Windows.UI.Xaml.Application;

namespace Xamarin.Forms.Platform.UWP
{
	// Responsible for rendering the content title, as well as the bottom bar list of shell sections
	public class ShellItemRenderer : UwpGrid, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		ShellSectionRenderer SectionRenderer { get; }
		TextBlock _Title;
		Border _BottomBarArea;
		UwpGrid _BottomBar;
		UwpGrid _HeaderArea;
		ItemsControl _Toolbar;

		internal ShellItem ShellItem { get; set; }

		internal ShellRenderer ShellContext { get; set; }

		IShellItemController ShellItemController => ShellItem;

		public ShellItemRenderer(ShellRenderer shellContext)
		{
			Xamarin.Forms.Shell.VerifyShellUWPFlagEnabled(nameof(ShellItemRenderer));
			_ = shellContext ?? throw new ArgumentNullException(nameof(shellContext));

			ShellContext = shellContext;
			RowDefinitions.Add(new UwpRowDefinition() { Height = new UwpGridLength(1, UwpGridUnitType.Auto) });
			RowDefinitions.Add(new UwpRowDefinition() { Height = new UwpGridLength(1, UwpGridUnitType.Star) });
			RowDefinitions.Add(new UwpRowDefinition() { Height = new UwpGridLength(1, UwpGridUnitType.Auto) });

			_Title = new TextBlock()
			{
				Style = Resources["SubtitleTextBlockStyle"] as UwpStyle,
				VerticalAlignment = VerticalAlignment.Center,
				TextTrimming = TextTrimming.CharacterEllipsis,
				TextWrapping = TextWrapping.NoWrap
			};
			_HeaderArea = new UwpGrid() { Height = 40, Padding = new UwpThickness(10, 0, 10, 0) };
			_HeaderArea.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = new UwpGridLength(1, UwpGridUnitType.Star) });
			_HeaderArea.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = new UwpGridLength(1, UwpGridUnitType.Auto) });
			_HeaderArea.Children.Add(_Title);
			Children.Add(_HeaderArea);

			_Toolbar = new ItemsControl()
			{
				ItemTemplate = UwpApplication.Current.Resources["ShellToolbarItemTemplate"] as UwpDataTemplate,
				ItemsPanel = UwpApplication.Current.Resources["ShellToolbarItemsPanelTemplate"] as ItemsPanelTemplate,
			};
			SetColumn(_Toolbar, 1);
			_HeaderArea.Children.Add(_Toolbar);

			SectionRenderer = shellContext.CreateShellSectionRenderer();
			SetRow(SectionRenderer, 1);

			Children.Add(SectionRenderer);

			_BottomBar = new UwpGrid() { HorizontalAlignment = HorizontalAlignment.Center };
			_BottomBarArea = new Border() { Child = _BottomBar };
			SetRow(_BottomBarArea, 2);
			Children.Add(_BottomBarArea);
		}

		internal void SetShellContext(ShellRenderer context)
		{
			if (ShellContext != null)
			{
				((IShellController)ShellContext.Shell).RemoveAppearanceObserver(this);
				((IShellController)ShellContext.Shell).RemoveFlyoutBehaviorObserver(this);
			}
			ShellContext = context;
			if (ShellContext != null)
			{
				((IShellController)ShellContext.Shell).AddFlyoutBehaviorObserver(this);
				((IShellController)ShellContext.Shell).AddAppearanceObserver(this, ShellContext.Shell);
				UpdateHeaderInsets();
			}
		}

		internal void NavigateToShellItem(ShellItem newItem, bool animate)
		{
			UnhookEvents(ShellItem);
			ShellItem = newItem;

			if (newItem.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {newItem}. Title: {newItem.Title}. Route: {newItem.Route}.");

			ShellSection = newItem.CurrentItem;

			if (ShellSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {ShellSection}. Title: {ShellSection.Title}. Route: {ShellSection.Route}.");

			HookEvents(newItem);
		}

		internal void UpdateHeaderInsets()
		{
			double inset = 10;
			if (ShellContext.IsPaneToggleButtonVisible)
				inset += 45;
			if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Controls.NavigationView", "IsBackButtonVisible"))
			{
				if (ShellContext.IsBackButtonVisible != Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed)
					inset += 45;
			}
			_HeaderArea.Padding = new UwpThickness(inset, 0, 0, 0);
		}

		void UpdateBottomBar()
		{
			_BottomBar.Children.Clear();
			_BottomBar.ColumnDefinitions.Clear();
			var items = ShellItemController?.GetItems();

			if (items?.Count > 1)
			{
				for (int i = 0; i < items.Count; i++)
				{
					var section = items[i];
					var btn = new AppBarButton()
					{
						Label = section.Title,
						Width = double.NaN,
						MinWidth = 68,
						MaxWidth = 200
					};

					switch (section.Icon)
					{
						case FileImageSource fileImageSource:
							btn.Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///" + fileImageSource.File) };
							break;

						case FontImageSource fontImageSource:

							var icon = new FontIcon()
							{
								Glyph = fontImageSource.Glyph,
								FontFamily = new FontFamily(fontImageSource.FontFamily),
								FontSize = fontImageSource.Size,
							};

							if (!fontImageSource.Color.IsDefault)
							{
								icon.Foreground = fontImageSource.Color.ToBrush();
							}

							btn.Icon = icon;
							break;
					}

					btn.Click += (s, e) => OnShellSectionClicked(section);
					_BottomBar.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = new UwpGridLength(1, UwpGridUnitType.Star) });
					SetColumn(btn, i);
					_BottomBar.Children.Add(btn);
				}
			}
		}

		void OnShellSectionClicked(ShellSection shellSection)
		{
			if (shellSection != null)
				((IShellItemController)ShellItem).ProposeSection(shellSection);
		}

		protected virtual bool ChangeSection(ShellSection shellSection)
		{
			return ((IShellItemController)ShellItem).ProposeSection(shellSection);
		}

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance) => UpdateAppearance(appearance);
		void UpdateAppearance(ShellAppearance appearance)
		{
			var tabBarBackgroundColor = ShellRenderer.DefaultBackgroundColor;
			var tabBarForegroundColor = ShellRenderer.DefaultForegroundColor;
			var titleColor = ShellRenderer.DefaultTitleColor;
			if (appearance != null)
			{
				var a = (IShellAppearanceElement)appearance;
				tabBarBackgroundColor = a.EffectiveTabBarBackgroundColor.ToWindowsColor();
				tabBarForegroundColor = a.EffectiveTabBarForegroundColor.ToWindowsColor();
				if (!appearance.TitleColor.IsDefault)
					titleColor = appearance.TitleColor.ToWindowsColor();
			}
			_BottomBarArea.Background = _HeaderArea.Background =
				new SolidColorBrush(tabBarBackgroundColor);
			_Title.Foreground = new SolidColorBrush(titleColor);
			var tabbarForeground = new SolidColorBrush(tabBarForegroundColor);
			foreach (var button in _BottomBar.Children.OfType<AppBarButton>())
				button.Foreground = tabbarForeground;
			if (SectionRenderer is IAppearanceObserver iao)
				iao.OnAppearanceChanged(appearance);
		}

		#endregion

		ShellSection _shellSection;

		protected ShellSection ShellSection
		{
			get => _shellSection;
			set
			{
				if (_shellSection == value)
					return;
				var oldValue = _shellSection;
				if (_shellSection != null)
				{
					((IShellSectionController)_shellSection).RemoveDisplayedPageObserver(this);
				}

				_shellSection = value;
				if (value != null)
				{
					OnShellSectionChanged(oldValue, value);
					((IShellSectionController)ShellSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);
				}
				UpdateBottomBar();
			}
		}

		void HookEvents(ShellItem shellItem)
		{
			shellItem.PropertyChanged += OnShellItemPropertyChanged;
			ShellItemController.ItemsCollectionChanged += OnShellItemsChanged;
			foreach (var shellSection in ShellItemController.GetItems())
			{
				HookChildEvents(shellSection);
			}
		}

		protected virtual void UnhookEvents(ShellItem shellItem)
		{
			if (shellItem != null)
			{
				foreach (var shellSection in ShellItemController.GetItems())
				{
					UnhookChildEvents(shellSection);
				}
				ShellItemController.ItemsCollectionChanged -= OnShellItemsChanged;
				ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
				ShellSection = null;
				ShellItem = null;
			}
		}

		void HookChildEvents(ShellSection shellSection)
		{
			((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;
		}

		protected virtual void UnhookChildEvents(ShellSection shellSection)
		{
			((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
				ShellSection = ShellItem.CurrentItem;
		}

		protected virtual void OnShellItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellSection shellSection in e.OldItems)
					UnhookChildEvents(shellSection);
			}

			if (e.NewItems != null)
			{
				foreach (ShellSection shellSection in e.NewItems)
					HookChildEvents(shellSection);
			}
			UpdateBottomBar();
		}

		protected virtual void OnShellSectionChanged(ShellSection oldSection, ShellSection newSection)
		{
			SwitchSection(ShellNavigationSource.ShellSectionChanged, newSection, null, oldSection != null);
		}

		void SwitchSection(ShellNavigationSource source, ShellSection section, Page page, bool animate = true)
		{
			if (section == null)
				throw new InvalidOperationException($"Content not found for active {ShellItem} - {ShellItem.Title}.");

			if (section.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {section} - {section.Title}.");

			SectionRenderer.NavigateToShellSection(source, section, animate);
		}

		Page DisplayedPage { get; set; }

		void UpdateDisplayedPage(Page page)
		{
			if (DisplayedPage != null)
			{
				DisplayedPage.PropertyChanged -= OnPagePropertyChanged;
			}
			DisplayedPage = page;
			if (DisplayedPage != null)
			{
				DisplayedPage.PropertyChanged += OnPagePropertyChanged;
			}
			UpdateBottomBarVisibility();
			UpdatePageTitle();
			UpdateToolbar();
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
			{
				UpdateBottomBarVisibility();
			}
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				UpdatePageTitle();
			}
			else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
			{
				UpdateNavBarVisibility();
			}
		}

		void UpdateNavBarVisibility()
		{
			if (DisplayedPage == null || Shell.GetNavBarIsVisible(DisplayedPage))
			{
				_HeaderArea.Visibility = Visibility.Visible;
				Shell.SetFlyoutBehavior(Shell.Current, Xamarin.Forms.FlyoutBehavior.Flyout);
			}
			else
			{
				_HeaderArea.Visibility = Visibility.Collapsed;
				Shell.SetFlyoutBehavior(Shell.Current, Xamarin.Forms.FlyoutBehavior.Disabled);
			}
		}

		void UpdatePageTitle()
		{
			_Title.Text = DisplayedPage?.Title ?? ShellSection?.Title ?? "";
		}

		void UpdateBottomBarVisibility()
		{
			_BottomBar.Visibility = DisplayedPage == null || Shell.GetTabBarIsVisible(DisplayedPage) ? Visibility.Visible : Visibility.Collapsed;
		}

		void UpdateToolbar()
		{
			_Toolbar.ItemsSource = DisplayedPage?.ToolbarItems;
		}

		void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			SwitchSection((ShellNavigationSource)e.RequestType, (ShellSection)sender, e.Page, e.Animated);
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			UpdateHeaderInsets();
		}
	}
}
