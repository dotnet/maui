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
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 48236, "[WinRT/UWP] BackgroundColor for Stepper behaves differently compared to iOS to Android", PlatformAffected.WinRT)]
	public class Bugzilla48236 : TestContentPage
	{
		protected override void Init()
		{
			var stepper = new Stepper
			{
				BackgroundColor = Colors.Green,
				Minimum = 0,
				Maximum = 10
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "If the Stepper's background color extends the width of the page, then this test has failed."
					},
					stepper,
					new Button
					{
						BackgroundColor = Colors.Aqua,
						Text = "Change Stepper Color to Yellow",
						Command = new Command(() =>
						{
							stepper.BackgroundColor = Colors.Yellow;
						})
					}
				}
			};
		}
	}
}