using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53362, "Layout regression in Grid on iOS: HorizontalOption = Center does not center", PlatformAffected.iOS)]
	public class Bugzilla53362 : TestContentPage
	{
		protected override void Init()
		{
			var label1 = new Label { Text = "auto sized row", TextColor = Color.Silver, HorizontalOptions = LayoutOptions.Center, BackgroundColor = Color.Purple };
			var label2 = new Label { Text = "row size 20", TextColor = Color.Silver, HorizontalOptions = LayoutOptions.Center, BackgroundColor = Color.Purple };
			var label3 = new Label { Text = "row size 25", TextColor = Color.Silver, HorizontalOptions = LayoutOptions.Center, BackgroundColor = Color.Purple };

			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(25, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
				}
			};

			grid.Children.Add(label1, 0, 0);
			grid.Children.Add(label2, 0, 1);
			grid.Children.Add(label3, 0, 2);
			grid.Children.Add(new Label { Text = "If the three labels above are not all centered horizontally, this test has failed." }, 0, 3);

			Content = grid;
		}
	}
}