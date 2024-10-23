namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 34061, "RelativeLayout - First child added after page display does not appear")]
	public class Bugzilla34061 : TestContentPage
	{
		Microsoft.Maui.Controls.Compatibility.RelativeLayout _layout;

		protected override void Init()
		{
			_layout = new Microsoft.Maui.Controls.Compatibility.RelativeLayout();
			var label = new Label { Text = "Some content goes here", HorizontalOptions = LayoutOptions.Center };

			var addButton = new Button { Text = "Add Popover", AutomationId = "btnAdd" };
			addButton.Clicked += (s, ea) => AddPopover();

			var stack = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children = {
					label,
					addButton
				},
			};

			_layout.Children.Add(stack,
				Microsoft.Maui.Controls.Compatibility.Constraint.Constant(0),
				Microsoft.Maui.Controls.Compatibility.Constraint.Constant(0),
				Microsoft.Maui.Controls.Compatibility.Constraint.RelativeToParent(p => p.Width),
				Microsoft.Maui.Controls.Compatibility.Constraint.RelativeToParent(p => p.Height));

			Content = _layout;
		}

		void AddPopover()
		{
			var newView = new Button
			{
				BackgroundColor = Color.FromRgba(64, 64, 64, 64),
				Text = "Remove Me",
				AutomationId = "btnRemoveMe"
			};
			newView.Clicked += (s, ea) => RemovePopover(newView);

			_layout.Children.Add(
				newView,
				Microsoft.Maui.Controls.Compatibility.Constraint.Constant(0),
				Microsoft.Maui.Controls.Compatibility.Constraint.RelativeToParent(p => p.Height / 2),
				Microsoft.Maui.Controls.Compatibility.Constraint.RelativeToParent(p => p.Width),
				Microsoft.Maui.Controls.Compatibility.Constraint.RelativeToParent(p => p.Height / 2));
		}

		void RemovePopover(View view)
		{
			_layout.Children.Remove(view);
		}
	}
}
