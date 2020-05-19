using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif


namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.LifeCycle)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29363, "PushModal followed immediate by PopModal crashes")]
    public class Bugzilla29363 : TestContentPage
    {
		protected override void Init ()
		{
			var layout = new StackLayout () {HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand};

			Button modal = new Button {
				Text = "Modal Push Pop Test",
				Font = Font.SystemFontOfSize (25, FontAttributes.Bold),
				HorizontalOptions = LayoutOptions.Center
			};
			modal.Clicked += async (object sender, EventArgs e) => {
				var page = new ContentPage () {BackgroundColor = Color.Red};

				await Navigation.PushModalAsync (page);

				await Navigation.PopModalAsync (true);
			};

			layout.Children.Add (modal);
			Content = layout;
		}

#if UITEST
		[Test]
		public void PushButton ()
		{
			RunningApp.Tap (q => q.Marked ("Modal Push Pop Test"));
			System.Threading.Thread.Sleep (2000);
			// if it didn't crash, yay
			RunningApp.WaitForElement(q => q.Marked("Modal Push Pop Test"));
		}
#endif
    }
}
