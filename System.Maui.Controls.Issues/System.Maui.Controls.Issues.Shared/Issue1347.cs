using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1347, "[Android] Frame outline color not rendering", PlatformAffected.Android)]
	public class Issue1347 : TestContentPage
	{
		Label label;
		Frame frame;
		protected override void Init()
		{
			label = new Label() { Text = "The ouline color should be red", VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.CenterAndExpand };
			frame = new Frame
			{
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 300,
				WidthRequest = 200,
				BorderColor = Color.Red,
				BackgroundColor = Color.LightBlue,
				HasShadow = true,
				Content = label
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) => {
				frame.BorderColor = frame.BorderColor == Color.Default ? Color.Red : Color.Default;
				frame.BackgroundColor = frame.BackgroundColor == Color.Default ? Color.LightBlue : Color.Default;
				label.Text = frame.BorderColor == Color.Default ? "The ouline color should be default (click here to change color)" : "The ouline color should be red (click here to change color)";
			};
			frame.GestureRecognizers.Add(tapGestureRecognizer);

			Content = frame;
		}
	}
}
