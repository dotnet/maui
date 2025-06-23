namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 29128, "Slider background lays out wrong Android")]
	public class Bugzilla29128 : TestContentPage
	{
		protected override void Init()
		{
			Content = new Slider
			{
				AutomationId = "SliderId",
				BackgroundColor = Colors.Blue,
				Maximum = 255,
				Minimum = 0,
			};
		}
	}
}
