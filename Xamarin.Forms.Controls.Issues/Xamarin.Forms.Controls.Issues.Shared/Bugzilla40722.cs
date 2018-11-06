using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40722, "Using FormsAppCompatActivity calls OnDisappearing on device sleep")]
	public class Bugzilla40722 : TestContentPage
	{
		const string ButtonText_Disable = "Disable Pause/Resume events";
		const string ButtonText_Enable = "Enable Pause/Resume events";
		const string Instructions_Disabled = "Sleep the device, then wake it. If \"Disappearing!\" and/or \"Appearing!\" is displayed on this screen, this test has failed.";
		const string Instructions_Enabled = "Sleep the device, then wake it. If \"Disappearing!\" and/or \"Appearing!\" is NOT displayed on this screen, this test has failed.";

		Label _Target = new Label();
		bool _sendEvents = true;

		protected override void Init()
		{
			ToggleEvents(_sendEvents);

			var instructions = new Label
			{
				Text = Instructions_Enabled
			};

			var button = new Button
			{
				Text = ButtonText_Disable
			};

			button.Clicked += (sender, e) =>
			{
				_sendEvents = !_sendEvents;
				ToggleEvents(_sendEvents);
				button.Text = _sendEvents ? ButtonText_Disable : ButtonText_Enable;
				instructions.Text = _sendEvents ? Instructions_Enabled : Instructions_Disabled;
				_Target.Text = "";
			};

			Content = new StackLayout { Children = { instructions, button, _Target } };
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();

			_Target.Text += "\r\nAppearing!";
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			_Target.Text += "\r\nDisappearing!";
		}

		void ToggleEvents(bool value)
		{
			Application.Current.On<Android>()
							.SendDisappearingEventOnPause(value)
							.SendAppearingEventOnResume(value);
		}
	}
}
