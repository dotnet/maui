using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
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

			carousel.PropertyChanged += (sender, args) => {
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
		public void CarouselViewPositionFromVisibilityChangeTest()
		{
			RunningApp.WaitForElement(q => q.Marked("AppearButton"));
			RunningApp.Tap(q => q.Marked("AppearButton"));
			RunningApp.WaitForElement("The test has passed");
			RunningApp.Screenshot("Success");
		}
#endif

	}
}