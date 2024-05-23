using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51505, "ObjectDisposedException On Effect detachment.", PlatformAffected.Android)]
	public class Bugzilla51505 : TestContentPage
	{
		const string ButtonId = "button";

		protected override void Init()
		{
			var effect = Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.BorderEffect");

			var button = new Button { Text = "Click me", AutomationId = ButtonId };
			button.Clicked += async (sender, e) =>
			{
				await Navigation.PopAsync();
			};
			button.Effects.Add(effect);

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The following Button has an Effect applied to it that should attempt to access the Control when it is Detached. When you click the Button, this page should be popped. If the app crashes, this test has failed."
					},
					button
				}
			};
		}
	}
}