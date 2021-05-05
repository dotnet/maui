using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6556, "Button.CornerRadius doesn't work on WPF", PlatformAffected.WPF)]
	public class Issue6556 : TestContentPage
	{
		protected override void Init()
		{
			var sl = new StackLayout { Padding = new Size(20, 20) };
			sl.Children.Add(new TestButton(0));
			sl.Children.Add(new TestButton(5));
			sl.Children.Add(new TestButton(10));
			sl.Children.Add(new TestButton(20));
			sl.Children.Add(new Button()
			{
				Text = "Round",
				WidthRequest = 50,
				HeightRequest = 50,
				CornerRadius = 25,
				BorderWidth = 5,
				HorizontalOptions = LayoutOptions.Center
			});
			Content = sl;
		}

		class TestButton : Button
		{
			public TestButton(int cr)
			{
				Text = $"radius is {cr}";
				BorderWidth = 5;
				CornerRadius = cr;
			}
		}
	}
}