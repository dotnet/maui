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
	[Issue(IssueTracker.Github, 3884, "BoxView corner radius", PlatformAffected.Android)]
	public class Issue3884 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var label = new Label { Text = "You should see a blue circle" };
			var box = new BoxView
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Blue,
				HeightRequest = 100,
				WidthRequest = 100,
				CornerRadius = 50
			};

			Content = new StackLayout
			{
				Children = { label,box}
			};
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue3884Test() 
		{
			RunningApp.Screenshot ("I see a blue circle");
		}
#endif
	}
}