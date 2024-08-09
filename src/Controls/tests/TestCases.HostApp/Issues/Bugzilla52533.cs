using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 52533, "System.ArgumentException: NaN is not a valid value for width", PlatformAffected.iOS)]
	public class Bugzilla52533 : TestContentPage
	{
		const string LabelId = "label";

		protected override void Init()
		{
			Content = new ListView { ItemTemplate = new DataTemplate(typeof(GridViewCell)), ItemsSource = Enumerable.Range(0, 10) };
		}

		[Preserve(AllMembers = true)]
		class GridViewCell : ViewCell
		{

			public GridViewCell()
			{
				var grid = new Grid
				{
					// Multiple rows
					RowDefinitions = {
						new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) },
						new RowDefinition { Height = new GridLength(150, GridUnitType.Absolute) }
					},
					// Dynamic width
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
					}
				};

				// Infinitely wide + Label
				var horStack = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { new Label { Text = "If this does not crash, this test has passed.", AutomationId = LabelId } } };
				grid.Add(horStack, 0, 0);

				View = grid;
			}
		}
	}
}
