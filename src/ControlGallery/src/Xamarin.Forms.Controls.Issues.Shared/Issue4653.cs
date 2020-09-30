using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4653, "IsTabStop property not working when Grid contains ScrollView", PlatformAffected.UWP)]
	public class Issue4653 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid()
			{
				Padding = new Thickness(10),
				BackgroundColor = Color.Aquamarine,
				IsTabStop = false
			};
			grid.AddChild(new Button
			{
				Text = "Toggle Grid IsTabStop",
				Command = new Command(() => grid.IsTabStop = !grid.IsTabStop)
			}, 0, 0);
			grid.AddChild(new Button
			{
				Text = "IsTabStop: false",
				IsTabStop = false
			}, 1, 0);

			var buttonInSctoolView = new Button
			{
				Text = "Button inside tadded ScrollView",
				IsTabStop = false
			};
			buttonInSctoolView.Command = new Command(() =>
			{
				buttonInSctoolView.IsTabStop = !buttonInSctoolView.IsTabStop;
				buttonInSctoolView.Text = $"IsTabStop: {buttonInSctoolView.IsTabStop}";
			});

			grid.AddChild(new ScrollView
			{
				Content = buttonInSctoolView
			}, 0, 1);

			grid.AddChild(new Entry
			{
				Text = "entry"
			}, 1, 1);

			int col = 2;
			grid.AddChild(new Button
			{
				Text = "Add default button",
				IsTabStop = false,
				Command = new Command(() => grid.AddChild(new Button { Text = "default" }, 0, ++col))
			}, 0, 2);

			int colNonTabbed = 2;
			grid.AddChild(new Button
			{
				Text = "Add non tabbed button",
				IsTabStop = true,
				Command = new Command(() => grid.AddChild(new Button { Text = "non tabbed", IsTabStop = false }, 1, ++colNonTabbed))
			}, 1, 2);

			Content = new StackLayout
			{
				Children =
				{
					grid
				}
			};
		}
	}
}