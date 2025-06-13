using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using Microsoft.UI.Xaml.Media;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public static class NavigationViewExtensions
	{
		public static void UpdateTopNavAreaBackground(this MauiNavigationView navigationView, Paint? paint)
		{
			// Background property is set via {ThemeResource NavigationViewTopPaneBackground} in the Control Template
			// AFAICT you can't modify properties set by ThemeResource at runtime so we have to just update this value directly
			if (paint != null)
				navigationView.TopNavArea?.UpdateBackground(paint, null);
			else if (Application.Current.Resources.TryGetValue("NavigationViewTopPaneBackground", out object value) && value is WBrush brush)
				navigationView.TopNavArea?.UpdateBackground(null, brush);
		}

		public static void UpdateTopNavigationViewItemTextColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();

			if (navigationView.TopNavArea is not null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForeground");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundPressed");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundDisabled");

					navigationView.TopNavArea.Resources.Remove("NavigationViewItemForeground");
					navigationView.TopNavArea.Resources.Remove("NavigationViewItemForegroundPointerOver");
					navigationView.TopNavArea.Resources.Remove("NavigationViewItemForegroundPressed");
				}
				else
				{
					navigationView.TopNavArea.Resources["TopNavigationViewItemForeground"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundPressed"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundDisabled"] = brush;

					//The NavigationViewItemForeground color is applied to the Expand/Collapse Chevron icon
					navigationView.TopNavArea.Resources["NavigationViewItemForeground"] = brush;
					navigationView.TopNavArea.Resources["NavigationViewItemForegroundPointerOver"] = brush;
					navigationView.TopNavArea.Resources["NavigationViewItemForegroundPressed"] = brush;
				}

				navigationView.TopNavArea.RefreshThemeResources();
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.UnselectedTitleColor = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemTextSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();

			if (navigationView.TopNavArea is not null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundSelected");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundSelectedPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundSelectedPressed");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundSelectedDisabled");
				}
				else
				{
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundSelected"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundSelectedPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundSelectedPressed"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundSelectedDisabled"] = brush;
				}

				navigationView.TopNavArea.RefreshThemeResources();
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.SelectedTitleColor = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();

			if (navigationView.TopNavArea is not null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("NavigationViewSelectionIndicatorForeground");
				}
				else
				{
					// Use the TabBarForegroundColor to update the Indicator Brush
					navigationView.TopNavArea.Resources["NavigationViewSelectionIndicatorForeground"] = brush;
				}

				navigationView.TopNavArea.RefreshThemeResources();
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.SelectedForeground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemUnselectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.UnselectedForeground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundUnselectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();
			if (navigationView.TopNavArea is not null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("NavigationViewItemBackground");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundPressed");
				}
				else
				{
					navigationView.TopNavArea.Resources["NavigationViewItemBackground"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundPressed"] = brush;
				}

				navigationView.TopNavArea.RefreshThemeResources();
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.UnselectedBackground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();
			if (navigationView.TopNavArea is not null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelected");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelectedPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelectedPressed");
				}
				else
				{
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelected"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelectedPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelectedPressed"] = brush;
				}

				navigationView.TopNavArea.RefreshThemeResources();
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.SelectedBackground = brush;
				}
			}
		}

		public static void UpdatePaneBackground(this MauiNavigationView navigationView, Paint? paint)
		{
			var paneContentGrid = navigationView.PaneContentGrid;

			if (paneContentGrid is null)
				return;

			var brush = paint?.ToPlatform();

			if (brush is null)
			{
				object? color;
				if (navigationView.IsPaneOpen)
					color = navigationView.Resources["NavigationViewExpandedPaneBackground"];
				else
					color = navigationView.Resources["NavigationViewDefaultPaneBackground"];

				if (color is WBrush colorBrush)
					paneContentGrid.Background = colorBrush;
				else if (color is global::Windows.UI.Color uiColor)
					paneContentGrid.Background = new WSolidColorBrush(uiColor);
			}
			else
				paneContentGrid.Background = brush;
		}

		public static void UpdateFlyoutVerticalScrollMode(this MauiNavigationView navigationView, ScrollMode scrollMode)
		{
			var scrollViewer = navigationView.MenuItemsScrollViewer;
			if (scrollViewer is not null)
			{
				switch (scrollMode)
				{
					case ScrollMode.Disabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Disabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
						break;
					case ScrollMode.Enabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Enabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible;
						break;
					default:
						scrollViewer.VerticalScrollMode = WScrollMode.Auto;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto;
						break;
				}
			}
		}

		public static void UpdateFlyoutBehavior(this MauiNavigationView navigationView, IFlyoutView flyoutView)
		{
			navigationView.UpdatePaneDisplayModeFromFlyoutBehavior(flyoutView.FlyoutBehavior);
		}

		public static void UpdateFlyoutWidth(this MauiNavigationView navigationView, IFlyoutView flyoutView)
		{
			if (flyoutView.FlyoutWidth >= 0)
			{
				navigationView.OpenPaneLength = flyoutView.FlyoutWidth;
			}
			else
			{
				navigationView.OpenPaneLength = 320;
			}

			if (navigationView.PaneContentGrid is not null)
			{
				navigationView.PaneContentGrid.Width = navigationView.OpenPaneLength;
			}

			// At some point this Template Setting is going to show up with a bump to winui
			//handler.PlatformView.OpenPaneLength = handler.PlatformView.TemplateSettings.OpenPaneWidth;
		}

		internal static async Task UpdateBackgroundImageSourceAsync(this MauiNavigationView navigationView, IImageSource? imageSource, IImageSourceServiceProvider? provider, Aspect aspect)
		{
			if (provider is null || imageSource is null)
			{
				return;
			}
			var paneContentGrid = navigationView.PaneContentGrid;
			if (paneContentGrid is null)
			{
				return;
			}

			var service = provider.GetRequiredImageSourceService(imageSource);
			var nativeImageSource = await service.GetImageSourceAsync(imageSource);

			if (nativeImageSource is null)
			{
				paneContentGrid.Background = null;
				return;
			}

			var BackgroundImage = new ImageBrush
			{
				ImageSource = nativeImageSource?.Value,
				Stretch = aspect switch
				{
					Aspect.AspectFit => Stretch.Uniform,
					Aspect.AspectFill => Stretch.UniformToFill,
					Aspect.Fill => Stretch.Fill,
					_ => Stretch.None
				}
			};

			paneContentGrid.Background = BackgroundImage;
		}

		public static async Task UpdateFlyoutIconAsync(this MauiNavigationView navigationView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			var togglePaneButton = navigationView.TogglePaneButton;

			if (togglePaneButton is null)
				return;

			var animatedIcon = togglePaneButton.GetFirstDescendant<AnimatedIcon>();

			if (animatedIcon is null)
				return;

			await animatedIcon.UpdateFlyoutIconAsync(imageSource, provider);
		}

		public static async Task UpdateFlyoutIconAsync(this AnimatedIcon platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (platformView is null)
				return;

			if (provider is not null && imageSource is not null)
			{
				// Custom Icon
				var service = provider.GetRequiredImageSourceService(imageSource);
				var nativeImageSource = await service.GetImageSourceAsync(imageSource);

				platformView.Source = null;

				var fallbackIconSource = new ImageIconSource { ImageSource = nativeImageSource?.Value };

				platformView.Height = platformView.Width = double.NaN;
				platformView.FallbackIconSource = fallbackIconSource;
			}
			else
			{
				// Fallback to the default hamburger icon
				// https://github.com/microsoft/microsoft-ui-xaml/blob/a7183df20367bc0e2b8c825430597a5c1e6871b6/dev/NavigationView/NavigationView_rs1_themeresources.xaml#L389-L391
				var fallbackIconSource = new FontIconSource { Glyph = "&#xE700;" };
				platformView.Height = platformView.Width = 16d;
				platformView.Source = new AnimatedGlobalNavigationButtonVisualSource();
				platformView.FallbackIconSource = fallbackIconSource;
			}
		}
	}
}
