using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1544, "StackLayout with zero spacing is not always zero spacing ", PlatformAffected.Android | PlatformAffected.WPF)]
	public class Issue1544 : TestContentPage
	{
		protected override void Init()
		{
			var colors = new[] {
				Color.FromHex("#433DBA"),
				Color.FromHex("#6461B7")
			};
			var layout = new StackLayout()
			{
				Spacing = 0,
				Children =
				{
					new Label()
					{
						BackgroundColor = colors[1],
						HeightRequest = 55.7
					}
				}
			};
			for (int i = 0; i < 40; i++)
			{
				layout.Children.Add(new BoxView()
				{
					BackgroundColor = colors[i % 2],
					HeightRequest = 10
				});
			}
			Content = layout;
		}
	}
}