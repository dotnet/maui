using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45067, "[UWP] No way of cleanly dismissing soft keyboard", PlatformAffected.WinRT)]
	public class Bugzilla45067 : TestContentPage
	{
		protected override void Init()
		{
			var button = new Button { Text = "Start" };

			button.Clicked += (sender, args) =>
			{
				SwitchMainPage();
			};

			Content = button;
		}

		void SwitchMainPage()
		{
			Application.Current.MainPage = new _45067Content();
		}

		class _45067Content : TestContentPage
		{
			protected override void Init()
			{
				var instructions1 = new Label { Text = "Enter text in the 'Username' Entry, then hit 'Enter/Return' on the soft keyboard. The keyboard should be dismissed." };

				var instructions2 = new Label { Text = "Enter text in the 'Password' Entry, then hit 'Enter/Return' on the soft keyboard. The keyboard should be dismissed." };

				var username = new Entry
				{
					Placeholder = "Username"
				};

				username.SetValue(AutomationProperties.LabeledByProperty, instructions1);

				var password = new Entry
				{
					Placeholder = "Password",
					IsPassword = true
				};

				password.SetValue(AutomationProperties.LabeledByProperty, instructions2);

				var button = new Button { Text = "Submit", IsEnabled = false };

				username.Completed += (s, e) =>
				{
					button.Focus();
				};

				password.Completed += (s, e) =>
				{
					button.Focus();
				};

				Content = new StackLayout
				{
					Children =
					{
						instructions1,
						username,
						instructions2,
						password,
						button
					}
				};
			}
		}
	}
}