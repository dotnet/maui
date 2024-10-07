using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38723, "Update Content in Picker's SelectedIndexChanged event causes NullReferenceException", PlatformAffected.All)]
	public class Bugzilla38723 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				Text = "NoSelected"
			};

			var picker = new Picker
			{
				Title = "Options",
				ItemsSource = new[] { "option1", "option2", "option3" }
			};

			picker.SelectedIndexChanged += (sender, args) =>
			{
				label.Text = "Selected";
				Content = label;
			};

			var button = new Button
			{
				AutomationId = "SELECT",
				Text = "SELECT"
			};

			button.Clicked += (sender, args) =>
			{
				picker.SelectedIndex = 0;
			};

			Content = new StackLayout
			{
				Children =
				{
					label,
					picker,
					button
				}
			};
		}
	}
}
