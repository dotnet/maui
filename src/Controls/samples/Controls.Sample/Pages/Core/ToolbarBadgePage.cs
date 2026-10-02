using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Maui.Controls.Sample.Pages.Base;

namespace Maui.Controls.Sample.Pages;

public class ToolbarBadgePage : BasePage
{
	readonly ToolbarItem _numericItem;
	readonly ToolbarItem _textItem;
	readonly ToolbarItem _colorItem;
	readonly Label _statusLabel;
	int _count;

	public ToolbarBadgePage()
	{
		// Remove default Settings toolbar item from BasePage and re-add with badge
		ToolbarItems.Clear();

		_statusLabel = new Label
		{
			Text = "Toolbar items above have badges. Use buttons to interact.",
			FontSize = 16,
			Margin = new Thickness(0, 0, 0, 20)
		};

		_numericItem = new ToolbarItem
		{
			Text = "Alerts",
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ionicons",
				Glyph = "\uf2e3",
				Color = Colors.Black
			},
			BadgeText = "3"
		};
		_numericItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Alerts";

		_textItem = new ToolbarItem
		{
			Text = "Messages",
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ionicons",
				Glyph = "\uf30c",
				Color = Colors.Black
			},
			BadgeText = "New"
		};
		_textItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Messages";

		_colorItem = new ToolbarItem
		{
			Text = "Cart",
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ionicons",
				Glyph = "\uf30d",
				Color = Colors.Black
			},
			BadgeText = "2",
			BadgeColor = Colors.Green
		};
		_colorItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Cart";

		ToolbarItems.Add(_numericItem);
		ToolbarItems.Add(_textItem);
		ToolbarItems.Add(_colorItem);

		_count = 3;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = 20,
				Children =
				{
					_statusLabel,
					CreateSection("Badge Count",
						new Button { Text = "Increment Count", Command = new Command(IncrementCount) },
						new Button { Text = "Decrement Count", Command = new Command(DecrementCount) },
						new Button { Text = "Set Large Count (99+)", Command = new Command(() => SetBadgeText(_numericItem, "99+")) }
					),
					CreateSection("Badge Text",
						new Button { Text = "Set 'New'", Command = new Command(() => SetBadgeText(_textItem, "New")) },
						new Button { Text = "Set '!'", Command = new Command(() => SetBadgeText(_textItem, "!")) },
						new Button { Text = "Set Empty (dot badge)", Command = new Command(() => SetBadgeText(_textItem, "")) }
					),
					CreateSection("Badge Color",
						new Button { Text = "Red", Command = new Command(() => SetBadgeColor(Colors.Red)) },
						new Button { Text = "Blue", Command = new Command(() => SetBadgeColor(Colors.Blue)) },
						new Button { Text = "Green", Command = new Command(() => SetBadgeColor(Colors.Green)) },
						new Button { Text = "Platform Default (null)", Command = new Command(() => SetBadgeColor(null)) }
					),
					CreateSection("Badge Text Color",
						new Button { Text = "White Text", Command = new Command(() => SetBadgeTextColor(Colors.White)) },
						new Button { Text = "Black Text", Command = new Command(() => SetBadgeTextColor(Colors.Black)) },
						new Button { Text = "Yellow Text", Command = new Command(() => SetBadgeTextColor(Colors.Yellow)) },
						new Button { Text = "Platform Default (null)", Command = new Command(() => SetBadgeTextColor(null)) }
					),
					CreateSection("Visibility",
						new Button { Text = "Clear All Badges", Command = new Command(ClearAll) },
						new Button { Text = "Restore All Badges", Command = new Command(RestoreAll) }
					),
					new Label
					{
						Text = "Platform Notes:\n" +
						       "• Android: Full support via Material BadgeDrawable\n" +
						       "• iOS/macOS: Requires iOS 26+ / macOS 26+\n" +
						       "• Windows: Non-numeric text shows as dot indicator",
						FontSize = 12,
						TextColor = Colors.Gray,
						Margin = new Thickness(0, 20, 0, 0)
					}
				}
			}
		};
	}

	static Border CreateSection(string title, params View[] children)
	{
		var stack = new VerticalStackLayout { Spacing = 8 };
		stack.Children.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 14 });
		foreach (var child in children)
			stack.Children.Add(child);

		return new Border
		{
			Content = stack,
			Padding = 12,
			Margin = new Thickness(0, 4),
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
			Stroke = Colors.LightGray
		};
	}

	void IncrementCount()
	{
		_count++;
		_numericItem.BadgeText = _count.ToString();
		_statusLabel.Text = $"Count: {_count}";
	}

	void DecrementCount()
	{
		_count = Math.Max(0, _count - 1);
		_numericItem.BadgeText = _count > 0 ? _count.ToString() : null;
		_statusLabel.Text = _count > 0 ? $"Count: {_count}" : "Count badge cleared (0)";
	}

	void SetBadgeText(ToolbarItem item, string text)
	{
		item.BadgeText = text;
		_statusLabel.Text = string.IsNullOrEmpty(text) ? "Badge text: (empty/dot)" : $"Badge text: '{text}'";
	}

	void SetBadgeColor(Color? color)
	{
		_colorItem.BadgeColor = color;
		_statusLabel.Text = color is null ? "Badge color: platform default" : $"Badge color: {color}";
	}

	void SetBadgeTextColor(Color? color)
	{
		// Apply text color to all toolbar items to demonstrate the effect
		_numericItem.BadgeTextColor = color;
		_textItem.BadgeTextColor = color;
		_colorItem.BadgeTextColor = color;
		_statusLabel.Text = color is null ? "Badge text color: platform default" : $"Badge text color: {color}";
	}

	void ClearAll()
	{
		_numericItem.BadgeText = null;
		_textItem.BadgeText = null;
		_colorItem.BadgeText = null;
		_count = 0;
		_statusLabel.Text = "All badges cleared";
	}

	void RestoreAll()
	{
		_count = 3;
		_numericItem.BadgeText = "3";
		_textItem.BadgeText = "New";
		_colorItem.BadgeText = "2";
		_colorItem.BadgeColor = Colors.Green;
		_statusLabel.Text = "Badges restored";
	}
}
