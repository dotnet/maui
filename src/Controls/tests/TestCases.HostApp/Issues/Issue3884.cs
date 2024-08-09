using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3884, "BoxView corner radius", PlatformAffected.Android)]
	public class Issue3884 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var label = new Label { Text = "You should see a blue circle" };
			var box = new BoxView
			{
				AutomationId = "TestReady",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Blue,
				HeightRequest = 100,
				WidthRequest = 100,
				CornerRadius = 50
			};

			Content = new StackLayout
			{
				Children = { label, box }
			};
		}
	}
}
