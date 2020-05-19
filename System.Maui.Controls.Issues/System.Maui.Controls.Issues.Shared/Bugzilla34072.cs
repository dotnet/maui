using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 34072, "Inconsistent Disabled Button behavior between Forms for Android & iOS")]
	public class Bugzilla34072 : TestContentPage
	{
		Button _testButton;
		Label _reproStepsLabel;

		protected override void Init ()
		{
			_testButton = new Button () {
				Text = "Enabled",
				TextColor = Color.Yellow,
				IsEnabled = true
			};

			var switchStateButton = new Button {
				Text = "Switch Enabled State",
				Command = new Command (SwitchState),
			};

			_reproStepsLabel = new Label () {
				Text = "Tap the 'Switch Enabled State' button. The top button text should be grayed out when the button is Disabled. If the text remains the same color in both states, this is broken."
			};

			Content = new StackLayout {
				Padding = 10,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Vertical,
				Children = {
					_testButton,
					switchStateButton,
					_reproStepsLabel
				}
			};
		}

		public void SwitchState ()
		{
			if (_testButton.IsEnabled) {
				_testButton.IsEnabled = false;
				_testButton.Text = "Disabled";
				return;
			}

			_testButton.IsEnabled = true;
			_testButton.Text = "Enabled";
		}
	}
}
