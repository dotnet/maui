using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1538, "Crash measuring empty ScrollView", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1538 : TestContentPage
	{
		ScrollView _sv;

		protected override void Init()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			StackLayout sl = new StackLayout() { VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete
			sl.Children.Add(_sv = new ScrollView() { HeightRequest = 100 });
			Content = sl;

			AddContentDelayed();
		}

		async void AddContentDelayed()
		{
			await Task.Delay(1000);
			_sv.Content = new Label { Text = "Foo" };
		}
	}
}
