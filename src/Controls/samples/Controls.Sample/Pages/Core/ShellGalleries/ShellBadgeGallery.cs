using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.ShellGalleries
{
	public class ShellBadgeGallery : ContentPage
	{
		readonly Entry _badgeTextEntry;
		readonly Label _currentBadgeLabel;

		public ShellBadgeGallery()
		{
			Title = "Badge Gallery";

			_badgeTextEntry = new Entry
			{
				Placeholder = "Enter badge text (e.g. 5)",
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing
			};

			_currentBadgeLabel = new Label
			{
				Text = "Current badges: (none)",
				TextColor = Colors.Gray
			};

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Spacing = 10,
					Padding = new Thickness(16),
					Children =
					{
						new Label
						{
							Text = "Shell Badge Support",
							FontSize = 24,
							FontAttributes = FontAttributes.Bold
						},
						new Label
						{
							Text = "Set badges on Shell tabs. Enter a badge value below and tap a button to apply it to the corresponding tab.",
							TextColor = Colors.Gray
						},

						new BoxView { HeightRequest = 1, Color = Colors.LightGray },

						new Label { Text = "Badge Text", FontAttributes = FontAttributes.Bold },
						_badgeTextEntry,

						new Label { Text = "Apply Badge to Tab", FontAttributes = FontAttributes.Bold },
						CreateApplyButton("Set Badge on Current Tab", OnSetBadgeCurrentTab),
						CreateApplyButton("Set Badge on All Tabs", OnSetBadgeAllTabs),
						CreateApplyButton("Set Dot Badge (empty string)", OnSetDotBadge),
						CreateApplyButton("Clear All Badges", OnClearAllBadges),

						new BoxView { HeightRequest = 1, Color = Colors.LightGray },

						new Label { Text = "Badge Color", FontAttributes = FontAttributes.Bold },
						CreateColorButton("Red", Colors.Red),
						CreateColorButton("Blue", Colors.Blue),
						CreateColorButton("Green", Colors.Green),
						CreateColorButton("Orange", Colors.Orange),
						CreateColorButton("Purple", Colors.Purple),
						CreateColorButton("Default (clear color)", null),

						new BoxView { HeightRequest = 1, Color = Colors.LightGray },

						new Label { Text = "Badge Text Color", FontAttributes = FontAttributes.Bold },
						CreateTextColorButton("White", Colors.White),
						CreateTextColorButton("Black", Colors.Black),
						CreateTextColorButton("Yellow", Colors.Yellow),
						CreateTextColorButton("Default (clear)", null),

						new BoxView { HeightRequest = 1, Color = Colors.LightGray },

						new Label { Text = "Quick Actions", FontAttributes = FontAttributes.Bold },
						CreateApplyButton("Notification Dot (empty badge)", OnSetDotBadge),
						CreateApplyButton("Increment Current Badge", OnIncrementBadge),
						CreateApplyButton("Decrement Current Badge", OnDecrementBadge),

						new BoxView { HeightRequest = 1, Color = Colors.LightGray },

						_currentBadgeLabel
					}
				}
			};
		}

		static Button CreateApplyButton(string text, EventHandler handler)
		{
			var btn = new Button { Text = text };
			btn.Clicked += handler;
			return btn;
		}

		Button CreateColorButton(string colorName, Color? color)
		{
			var btn = new Button
			{
				Text = $"Set Badge Color: {colorName}",
				BackgroundColor = color ?? Colors.LightGray,
				TextColor = color is not null ? Colors.White : Colors.Black
			};
			btn.Clicked += (s, e) =>
			{
				var section = GetCurrentShellSection();
				if (section is not null)
				{
					section.BadgeColor = color;
					UpdateBadgeStatus();
				}
			};
			return btn;
		}

		Button CreateTextColorButton(string colorName, Color? color)
		{
			var btn = new Button
			{
				Text = $"Set Badge Text Color: {colorName}",
				BackgroundColor = color ?? Colors.LightGray,
				TextColor = color is not null ? Colors.White : Colors.Black
			};
			btn.Clicked += (s, e) =>
			{
				var section = GetCurrentShellSection();
				if (section is not null)
				{
					section.BadgeTextColor = color;
					UpdateBadgeStatus();
				}
			};
			return btn;
		}

		void OnSetBadgeCurrentTab(object? sender, EventArgs e)
		{
			var section = GetCurrentShellSection();
			if (section is not null)
			{
				section.BadgeText = _badgeTextEntry.Text;
				UpdateBadgeStatus();
			}
		}

		void OnSetBadgeAllTabs(object? sender, EventArgs e)
		{
			var sections = GetAllShellSections();
			if (sections is null)
				return;

			foreach (var section in sections)
			{
				section.BadgeText = _badgeTextEntry.Text;
			}
			UpdateBadgeStatus();
		}

		void OnClearAllBadges(object? sender, EventArgs e)
		{
			var sections = GetAllShellSections();
			if (sections is null)
				return;

			foreach (var section in sections)
			{
				section.BadgeText = null;
				section.BadgeColor = null;
			}
			UpdateBadgeStatus();
		}

		void OnSetDotBadge(object? sender, EventArgs e)
		{
			var section = GetCurrentShellSection();
			if (section is not null)
			{
				// Empty string shows as a dot indicator on all platforms
				section.BadgeText = "";
				UpdateBadgeStatus();
			}
		}

		void OnIncrementBadge(object? sender, EventArgs e)
		{
			var section = GetCurrentShellSection();
			if (section is not null)
			{
				if (int.TryParse(section.BadgeText, out var current))
					section.BadgeText = (current + 1).ToString();
				else
					section.BadgeText = "1";
				UpdateBadgeStatus();
			}
		}

		void OnDecrementBadge(object? sender, EventArgs e)
		{
			var section = GetCurrentShellSection();
			if (section is not null)
			{
				if (int.TryParse(section.BadgeText, out var current) && current > 0)
					section.BadgeText = (current - 1).ToString();
				else
					section.BadgeText = null;
				UpdateBadgeStatus();
			}
		}

		ShellSection? GetCurrentShellSection()
		{
			return Shell.Current?.CurrentItem?.CurrentItem;
		}

		IList<ShellSection>? GetAllShellSections()
		{
			var shellItem = Shell.Current?.CurrentItem;
			if (shellItem is null)
				return null;

			return ((IShellItemController)shellItem).GetItems();
		}

		void UpdateBadgeStatus()
		{
			var sections = GetAllShellSections();
			if (sections is null || sections.Count == 0)
			{
				_currentBadgeLabel.Text = "Current badges: (none)";
				return;
			}

			var status = new System.Text.StringBuilder("Current badges:\n");
			for (int i = 0; i < sections.Count; i++)
			{
				var section = sections[i];
				var badge = section.BadgeText ?? "(none)";
				var color = section.BadgeColor?.ToString() ?? "default";
				status.AppendLine($"  Tab {i} ({section.Title}): \"{badge}\" [color: {color}]");
			}
			_currentBadgeLabel.Text = status.ToString();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateBadgeStatus();
		}
	}
}
