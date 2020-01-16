using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.CustomAttributes;
using System;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8781, "SwipeViewItem rendering issue on iOS", PlatformAffected.iOS)]
	public partial class Issue8781 : ContentPage
	{
		public Issue8781()
		{
#if APP
			InitializeComponent();
#endif
		}

		async void OnIncorrectAnswerInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("Incorrect!", "Try again.", "OK");
		}

		async void OnCorrectAnswerInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("Correct!", "The answer is 2.", "OK");
		}
	}
}