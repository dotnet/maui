using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39458, "[UWP/WinRT] Cannot Set CarouselPage.CurrentPage Inside Constructor", PlatformAffected.WinRT)]
	public class Bugzilla39458 : TestCarouselPage
	{
		public class ChildPage : ContentPage
		{
			public ChildPage(int pageNumber)
			{
				var layout = new StackLayout();
				var MyLabel = new Label {
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 21,
					TextColor = Color.White,
					Text = $"This is page {pageNumber}"
				};
				var TestBtn = new Button {
					Text = "Go to Page 2",
					IsEnabled = false,
					BackgroundColor = Color.White
				};

				if (pageNumber != 2)
				{
					TestBtn.IsEnabled = true;
					TestBtn.Clicked += TestBtn_Clicked;
				}

				layout.Children.Add(MyLabel);
				layout.Children.Add(TestBtn);
				Content = layout;
			}

			private void TestBtn_Clicked(object sender, EventArgs e)
			{
				var carousel = Application.Current.MainPage as CarouselPage;
				carousel.CurrentPage = carousel.Children[1];
			}
		}

		public class DesiredPage : ChildPage
		{
			public DesiredPage(int pageNumber) : base(pageNumber)
			{
			}
		}

		protected override void Init()
		{
			var firstPage = new ChildPage(1);
			firstPage.BackgroundColor = Color.Blue;
			Children.Add(firstPage);

			var secondPage = new DesiredPage(2);
			secondPage.BackgroundColor = Color.Red;
			Children.Add(secondPage);

			var thirdPage = new ChildPage(3);
			thirdPage.BackgroundColor = Color.Green;
			Children.Add(thirdPage);

			CurrentPage = secondPage;
		}

#if UITEST
		[Test]
		public void Bugzilla39458Test()
		{
			RunningApp.WaitForElement(q => q.Marked("This is page 2"));
		}
#endif
	}
}
