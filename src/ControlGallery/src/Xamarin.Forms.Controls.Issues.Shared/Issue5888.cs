using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5888, "[Bug] ListView HasUnevenRows is not working in iOS 10", PlatformAffected.iOS)]
	public class Issue5888 : TestContentPage
	{
		protected override void Init()
		{

			var stack2 = new StackLayout
			{
				Children = { new Label { Text = "Hi" }, new Label { Text = "Bye" }, new Label { Text = "Open" } }
			};
			var listview = new ListView
			{
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate(() => new ViewCell { View = stack2 }),
				ItemsSource = new string[] { "mono", "monodroid" }
			};
			Content = new StackLayout { Children = { listview } };
		}
	}
}