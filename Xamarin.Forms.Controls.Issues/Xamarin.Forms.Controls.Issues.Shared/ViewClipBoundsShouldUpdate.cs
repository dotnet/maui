using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 888888, "Bounds clipping does not update when View bounds change", PlatformAffected.Android)]
	public class ViewClipBoundsShouldUpdate : TestContentPage
	{
		const string Success = "Success";

		class TestContentView : ContentView
		{
			public TestContentView()
			{
				Content = new Label { Text = Success };

				IsClippedToBounds = true;
			}
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = $"If '{Success}' displays below then this test has passed."
					},
					new TestContentView()
				}
			};

			Content = layout;
		}

	}
}