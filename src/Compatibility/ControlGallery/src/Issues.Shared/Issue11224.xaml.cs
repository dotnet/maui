using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
	[Category(UITestCategories.UwpIgnore)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11224, "[Bug] CarouselView Position property fails to update visual while control isn't visible", PlatformAffected.Android)]
	public partial class Issue11224 : TestContentPage
	{
		public Issue11224()
		{
#if APP
			Title = "Issue 11224";
			InitializeComponent();

			carousel.Scrolled += (sender, args) =>
			{
				if (args.CenterItemIndex == 3)
					ResultLabel.Text = "The test has passed";
				else
					ResultLabel.Text = "The test has failed";
			};

			carousel.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == CarouselView.IsVisibleProperty.PropertyName)
				{
					if (carousel.IsVisible && carousel.Position == 3)
					{
						ResultLabel.Text = "The test has passed";
					}
				}
			};
#endif
		}

		protected override void Init()
		{

		}


#if APP
		void Button_Clicked(object sender, EventArgs e)
		{
			carousel.IsVisible = true;
		}
#endif

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CarouselViewPositionFromVisibilityChangeTest()
		{
			RunningApp.WaitForElement(q => q.Marked("AppearButton"));
			RunningApp.Tap(q => q.Marked("AppearButton"));
			RunningApp.WaitForElement("Item 4");
			RunningApp.Screenshot("Success");
		}
#endif

	}
}