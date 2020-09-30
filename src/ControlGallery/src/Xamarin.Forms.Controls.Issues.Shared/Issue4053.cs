using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4053, "AutomationProperties.Name on Button is visible on Android", PlatformAffected.Android)]
	public class Issue4053 : TestContentPage
	{
		protected override void Init()
		{
			Button button = new Button();
			Switch _switch = new Switch();
			AutomationProperties.SetName(button, "invisible text");
			AutomationProperties.SetName(_switch, "invisible text");

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "The text on the controls below should be empty. But TalkBack should read 'invisible text'" },
					button,
					_switch
				}
			};
		}
	}
}