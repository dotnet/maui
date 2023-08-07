using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 893274, "Hide Soft Input On Tapped Page",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class HideSoftInputOnTappedPage : TestContentPage
	{
		Label _isKeyboardOpen = new Label();
		public HideSoftInputOnTappedPage()
		{
			Title = "Hide Soft Input On Tapped Page";
			_isKeyboardOpen.Text = "Tap Page and Keyboard Should Close";
			_isKeyboardOpen.AutomationId = "EmptySpace";
		}

		protected override void Init()
		{
			Entry entry = new Entry() { AutomationId = "Entry" };
			var checkBox = new CheckBox();
			checkBox.BindingContext = this;
			checkBox.SetBinding(CheckBox.IsCheckedProperty, "HideSoftInputOnTapped");
			checkBox.AutomationId = "ToggleHideSoftInputOnTapped";

			Content = new VerticalStackLayout()
			{
				new HorizontalStackLayout()
				{
					checkBox,
					_isKeyboardOpen
				},
				entry,
				new Editor() { AutomationId = "Editor" },
				new SearchBar() { AutomationId = "SearchBar" }
			};
		}
	}
}
