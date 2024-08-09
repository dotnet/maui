using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 181, "Color not initialized for Label", PlatformAffected.Android)]
	public class Issue181 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 181";
			Content = new Frame
			{
				BorderColor = Colors.Red,
				BackgroundColor = new Color(1.0f, 1.0f, 0.0f),
				Content = new Label
				{
					AutomationId = "TestLabel",
					Text = "I should have red text",
					TextColor = Colors.Red,
					BackgroundColor = new Color(0.5f, 0.5f, 0.5f),
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				}
			};
		}
	}
}
