using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	// Manual test to make sure diagonal scrolling works at the correct speed 

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60774, "[Android] ScrollOrientation.Both doubles the distance of scrolling", 
		PlatformAffected.Android, issueTestNumber: 2)]
	public class Bugzilla60774_2 : TestContentPage
	{
		protected override void Init()
		{
			Title = "ScrollOrientation Both";

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition()
				}
			};

			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Move the label around. It should be able to move in any direction, with uniform speed. " 
						+ "If it moves twice as fast vertically as horizontally or vice versa, the test has failed. " 
						+ "If the label cannot move diagonally, the test has failed."
			};

			layout.Children.Add(instructions);
			grid.Children.Add(layout);
			
			var host = new Grid();
			Grid.SetRow(host, 1);
			
			grid.Children.Add(host);

			Content = grid;

			var al = new AbsoluteLayout();
			
			var label = new Label { Text = "Move this label around", FontSize = 72, Margin = 300 };
			AbsoluteLayout.SetLayoutBounds(label, new Rectangle(0, 0, 2000, 2000));
			al.Children.Add(label);

			var sv = new ScrollView
			{
				Orientation = ScrollOrientation.Both,
				Content = al
			};

			host.Children.Add(sv);
		}
	}
}