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
	[Issue(IssueTracker.Github, 1323, "tabbed page BarTextColor is not pervasive and can't be applied after instantiation", PlatformAffected.iOS)]
	public class Issue1323 : TestTabbedPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			BarBackgroundColor = Color.FromArgb("#61a60e");
			BarTextColor = Color.FromArgb("#ffffff");
			BackgroundColor = Color.FromArgb("#61a60e");

			var page = new ContentPage { Title = "Page 1", Content = new Button { Text = "Pop", Command = new Command(async () => await Navigation.PopModalAsync()) } };
			var page2 = new ContentPage { Title = "Page 2" };
			var page3 = new ContentPage { Title = "Page 3" };
			var page4 = new ContentPage { Title = "Page 4" };

			Children.Add(page);
			Children.Add(page2);
			Children.Add(page3);
			Children.Add(page4);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			BarTextColor = Colors.White;
			Children.RemoveAt(1);
			Children.Insert(1, new ContentPage { Title = "Page5", IconImageSource = "Loyalty.png" });

			Children.RemoveAt(3);
			Children.Insert(2, new ContentPage { Title = "Page6", IconImageSource = "Gift.png" });
			BarTextColor = Colors.White;
		}

#if UITEST
		[Test]
		public void Issue1323Test()
		{
			RunningApp.WaitForElement(X => X.Marked("Page 1"));
			RunningApp.WaitForElement(X => X.Marked("Page5"));
			RunningApp.Screenshot("All tab bar items text should be white");
		}
#endif
	}
}