using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
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
				BorderColor = Colors.Red,
				BackgroundColor = Colors.LightBlue,
				HasShadow = true,
				Content = label
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				frame.BorderColor = frame.BorderColor == null ? Colors.Red : null;
				frame.BackgroundColor = frame.BackgroundColor == null ? Colors.LightBlue : null;
				label.Text = frame.BorderColor == null ? "The ouline color should be default (click here to change color)" : "The ouline color should be red (click here to change color)";
			};
			frame.GestureRecognizers.Add(tapGestureRecognizer);

			Content = frame;
		}
	}
}
