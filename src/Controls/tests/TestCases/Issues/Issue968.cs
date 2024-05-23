
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 968, "StackLayout does not relayout on device rotation", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue968 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Nav Bar";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray,
				AutomationId = "TestReady"
			};

			layout.Children.Add(new BoxView
			{
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			});

			layout.Children.Add(new Label
			{
				Text = "You should see me after rotating"
			});

			Content = layout;
		}
	}
}
