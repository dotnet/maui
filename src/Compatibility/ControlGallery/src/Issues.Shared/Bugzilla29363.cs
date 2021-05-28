using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif


namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.LifeCycle)]
	[Category(UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29363, "PushModal followed immediate by PopModal crashes")]
	public class Bugzilla29363 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

			Button modal = new Button
			{
				Text = "Modal Push Pop Test",
				Font = Font.SystemFontOfSize(25, FontWeight.Bold),
				HorizontalOptions = LayoutOptions.Center
			};
			modal.Clicked += async (object sender, EventArgs e) =>
			{
				var page = new ContentPage() { BackgroundColor = Colors.Red };

				await Navigation.PushModalAsync(page);

				await Navigation.PopModalAsync(true);
			};

			layout.Children.Add(modal);
			Content = layout;
		}

#if UITEST
		[Test]
		public void PushButton()
		{
			RunningApp.Tap(q => q.Marked("Modal Push Pop Test"));
			System.Threading.Thread.Sleep(2000);
			// if it didn't crash, yay
			RunningApp.WaitForElement(q => q.Marked("Modal Push Pop Test"));
		}
#endif
	}
}
