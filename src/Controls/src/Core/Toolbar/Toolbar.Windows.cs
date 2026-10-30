using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		readonly ImageConverter _imageConverter = new ImageConverter();
		static readonly ConditionalWeakTable<AppBarButton, NativeIconBadgeState> s_nativeIconBadgeStates = new();

		NavigationRootManager? NavigationRootManager =>
			MauiContext?.GetNavigationRootManager();

		partial void OnHandlerChanging(IElementHandler oldHandler, IElementHandler newHandler)
		{
			if (newHandler == null)
			{
				foreach (var item in ToolbarItems)
					item.PropertyChanged -= OnToolbarItemPropertyChanged;
			}
		}

		internal void UpdateMenu()
		{
			if (Handler.PlatformView is not MauiToolbar wh)
				return;

			var commandBar = wh.CommandBar;
			if (commandBar == null)
			{
				return;
			}

			commandBar.PrimaryCommands.Clear();
			commandBar.SecondaryCommands.Clear();

			List<ToolbarItem> toolbarItems = new List<ToolbarItem>(ToolbarItems ?? Array.Empty<ToolbarItem>());

			foreach (ToolbarItem item in toolbarItems)
			{
				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");
				UpdateToolbarItemIcon(button, item, commandBar);

				button.Command = new MenuItemCommand(item);
				button.DataContext = item;
				button.SetValue(NativeAutomationProperties.AutomationIdProperty, item.AutomationId);
				button.SetAutomationPropertiesName(item);
				button.SetAutomationPropertiesAccessibilityView(item);
				button.SetAutomationPropertiesHelpText(item);
				button.SetAutomationPropertiesLabeledBy(item, null);

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
				{
					button.UpdateTextColor(BarTextColor);
					commandBar.PrimaryCommands.Add(button);
				}
				else
				{
					commandBar.SecondaryCommands.Add(button);
				}

				item.PropertyChanged -= OnToolbarItemPropertyChanged;
				item.PropertyChanged += OnToolbarItemPropertyChanged;
			}

			SetDefaultLabelPosition(commandBar, toolbarItems);
		}

		internal void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (Handler?.PlatformView is not MauiToolbar wh)
				return;

			var commandBar = wh.CommandBar;
			if (commandBar == null)
			{
				return;
			}

			if (e.PropertyName == nameof(ToolbarItem.BadgeText) ||
				e.PropertyName == nameof(ToolbarItem.BadgeColor) ||
				e.PropertyName == nameof(ToolbarItem.BadgeTextColor))
			{
				if (sender is ToolbarItem badgeItem &&
					FindToolbarItemButton(commandBar, badgeItem) is AppBarButton badgeButton)
				{
					UpdateToolbarItemIcon(badgeButton, badgeItem, commandBar);
				}

				return;
			}

			if (e.PropertyName == nameof(ToolbarItem.IconImageSource))
			{
				if (sender is ToolbarItem item &&
					FindToolbarItemButton(commandBar, item) is AppBarButton button)
				{
					UpdateToolbarItemIcon(button, item, commandBar);
				}

				var toolbarItems = new List<ToolbarItem>(ToolbarItems ?? Array.Empty<ToolbarItem>());
				SetDefaultLabelPosition(commandBar, toolbarItems);
				return;
			}

			if (e.PropertyName == nameof(ToolbarItem.Text))
			{
				var toolbarItems = new List<ToolbarItem>(ToolbarItems ?? Array.Empty<ToolbarItem>());
				SetDefaultLabelPosition(commandBar, toolbarItems);
			}
		}

		private void UpdateToolbarItemIcon(AppBarButton button, ToolbarItem item, CommandBar commandBar)
		{
			button.ClearValue(AppBarButton.IconProperty);
			button.Content = null;

			var source = item.IconImageSource;
			if (source.IsNullOrEmpty())
			{
				UpdateToolbarItemBadge(button, item);
				return;
			}

			if (source is FontImageSource fontSource &&
				fontSource.FontFamily is string fontFamily &&
				MauiContext.Services.GetRequiredService<IFontRegistrar>().GetFont(fontFamily) is not null)
			{
				// Render registered app fonts like Image while retaining the native icon slot for the label.
				var icon = new ImageIcon();
				icon.SetBinding(ImageIcon.SourceProperty, "Value");
				icon.DataContext = _imageConverter.Convert(source, typeof(object), MauiContext, string.Empty);
				button.Icon = icon;
			}
			else if (source is FontImageSource ||
				(commandBar.IsDynamicOverflowEnabled && item.Order == ToolbarItemOrder.Secondary))
			{
				button.Icon = source.ToIconSource(MauiContext)?.CreateIconElement();
			}
			else
			{
				var image = new WImage();
				image.SetBinding(WImage.SourceProperty, "Value");
				image.DataContext = _imageConverter.Convert(source, typeof(object), MauiContext, string.Empty);
				button.Content = image;
			}

			UpdateToolbarItemBadge(button, item);
		}

		static void UpdateToolbarItemBadge(AppBarButton button, ToolbarItem item)
		{
			Microsoft.UI.Xaml.UIElement? content = button.Content as Microsoft.UI.Xaml.UIElement;
			if (content is null && item.IconImageSource.IsNullOrEmpty())
				content = new TextBlock { Text = item.Text ?? string.Empty };

			if (content is null)
			{
				UpdateNativeIconBadge(button, item);
				return;
			}

			RemoveNativeIconBadge(button);

			if (item.BadgeText is null)
				return;

			var grid = new WGrid();
#pragma warning disable RS0030 // Standalone WinUI Grid, not a MauiPanel
			grid.Children.Add(content);

			var badge = new InfoBadge
			{
				HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
				VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
				Margin = new Microsoft.UI.Xaml.Thickness(0, -4, -4, 0),
			};
			UpdateInfoBadge(badge, item);
			grid.Children.Add(badge);
#pragma warning restore RS0030

			button.Content = grid;
		}

		static void UpdateNativeIconBadge(AppBarButton button, ToolbarItem item)
		{
			if (item.BadgeText is null)
			{
				RemoveNativeIconBadge(button);
				return;
			}

			var state = s_nativeIconBadgeStates.GetValue(button, static button =>
			{
				button.Loaded += OnNativeIconButtonLoaded;
				return new NativeIconBadgeState();
			});

			UpdateInfoBadge(state.Badge, item);
			AttachNativeIconBadge(button, state);
		}

		static void OnNativeIconButtonLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is AppBarButton button &&
				s_nativeIconBadgeStates.TryGetValue(button, out var state))
			{
				AttachNativeIconBadge(button, state);
			}
		}

		static void AttachNativeIconBadge(AppBarButton button, NativeIconBadgeState state)
		{
			button.ApplyTemplate();
			var host = FindBadgeHost(button);
			if (host is null || ReferenceEquals(host, state.Host))
				return;

			if (state.Host is not null)
				state.Host.Children.Remove(state.Badge);

			state.Host = host;
#pragma warning disable RS0030 // AppBarButton template Grid, not a MauiPanel
			host.Children.Add(state.Badge);
#pragma warning restore RS0030
		}

		static WGrid? FindBadgeHost(DependencyObject parent)
		{
			var childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int index = 0; index < childCount; index++)
			{
				var child = VisualTreeHelper.GetChild(parent, index);
				if (child is WGrid grid)
					return grid;

				if (FindBadgeHost(child) is WGrid descendantGrid)
					return descendantGrid;
			}

			return null;
		}

		static void RemoveNativeIconBadge(AppBarButton button)
		{
			if (s_nativeIconBadgeStates.TryGetValue(button, out var state) &&
				state.Host is not null)
			{
				state.Host.Children.Remove(state.Badge);
				state.Host = null;
			}
		}

		private static AppBarButton? FindToolbarItemButton(CommandBar commandBar, ToolbarItem item)
		{
			foreach (var command in commandBar.PrimaryCommands)
			{
				if (command is AppBarButton button && ReferenceEquals(button.DataContext, item))
					return button;
			}

			foreach (var command in commandBar.SecondaryCommands)
			{
				if (command is AppBarButton button && ReferenceEquals(button.DataContext, item))
					return button;
			}

			return null;
		}

		/// <summary>
		/// Updates an InfoBadge control to reflect the current badge state of a ToolbarItem.
		/// On Windows, numeric badge text displays as a count; non-numeric text and empty string display as a dot indicator.
		/// Setting BadgeText to null hides the badge; setting to empty string shows a dot.
		/// </summary>
		static void UpdateInfoBadge(InfoBadge badge, ToolbarItem item)
		{
			var badgeText = item.BadgeText;

			if (badgeText is null)
			{
				badge.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
				return;
			}

			badge.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

			if (badgeText.Length == 0)
				badge.Value = -1; // Empty string shows as dot indicator
			else if (int.TryParse(badgeText, out var value) && value >= 0)
				badge.Value = value;
			else
				badge.Value = -1; // Non-numeric text also shows as dot indicator

			if (item.BadgeColor is not null)
			{
				badge.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
					item.BadgeColor.ToWindowsColor());
			}
			else
				badge.ClearValue(InfoBadge.BackgroundProperty);

			if (item.BadgeTextColor is not null)
			{
				badge.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
					item.BadgeTextColor.ToWindowsColor());
			}
			else
				badge.ClearValue(InfoBadge.ForegroundProperty);
		}

		sealed class NativeIconBadgeState
		{
			public InfoBadge Badge { get; } = new()
			{
				HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
				VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
				Margin = new Microsoft.UI.Xaml.Thickness(0, -4, -4, 0),
			};

			public WGrid? Host { get; set; }
		}

		private static void SetDefaultLabelPosition(CommandBar commandBar, IList<ToolbarItem> toolbarItems)
		{
			int itemsWithTextCount = 0;
			int itemsWithIconCount = 0;

			foreach (ToolbarItem item in toolbarItems)
			{
				if (!string.IsNullOrEmpty(item.Text))
				{
					itemsWithTextCount++;
				}
				if (item.IconImageSource != null)
				{
					itemsWithIconCount++;
				}
			}

			bool allItemsHaveIcons = toolbarItems.Count == itemsWithIconCount;

			// All items have icons, none have text
			if (allItemsHaveIcons && itemsWithTextCount == 0)
			{
				commandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed;
			}
			else
			{
				commandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
			}
		}

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarTextColor((IToolbarHandler)arg1, arg2);

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarBackground((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonTitle((IToolbarHandler)arg1, arg2);

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarItems((IToolbarHandler)arg1, arg2);

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapIconColor((IToolbarHandler)arg1, arg2);

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleView((IToolbarHandler)arg1, arg2);

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleIcon((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonVisible((IToolbarHandler)arg1, arg2);

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapIsVisible((IToolbarHandler)arg1, arg2);

		public static void MapToolbarPlacement(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarPlacement((IToolbarHandler)arg1, arg2);

		public static void MapToolbarDynamicOverflowEnabled(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarDynamicOverflowEnabled((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonAccessibilityLabel(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonAccessibilityLabel((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonAccessibilityLabel(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapToolbarPlacement(IToolbarHandler arg1, Toolbar arg2)
		{
		}

		public static void MapToolbarDynamicOverflowEnabled(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateToolbarDynamicOverflowEnabled(arg2);
		}

		public static void MapBarTextColor(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarTextColor(arg2);
		}

		public static void MapBarBackground(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarBackground(arg2);
		}

		public static void MapBackButtonTitle(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapToolbarItems(IToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateMenu();
		}

		public static void MapIconColor(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIconColor(arg2);
		}

		public static void MapIcon(ToolbarHandler arg1, Toolbar arg2)
		{
		}

		public static void MapIcon(IToolbarHandler arg1, Toolbar arg2)
		{
		}

		public static void MapTitleView(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitleView(arg2);
		}

		public static void MapTitleIcon(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitleIcon(arg2);
		}

		public static void MapBackButtonVisible(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapBackButtonEnabled(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonEnabled((IToolbarHandler)arg1, arg2);


		public static void MapBackButtonEnabled(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapIsVisible(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIsVisible(arg2);
		}
	}
}
