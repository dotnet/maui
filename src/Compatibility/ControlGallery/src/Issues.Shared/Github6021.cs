using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6021, "[macOS] ListView does not handle transparent backgrounds correctly", PlatformAffected.macOS)]
	public class Github6201 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			BackgroundColor = Color.Red;

			Content = new ListView
			{
				Margin = new Thickness(20),
				BackgroundColor = Color.FromRgba(0, 0, 1, 0.5),
				ItemsSource = new string[] { "Page background should be red", "ListView background should be blue with 50% alpha and so should appear purple", "[iOS, macOS] Default behavior is to make cells have the same color as the ListView, so the cells should appear 100% blue", "[Other platforms] Cells should be transparent, and appear purple", "If the cells appear pale blue, then the ListView has an extra white background", "If the cells appear dark blue, then the cells have the same blue with 50% alpha as a background", "If the ListView appears dark blue, then background colors with alpha is not supported" },
			};
		}
	}
}