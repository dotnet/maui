using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44944, "iOS: Text goes outside the bounds of Entry if it can't fit inside", PlatformAffected.iOS)]
	public class Bugzilla44944 : TestContentPage
	{
		protected override void Init()
		{
			Content = new Grid
			{
				Children =
				{
					new Label
					{
						Text = @"Tap the Entry, type some text, and type anywhere on the screen to dismiss the keyboard. Even though it has a large fontsize (200), the text should not go outside the bounds of the Entry. Instead, it should be clipped by the Entry.",
						VerticalOptions = LayoutOptions.Start
					},
					new Entry
					{
						FontSize = 200,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			};
		}
	}
}
