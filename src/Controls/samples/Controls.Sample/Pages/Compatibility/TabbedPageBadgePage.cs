using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class TabbedPageBadgePage : TabbedPage
	{
		readonly ContentPage _inboxPage;
		readonly ContentPage _alertsPage;
		readonly ContentPage _profilePage;
		int _inboxCount = 3;

		public TabbedPageBadgePage()
		{
			Title = "TabbedPage Badges";
			BarBackgroundColor = Colors.White;
			BarTextColor = Colors.Black;
			SelectedTabColor = Colors.DarkBlue;
			UnselectedTabColor = Colors.Gray;

			_inboxPage = CreateDemoPage("Inbox", "Numeric badge", "InboxBadgeStatus");
			_alertsPage = CreateDemoPage("Alerts", "Dot indicator", "AlertsBadgeStatus");
			_profilePage = CreateDemoPage("Profile", "Text badge", "ProfileBadgeStatus");

			_inboxPage.Content = CreateControls(
				_inboxPage,
				"Numeric badges update at runtime.",
				new Button
				{
					Text = "Increment",
					AutomationId = "IncrementInboxBadge",
					Command = new Command(() =>
					{
						_inboxCount++;
						SetBadgeText(_inboxPage, _inboxCount.ToString());
					})
				},
				new Button
				{
					Text = "Set 99+",
					Command = new Command(() => SetBadgeText(_inboxPage, "99+"))
				});

			_alertsPage.Content = CreateControls(
				_alertsPage,
				"An empty string displays a dot.",
				new Button
				{
					Text = "Show Dot",
					AutomationId = "ShowDotBadge",
					Command = new Command(() => SetBadgeText(_alertsPage, ""))
				},
				new Button
				{
					Text = "Show !",
					Command = new Command(() => SetBadgeText(_alertsPage, "!"))
				});

			_profilePage.Content = CreateControls(
				_profilePage,
				GetBadgeColorDescription(),
				new Button
				{
					Text = "Show New",
					Command = new Command(() => SetBadgeText(_profilePage, "New"))
				},
				new Button
				{
					Text = "Swap Colors",
					AutomationId = "SwapBadgeColors",
					Command = new Command(() =>
					{
						var useOrange = GetBadgeColor(_profilePage) != Colors.Orange;
						SetBadgeColor(_profilePage, useOrange ? Colors.Orange : Colors.Blue);
						SetBadgeTextColor(_profilePage, useOrange ? Colors.Black : Colors.White);
					})
				});

			Children.Add(_inboxPage);
			Children.Add(_alertsPage);
			Children.Add(_profilePage);

			RestoreBadges();
		}

		static string GetBadgeColorDescription()
		{
#if IOS || MACCATALYST
			return "Custom colors use public UIKit APIs. System tab bars on iOS and Mac Catalyst 18 or later may render the system badge colors instead.";
#else
			return "Text and colors are independently configurable.";
#endif
		}

		ContentPage CreateDemoPage(string title, string description, string statusAutomationId) =>
			new()
			{
				Title = title,
				AutomationId = $"{title}BadgeTab",
				Content = new Label
				{
					Text = description,
					AutomationId = statusAutomationId
				}
			};

		View CreateControls(Page page, string description, params Button[] actions)
		{
			var layout = new VerticalStackLayout
			{
				Padding = 24,
				Spacing = 12,
				Children =
				{
					new Label
					{
						Text = description,
						FontSize = 20,
						FontAttributes = FontAttributes.Bold
					}
				}
			};

			foreach (var action in actions)
			{
				layout.Children.Add(action);
			}

			layout.Children.Add(new Button
			{
				Text = "Clear This Badge",
				Command = new Command(() => SetBadgeText(page, null))
			});
			layout.Children.Add(new Button
			{
				Text = "Restore All Badges",
				AutomationId = "RestoreAllBadges",
				Command = new Command(RestoreBadges)
			});

			return new ScrollView { Content = layout };
		}

		void RestoreBadges()
		{
			_inboxCount = 3;
			SetBadgeText(_inboxPage, "3");
			SetBadgeColor(_inboxPage, Colors.Red);
			SetBadgeTextColor(_inboxPage, Colors.White);

			SetBadgeText(_alertsPage, "");
			SetBadgeColor(_alertsPage, Colors.Orange);
			SetBadgeTextColor(_alertsPage, Colors.Black);

			SetBadgeText(_profilePage, "New");
			SetBadgeColor(_profilePage, Colors.Blue);
			SetBadgeTextColor(_profilePage, Colors.White);
		}
	}
}
