using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2394, "[WPF] StackLayout VerticalOptions = LayoutOptions.End is not working", PlatformAffected.WPF)]
	public class Issue2394 : TestContentPage
	{
		protected override void Init()
		{
			if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet)
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
			b1.Clicked += (sender, e) =>
			{
				b1.Text = "clicked1";
			};
			b2.Clicked += (sender, e) =>
			{
				b2.Text = "clicked2";
			};
			b3.Clicked += (sender, e) =>
			{
				b3.Text = "clicked3";
			};
			b4.Clicked += (sender, e) =>
			{
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
