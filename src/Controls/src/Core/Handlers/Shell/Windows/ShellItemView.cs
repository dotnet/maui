using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;

using UwpGrid = Microsoft.UI.Xaml.Controls.Grid;
using UwpColumnDefinition = Microsoft.UI.Xaml.Controls.ColumnDefinition;
using UwpRowDefinition = Microsoft.UI.Xaml.Controls.RowDefinition;
using UwpGridLength = Microsoft.UI.Xaml.GridLength;
using UwpGridUnitType = Microsoft.UI.Xaml.GridUnitType;
using UwpDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using UwpThickness = Microsoft.UI.Xaml.Thickness;
using UwpStyle = Microsoft.UI.Xaml.Style;
using Microsoft.UI.Xaml.Media;
using UwpApplication = Microsoft.UI.Xaml.Application;
using UwpSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using Microsoft.Maui.Controls.Platform;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	// Responsible for rendering the content title, as well as the bottom bar list of shell sections
	public class ShellItemView : UwpGrid, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		ShellSectionView SectionRenderer { get; }
		TextBlock _Title;
		Border _BottomBarArea;
		UwpGrid _BottomBar;
		UwpGrid _HeaderArea;
		ItemsControl _Toolbar;

		internal ShellItem ShellItem { get; set; }

		internal ShellView ShellContext { get; set; }

		IShellItemController ShellItemController => ShellItem;
		IShellController ShellController => ShellContext?.Shell;

		public ShellItemView(ShellView shellContext)
		{
			Microsoft.Maui.Controls.Shell.VerifyShellUWPFlagEnabled(nameof(ShellItemView));
			_ = shellContext ?? throw new ArgumentNullException(nameof(shellContext));

			ShellContext = shellContext;
			RowDefinitions.Add(new UwpRowDefinition() { Height = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Auto) });
			RowDefinitions.Add(new UwpRowDefinition() { Height = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Star) });
			RowDefinitions.Add(new UwpRowDefinition() { Height = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Auto) });

			_Title = new TextBlock()
			{
				Style = Resources["SubtitleTextBlockStyle"] as UwpStyle,
				VerticalAlignment = VerticalAlignment.Center,
				TextTrimming = TextTrimming.CharacterEllipsis,
				TextWrapping = TextWrapping.NoWrap
			};
			_HeaderArea = new UwpGrid() { Height = 40, Padding = WinUIHelpers.CreateThickness(10, 0, 10, 0) };
			_HeaderArea.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Star) });
			_HeaderArea.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Auto) });
			_HeaderArea.Children.Add(_Title);
			Children.Add(_HeaderArea);

			_Toolbar = new ItemsControl()
			{
				ItemTemplate = UwpApplication.Current.Resources["ShellToolbarItemTemplate"] as UwpDataTemplate,
				ItemsPanel = UwpApplication.Current.Resources["ShellToolbarItemsPanelTemplate"] as ItemsPanelTemplate,
			};
			SetColumn(_Toolbar, 1);
			_HeaderArea.Children.Add(_Toolbar);

			SectionRenderer = shellContext.CreateShellSectionView();
			SetRow(SectionRenderer, 1);

			Children.Add(SectionRenderer);

			_BottomBar = new UwpGrid() { HorizontalAlignment = HorizontalAlignment.Center };
			_BottomBarArea = new Border() { Child = _BottomBar };
			SetRow(_BottomBarArea, 2);
			Children.Add(_BottomBarArea);
		}

		internal void SetShellContext(ShellView context)
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

			if (newItem?.CurrentItem?.CurrentItem == null)
				return;

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

			if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Controls.NavigationView", "IsBackButtonVisible"))
			{
				if (ShellContext.IsBackButtonVisible != Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed &&
					ShellContext.IsBackEnabled)
					inset += 45;
			}

			_HeaderArea.Padding = WinUIHelpers.CreateThickness(inset, 0, 0, 0);
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

							if (!fontImageSource.Color.IsDefault())
							{
								icon.Foreground = Maui.ColorExtensions.ToNative(fontImageSource.Color);
							}

							btn.Icon = icon;
							break;
					}

					btn.Click += (s, e) => OnShellSectionClicked(section);
					_BottomBar.ColumnDefinitions.Add(new UwpColumnDefinition() { Width = WinUIHelpers.CreateGridLength(1, UwpGridUnitType.Star) });
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
			var tabBarBackgroundColor = ShellView.DefaultBackgroundColor;
			var tabBarForegroundColor = ShellView.DefaultForegroundColor;
			var titleColor = ShellView.DefaultTitleColor;
			if (appearance != null)
			{
				var a = (IShellAppearanceElement)appearance;
				tabBarBackgroundColor = a.EffectiveTabBarBackgroundColor.ToWindowsColor();
				tabBarForegroundColor = a.EffectiveTabBarForegroundColor.ToWindowsColor();
				if (!appearance.TitleColor.IsDefault())
					titleColor = appearance.TitleColor.ToWindowsColor();
			}
			_BottomBarArea.Background = _HeaderArea.Background =
				new UwpSolidColorBrush(tabBarBackgroundColor);
			_Title.Foreground = new UwpSolidColorBrush(titleColor);
			var tabbarForeground = new UwpSolidColorBrush(tabBarForegroundColor);
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
			ShellController.StructureChanged += OnShellStructureChanged;
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

				ShellController.StructureChanged -= OnShellStructureChanged;
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
			if (newSection == null)
				throw new InvalidOperationException($"Content not found for active {ShellItem} - {ShellItem.Title}.");

			if (newSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {newSection} - {newSection.Title}.");

			SectionRenderer.NavigateToShellSection(newSection);
		}

		void OnShellStructureChanged(object sender, EventArgs e)
		{
			UpdateBottomBarVisibility();
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
				_HeaderArea.Visibility = WVisibility.Visible;
				Shell.SetFlyoutBehavior(Shell.Current, Microsoft.Maui.Controls.FlyoutBehavior.Flyout);
			}
			else
			{
				_HeaderArea.Visibility = WVisibility.Collapsed;
				Shell.SetFlyoutBehavior(Shell.Current, Microsoft.Maui.Controls.FlyoutBehavior.Disabled);
			}
		}

		void UpdatePageTitle()
		{
			_Title.Text = DisplayedPage?.Title ?? ShellSection?.Title ?? "";
		}

		void UpdateBottomBarVisibility()
		{
			bool isVisible = ShellItemController?.ShowTabs ?? false;
			_BottomBar.Visibility = isVisible ? WVisibility.Visible : WVisibility.Collapsed;
		}

		void UpdateToolbar()
		{
			_Toolbar.ItemsSource = DisplayedPage?.ToolbarItems;
		}

		void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			SectionRenderer.NavigateToContent(e, (ShellSection)sender);
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			UpdateHeaderInsets();
		}
	}
}
