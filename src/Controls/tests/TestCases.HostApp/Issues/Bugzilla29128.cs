using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
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
