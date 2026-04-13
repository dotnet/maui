using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		readonly ImageConverter _imageConverter = new ImageConverter();
		readonly ImageSourceIconElementConverter _imageSourceIconElementConverter = new ImageSourceIconElementConverter();

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

				if (commandBar.IsDynamicOverflowEnabled && item.Order == ToolbarItemOrder.Secondary)
				{
					button.SetBinding(AppBarButton.IconProperty, "IconImageSource", _imageSourceIconElementConverter);
				}
				else if (!item.IconImageSource.IsNullOrEmpty())
				{
					var img = new WImage();
					img.SetBinding(WImage.SourceProperty, "Value");
					img.SetBinding(WImage.DataContextProperty, "IconImageSource", _imageConverter);

					if (item.BadgeText is not null)
					{
						// Wrap icon in a Grid with InfoBadge overlay for badge support
						var grid = new WGrid();
#pragma warning disable RS0030 // Standalone WinUI Grid, not a MauiPanel
						grid.Children.Add(img);

						var infoBadge = new InfoBadge
						{
							HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
							VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
							Margin = new Microsoft.UI.Xaml.Thickness(0, -4, -4, 0),
						};
						UpdateInfoBadge(infoBadge, item);
						grid.Children.Add(infoBadge);
#pragma warning restore RS0030

						button.Content = grid;
					}
					else
					{
						button.Content = img;
					}
				}

				// For text-only toolbar items (no icon), wrap the label in a Grid
				// with InfoBadge overlay so badges are still visible.
				if (item.IconImageSource.IsNullOrEmpty() && item.BadgeText is not null)
				{
					var textBlock = new Microsoft.UI.Xaml.Controls.TextBlock { Text = item.Text ?? string.Empty };
					var grid = new WGrid();
#pragma warning disable RS0030 // Standalone WinUI Grid, not a MauiPanel
					grid.Children.Add(textBlock);

					var infoBadge = new InfoBadge
					{
						HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
						VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
						Margin = new Microsoft.UI.Xaml.Thickness(0, -4, -4, 0),
					};
					UpdateInfoBadge(infoBadge, item);
					grid.Children.Add(infoBadge);
#pragma warning restore RS0030

					button.Content = grid;
				}

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

			if (e.PropertyName == nameof(ToolbarItem.BadgeText) || e.PropertyName == nameof(ToolbarItem.BadgeColor) || e.PropertyName == nameof(ToolbarItem.BadgeTextColor))
			{
				if (sender is ToolbarItem toolbarItem)
				{
					foreach (var command in commandBar.PrimaryCommands)
					{
						if (command is AppBarButton button && ReferenceEquals(button.DataContext, toolbarItem))
						{
							if (button.Content is WGrid grid)
							{
#pragma warning disable RS0030 // Standalone WinUI Grid, not a MauiPanel
								foreach (var child in grid.Children)
#pragma warning restore RS0030
								{
									if (child is InfoBadge badge)
									{
										UpdateInfoBadge(badge, toolbarItem);
										break;
									}
								}
							}
							else if (toolbarItem.BadgeText is not null)
							{
								// Item didn't have a badge before - wrap content in Grid with InfoBadge
								var existingContent = button.Content as Microsoft.UI.Xaml.UIElement;
								if (existingContent == null && !toolbarItem.IconImageSource.IsNullOrEmpty())
								{
									var img = new WImage();
									img.SetBinding(WImage.SourceProperty, "Value");
									img.SetBinding(WImage.DataContextProperty, "IconImageSource", _imageConverter);
									existingContent = img;
								}
								else if (existingContent == null)
								{
									existingContent = new Microsoft.UI.Xaml.Controls.TextBlock { Text = toolbarItem.Text ?? string.Empty };
								}

								var newGrid = new WGrid();
#pragma warning disable RS0030 // Standalone WinUI Grid, not a MauiPanel
								newGrid.Children.Add(existingContent);

								var newBadge = new InfoBadge
								{
									HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
									VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
									Margin = new Microsoft.UI.Xaml.Thickness(0, -4, -4, 0),
								};
								UpdateInfoBadge(newBadge, toolbarItem);
								newGrid.Children.Add(newBadge);
#pragma warning restore RS0030

								button.Content = newGrid;
							}
							break;
						}
					}
				}
				return;
			}

			if (e.PropertyName == nameof(ToolbarItem.Text) || e.PropertyName == nameof(ToolbarItem.IconImageSource))
			{
				var toolbarItems = new List<ToolbarItem>(ToolbarItems ?? Array.Empty<ToolbarItem>());
				SetDefaultLabelPosition(commandBar, toolbarItems);
			}
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
