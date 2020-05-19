using System;
using System.Collections.Generic;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8782, "[Bug] SwipeViewItems cut off on one or more sides", PlatformAffected.All)]
	public partial class Issue8782 : TestContentPage
	{
		public Issue8782()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.SwipeViewExperimental });
			Title = "Issue 8782";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

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