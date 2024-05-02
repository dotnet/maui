using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 12003, "Hide Soft Input On Tapped Page",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class HideSoftInputOnTappedPage : NavigationPage
	{
		public HideSoftInputOnTappedPage() : base(new StartingPage())
		{

		}

		public class StartingPage : TestContentPage
		{

			protected override void Init()
			{
				Content = new VerticalStackLayout()
			{
				new Button()
				{
					Text = "Test HideSoftInputOnTapped: false",
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new TestPage(false));
					}),
					AutomationId = "HideSoftInputOnTappedFalse"
				},
				new Button()
				{
					Text = "Test HideSoftInputOnTapped: true",
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new TestPage(true));
					}),
					AutomationId = "HideSoftInputOnTappedTrue"
				}
			};

				(Content as VerticalStackLayout).Spacing = 6;
			}

			public class TestPage : ContentPage
			{
				Label _isKeyboardOpen = new Label();
				public TestPage(bool hideSoftInputOnTapped)
				{
					this.HideSoftInputOnTapped = hideSoftInputOnTapped;
					Title = "Hide Soft Input On Tapped Page";
					_isKeyboardOpen.Text = "Tap Page and Keyboard Should Close";
					_isKeyboardOpen.AutomationId = "EmptySpace";

					Entry entry = new Entry() { AutomationId = "Entry" };

					var checkbox = new CheckBox();
					checkbox.BindingContext = this;
					checkbox.SetBinding(
						CheckBox.IsCheckedProperty,
						nameof(HideSoftInputOnTapped));

					checkbox.AutomationId = "ToggleHideSoftInputOnTapped";

					Entry dontHideKeyboardWhenTappingPage = new Entry()
					{
						Placeholder = "When this entry is focused tapping the page won't close the keyboard",
						AutomationId = "DontHideKeyboardWhenTappingPage"
					};

					dontHideKeyboardWhenTappingPage
						.Focused += (_, _) => checkbox.IsChecked = false;

					Entry hideKeyboardWhenTappingPage = new Entry()
					{
						Placeholder = "When this entry is focused tapping the page will close the keyboard",
						AutomationId = "HideKeyboardWhenTappingPage"
					};

					hideKeyboardWhenTappingPage
						.Focused += (_, _) => checkbox.IsChecked = true;

					Content = new VerticalStackLayout()
				{
					new HorizontalStackLayout()
					{
						checkbox,
						_isKeyboardOpen
					},
					entry,
					new Editor() { AutomationId = "Editor" },
					new SearchBar() { AutomationId = "SearchBar" },
					dontHideKeyboardWhenTappingPage,
					hideKeyboardWhenTappingPage
				};
				}
			}
		}
	}
}
