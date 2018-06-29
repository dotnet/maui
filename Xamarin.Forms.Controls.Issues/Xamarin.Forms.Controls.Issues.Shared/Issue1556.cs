using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1556, "Animation tasks do not complete when Battery Saver enabled", 
		PlatformAffected.Android)]
	public class Issue1556 : TestContentPage
	{
		const string FirstLabel = "Label 1";
		const string SecondLabel = "Label 2";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text =
					"Once the page appears, you have 30 seconds to enable Battery Saver; enabling Battery Saver " 
					+ "should make both labels fully visible immediately. " 
					+ "If either label is not fully visible, this test has failed"
			};

			var label1 = new Label { Text = FirstLabel, Opacity = 0 };
			var label2 = new Label { Text = SecondLabel, IsVisible = false };

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(label1);
			layout.Children.Add(label2);

			Content = layout;

			Appearing += async (sender, args) =>
			{
				await label1.FadeTo(1, 30000);
				label2.IsVisible = true;
			};
		}
	}
}