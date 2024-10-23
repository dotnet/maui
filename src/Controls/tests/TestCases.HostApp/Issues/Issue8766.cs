namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 8766, "[Bug] CollectionView.EmptyView does not inherit parent Visual", PlatformAffected.All)]
	public class Issue8766 : TestContentPage
	{
		protected override void Init()
		{
			//Visual = VisualMarker.Material;

			var layout = new StackLayout() { AutomationId = "TestReady" };

			var instructions = new Label { Text = "If the Entry and Button above the CollectionView and the Entry and Button inside the CollectionView, should both be using the Material Visual. If so, this test has passed." };
			layout.Children.Add(instructions);

			var entry = new Entry { Placeholder = "I am material" };
			var button = new Button { Text = "I am material" };
			layout.Children.Add(entry);
			layout.Children.Add(button);

			var colv = new CollectionView() { };

			var emptyViewEntry = new Entry { Placeholder = "I should be material" };
			var emptyViewButton = new Button { Text = "I should be material, too" };
			var stack = new StackLayout { Children = { emptyViewEntry, emptyViewButton } };

			colv.EmptyView = stack;
			layout.Children.Add(colv);

			Content = layout;
		}
	}
}
