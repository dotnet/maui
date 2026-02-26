namespace Maui.Controls.Sample.Issues;

// Reproduces GitHub Issue #33703 / #33444:
// "[.Net10][Android] FlyoutItem with tab — Extra space on top of bottom tab bar while navigating back"
//
// Shell structure (mirrors the issue report exactly):
//   Shell
//     └─ FlyoutItem  (FlyoutDisplayOptions=AsSingleItem, FlyoutItemIsVisible=false, Route="home")
//           ├─ Tab "Home"        → HomeContentPage  (has button → DetailPage)
//           └─ Tab "Second Page" → SecondContentPage
//
// Steps to reproduce:
//   1. Run on Android.
//   2. Tap "Go to Detail Page" on the Home tab.
//   3. The TabBar is HIDDEN on DetailPage.
//   4. Navigate back.
//   5. BUG: a blank/dark strip appears ABOVE the TabBar on the Home tab.
//      Switching tabs and back makes it disappear (confirming it's a layout bug).
[Issue(IssueTracker.Github, 33703, "Empty space above TabBar after navigate back when TabBar visibility toggled", PlatformAffected.Android)]
public class Issue33703 : Shell
{
	public Issue33703()
	{
		// ── Home tab content ───────────────────────────────────────────────────────
		var homeContentPage = BuildHomeContentPage();

		// ── Second page content ────────────────────────────────────────────────────
		var secondContentPage = new ContentPage
		{
			Title = "Second Page",
			BackgroundColor = Colors.CornflowerBlue,
			Content = new Label
			{
				Text = "Second Page shell content",
				AutomationId = "SecondTabLabel",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.White,
				FontSize = 18
			}
		};

		// ── Build Shell hierarchy: FlyoutItem → Tab → ShellContent ────────────────
		var homeShellContent = new ShellContent
		{
			Content = homeContentPage
		};
		Shell.SetFlyoutItemIsVisible(homeShellContent, false);

		var homeTab = new Tab { Title = "Home" };
		homeTab.Items.Add(homeShellContent);

		var secondTab = new Tab { Title = "Second Page", Route = "MyDevices" };
		secondTab.Items.Add(new ShellContent
		{
			Title = "Second page shell",
			Content = secondContentPage
		});

		var flyoutItem = new FlyoutItem
		{
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
			Route = "home"
		};
		Shell.SetFlyoutItemIsVisible(flyoutItem, false);
		flyoutItem.Items.Add(homeTab);
		flyoutItem.Items.Add(secondTab);

		Items.Add(flyoutItem);
	}

	// Builds the Home tab page with a button to DetailPage
	static ContentPage BuildHomeContentPage()
	{
		var stackLayout = new VerticalStackLayout
		{
			Spacing = 0,
			BackgroundColor = Colors.White
		};

		// Instruction label
		stackLayout.Add(new Label
		{
			Text = "Issue #33703 — FlyoutItem + Tab extra space repro\n\n" +
			       "Steps:\n" +
			       "  1. Tap 'Go to Detail Page' below.\n" +
			       "  2. TabBar is HIDDEN on that page.\n" +
			       "  3. Navigate back.\n" +
			       "  4. GREEN flush to TabBar = FIXED ✓\n" +
			       "     Blank strip above TabBar = BUG ✗",
			AutomationId = "InstructionLabel",
			FontSize = 13,
			Padding = 14,
			BackgroundColor = Colors.DarkOrange,
			TextColor = Colors.White
		});

		// Navigate button
		var btn = new Button
		{
			Text = "Go to Detail Page",
			AutomationId = "NavigateButton",
			Margin = new Thickness(20, 20, 20, 8),
			BackgroundColor = Colors.DodgerBlue,
			TextColor = Colors.White,
			CornerRadius = 6
		};
		btn.Clicked += async (s, e) => await Shell.Current.Navigation.PushAsync(new DetailPage());
		stackLayout.Add(btn);

		// List rows
		for (int i = 1; i <= 15; i++)
		{
			stackLayout.Add(new Label
			{
				Text = $"Row {i}",
				Padding = new Thickness(20, 10),
				BackgroundColor = i % 2 == 0 ? Color.FromArgb("#EEEEEE") : Colors.White
			});
		}

		// Green flush-marker — must sit right above the TabBar with zero gap after back-nav
		stackLayout.Add(new Label
		{
			Text = "▲ This green bar should sit FLUSH above the TabBar — any gap = BUG",
			AutomationId = "BottomLabel",
			Padding = 14,
			BackgroundColor = Colors.LimeGreen,
			TextColor = Colors.DarkGreen,
			HorizontalOptions = LayoutOptions.Fill
		});

		return new ContentPage
		{
			Title = "Home",
			BackgroundColor = Colors.White,
			Content = new ScrollView 
			{ 
				Content = stackLayout,
				AutomationId = "MainScrollView"
			}
		};
	}

	// DetailPage — TabBar intentionally hidden here
	class DetailPage : ContentPage
	{
		public DetailPage()
		{
			Title = "Detail Page";
			Shell.SetTabBarIsVisible(this, false);  // ← key trigger for issue
			BackgroundColor = Colors.LightSalmon;

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 14,
				Children =
				{
					new Label
					{
						Text = "Detail Page",
						AutomationId = "DetailLabel",
						FontSize = 26,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.White,
						BackgroundColor = Colors.Tomato,
						Padding = 14,
						HorizontalOptions = LayoutOptions.Fill
					},
					new Label
					{
						Text = "Shell.SetTabBarIsVisible(this, false) is set here.\n\n" +
						       "Navigate back. On the Home tab, if a blank strip " +
						       "appears ABOVE the TabBar, Issue #33703 is reproduced.",
						FontSize = 14,
						TextColor = Colors.DarkRed
					}
				}
			};
		}
	}
}
