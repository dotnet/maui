using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1236, "Label binding", PlatformAffected.iOS)]
	public class Issue1236 : TestContentPage
	{
		protected override void Init()
		{
			Content = new Label { HeightRequest = 30, WidthRequest = 200, BackgroundColor = Colors.Purple.WithLuminosity(.7f) };
			Content.SetBinding(Label.TextProperty, ".");

			DelayUpdatingBindingContext();
		}

		async void DelayUpdatingBindingContext()
		{
			await Task.Delay(2000);
			BindingContext = "Lorem Ipsum Dolor Sit Amet";
		}
	}
}