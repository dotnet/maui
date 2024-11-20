using System.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Inset Test",
	PlatformAffected.All)]
public class ShellInsets : TestShell
{
	const string EntryTest = nameof(EntryTest);
	const string EntryToClick = "EntryToClick";
	const string EntryToClick2 = "EntryToClick2";
	const string CreateTopTabButton = "CreateTopTabButton";
	const string CreateBottomTabButton = "CreateBottomTabButton";

	const string EntrySuccess = "EntrySuccess";
	const string ResetKeyboard = "Hide Keyboard";
	const string ResetKeyboard2 = "Hide Keyboard 2";
	const string Reset = "Reset";

	const string ToggleSafeArea = "ToggleSafeArea";
	const string SafeAreaTest = "SafeAreaTest";
	const string SafeAreaTopLabel = "SafeAreaTopLabel";
	const string SafeAreaBottomLabel = "SafeAreaBottomLabel";

	const string ListViewTest = "ListViewTest";

	const string PaddingTest = "PaddingTest";
	const string PaddingEntry = "PaddingEntry";
	const string PaddingLabel = "PaddingLabel";

	const string EmptyPageSafeAreaTest = "EmptyPageSafeAreaTest";

	protected override void Init()
	{
		SetupLandingPage();
	}

	void SetupLandingPage()
	{
		var page = CreateContentPage();
		CheckBox cbox = new CheckBox() { AutomationId = ToggleSafeArea };
		Microsoft.Maui.Controls.Entry entryPadding = new() { WidthRequest = 150, AutomationId = PaddingEntry };

		var stackLayout1 = new StackLayout()
		{
			cbox,
			new Button()
			{
				Text = "Safe Area",
				Command = new Command(() => SafeArea(cbox.IsChecked)),
				AutomationId = SafeAreaTest
			}
		};

		stackLayout1.Orientation = StackOrientation.Horizontal;
		stackLayout1.HorizontalOptions = LayoutOptions.Center;

		var stackLayout2 = new StackLayout()
		{
			entryPadding,
			new Button()
			{
				Text = "Padding",
				Command = new Command(() => PaddingPage(entryPadding.Text)),
				AutomationId = PaddingTest
			}
		};

		stackLayout2.Orientation = StackOrientation.Horizontal;
		stackLayout2.HorizontalOptions = LayoutOptions.Center;

		var stackLayout = new StackLayout()
		{
				new Button()
				{
					Text = "Entry Inset",
					Command = new Command(() => EntryInset()),
					AutomationId = EntryTest
				},
				new Button()
				{
					Text = "Safe Area on Page with no header",
					Command = new Command(() => EmptyPageSafeArea()),
					AutomationId = EmptyPageSafeAreaTest
				},
				new Button()
				{
					Text = "List View Scroll Test",
					Command = new Command(() => ListViewPage()),
					AutomationId = ListViewTest
				},
				stackLayout1,
				stackLayout2
		};

		page.Content = stackLayout;

		CurrentItem = Items.Last();
		if (Items.Count > 1)
		{
			Items.RemoveAt(0);
		}
	}

	void EmptyPageSafeArea()
	{
		var page = CreateContentPage();
		var topLabel = new Label() { Text = "Top Label", HeightRequest = 0, AutomationId = SafeAreaTopLabel, VerticalOptions = LayoutOptions.Start };

		var stackLayout = new StackLayout()
		{
			topLabel,
			new StackLayout()
			{
					new Label() { Text = "This page should have no safe area padding at the top" },
					new Button() { Text = "Reset", Command = new Command(() => SetupLandingPage()) }
			}
		};
#pragma warning disable CS0618 // Type or member is obsolete
		stackLayout.VerticalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete

		stackLayout.BackgroundColor = Colors.White;

		page.Content = stackLayout;

		page.BackgroundColor = Colors.Yellow;

		PropertyChangedEventHandler propertyChangedEventHandler = null;
		propertyChangedEventHandler = (sender, args) =>
		{
			if (args.PropertyName == Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty.PropertyName)
			{
				if (page.On<iOS>().SafeAreaInsets().Top > 0)
				{
					page.PropertyChanged -= propertyChangedEventHandler;
					topLabel.HeightRequest = page.On<iOS>().SafeAreaInsets().Top;
				}
			}
		};

		page.PropertyChanged += propertyChangedEventHandler;

		Shell.SetTabBarIsVisible(page, false);
		Shell.SetNavBarIsVisible(page, false);
		CurrentItem = Items.Last();
		Items.RemoveAt(0);
	}

	void ListViewPage()
	{
		var page = CreateContentPage();

		page.Content = new Microsoft.Maui.Controls.ListView(ListViewCachingStrategy.RecycleElement)
		{
			ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cell = new ViewCell();
				var label = new Label() { Text = " I am a label" };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				var stackLayout = new StackLayout()
				{
					label,
					new Microsoft.Maui.Controls.Entry(),
					new Button() { Text = "Reset", Command = new Command(() => SetupLandingPage()) }
				};

				stackLayout.Orientation = StackOrientation.Horizontal;

				cell.View = stackLayout;

				return cell;
			}),
			ItemsSource = Enumerable.Range(0, 1000).Select(x => $"Item{x}").ToArray()
		};
		page.BackgroundColor = Colors.Yellow;

		CurrentItem = Items.Last();
		Items.RemoveAt(0);
	}

	void PaddingPage(string text)
	{
		var page = CreateContentPage();

		int padding = 0;
		if (Int32.TryParse(text, out padding))
			page.Padding = padding;

		page.On<iOS>().SetUseSafeArea(false);
		page.Content = new StackLayout()
		{
			new Label(){ Text = "Top Label", HeightRequest = 200},
			new Label() { Text = $"Padding: {text}", AutomationId = PaddingLabel},
			new Button(){Text = "Reset", Command = new Command(() => SetupLandingPage() )}
		};
		page.BackgroundColor = Colors.Yellow;

		CurrentItem = Items.Last();
		Items.RemoveAt(0);
	}


	void SafeArea(bool value)
	{
		var page = CreateContentPage();
		page.Content = new StackLayout()
		{
			new Label(){ Text = "Top Label", HeightRequest = 200, AutomationId = SafeAreaTopLabel},
			new Label(){ Text = value ? "You should see two labels" : "You should see one label", AutomationId = SafeAreaBottomLabel},
			new Button(){ Text = "Reset", Command = new Command(() => SetupLandingPage() )}
		};
		page.BackgroundColor = Colors.Yellow;

		page.On<iOS>().SetUseSafeArea(value);
		CurrentItem = Items.Last();
		Items.RemoveAt(0);
	}

	void EntryInset()
	{
		var page = CreateContentPage();
		page.Title = "Main";
		page.Content = CreateEntryInsetView();
		page.BackgroundColor = Colors.Yellow;

		CurrentItem = Items.Last();
		Items.RemoveAt(0);
	}

	View CreateEntryInsetView()
	{
		Microsoft.Maui.Controls.ScrollView view = null;
#pragma warning disable CS0618 // Type or member is obsolete
		view = new Microsoft.Maui.Controls.ScrollView()
		{
			Content = new StackLayout()
			{
					new Label(){ AutomationId = EntrySuccess, VerticalOptions= LayoutOptions.FillAndExpand, Text = "Click the entry and it should scroll up and stay visible. Click off entry and this label should still be visible"},
					new Button(){ Text = "Change Navbar Visible", Command = new Command(() => Shell.SetNavBarIsVisible(view.Parent, !(Shell.GetNavBarIsVisible(view.Parent))))},
					new Button()
					{
						Text = "Push On Page",
						Command = new Command(() => Navigation.PushAsync(new ContentPage(){ Content = CreateEntryInsetView() }))
					},
					new Button(){Text = "Reset", Command = new Command(() => SetupLandingPage() )},
					new Button()
					{
						Text = ResetKeyboard

					},
					new Microsoft.Maui.Controls.Entry()
					{
						AutomationId = EntryToClick
					},
					new Button()
					{
						Text = ResetKeyboard,
						AutomationId = ResetKeyboard2

					},
					new Button()
					{
						Text = "Top Tab",
						AutomationId = CreateTopTabButton,
						Command = new Command(() => AddTopTab("top"))
					},
					new Button()
					{
						Text = "Bottom Tab",
						AutomationId = CreateBottomTabButton,
						Command = new Command(() => AddBottomTab("bottom"))
					},
					new Microsoft.Maui.Controls.Entry()
					{
						AutomationId = EntryToClick2
					},
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete

		return view;
	}
}
