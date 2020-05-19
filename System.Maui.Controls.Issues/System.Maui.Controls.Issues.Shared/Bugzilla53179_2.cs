using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53179, "Removing page during OnAppearing throws exception", PlatformAffected.Android,
		issueTestNumber: 2)]
	public class Bugzilla53179_2 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			Appearing += async (sender, args) =>
			{
				var nav = new NavigationPage(Root());
				Application.Current.MainPage = nav;
				await nav.PushAsync(Intermediate());
				await nav.PushAsync(new PageWhichRemovesAnEarlierPageOnAppearing());	
			};
		}

		static ContentPage Root()
		{
			return new ContentPage { Content = new Label {Text = "Root"} };
		}

		static ContentPage Intermediate()
		{
			return new ContentPage { Content = new Label {Text = "Intermediate page"} };
		}

		[Preserve(AllMembers = true)]
		class PageWhichRemovesAnEarlierPageOnAppearing : ContentPage
		{
			public PageWhichRemovesAnEarlierPageOnAppearing()
			{
				var instructions = new Label { Text = "If you can see this, the test has passed" };

				Content = new StackLayout { Children = { instructions, new Label { Text = Success } } };
			}

			protected override void OnAppearing()
			{
				var toRemove = Navigation.NavigationStack.Skip(1).First();

				// toRemove should be the IntermediatePage
				Navigation.RemovePage(toRemove);

				base.OnAppearing();
			}
		}

#if UITEST
		[Test]
		public void RemovePageOnAppearingDoesNotCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}