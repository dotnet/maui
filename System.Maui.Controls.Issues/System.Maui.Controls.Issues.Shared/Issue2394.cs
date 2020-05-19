using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2394, "[WPF] StackLayout VerticalOptions = LayoutOptions.End is not working", PlatformAffected.WPF)]
	public class Issue2394 : TestContentPage
	{
		protected override void Init()
		{
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var stack = new StackLayout { VerticalOptions = LayoutOptions.End };
			Button b1 = new Button { Text = "Boring", HeightRequest = 100, MinimumHeightRequest = 50 };
			Button b2 = new Button
			{
				Text = "Exciting!",
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			Button b3 = new Button { Text = "Amazing!", VerticalOptions = LayoutOptions.FillAndExpand };
			Button b4 = new Button { Text = "Meh", HeightRequest = 100, MinimumHeightRequest = 50 };
			b1.Clicked += (sender, e) => {
				b1.Text = "clicked1";
			};
			b2.Clicked += (sender, e) => {
				b2.Text = "clicked2";
			};
			b3.Clicked += (sender, e) => {
				b3.Text = "clicked3";
			};
			b4.Clicked += (sender, e) => {
				b4.Text = "clicked4";
			};
			stack.Children.Add(b1);
			stack.Children.Add(b2);
			stack.Children.Add(b3);
			stack.Children.Add(b4);
			Content = stack;
		}
	}
}
