using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5951, "App Crashes On Shadow Effect's OnDetached On Button That's Never Visible", PlatformAffected.iOS)]
	public class Issue5951 : TestContentPage
	{
		protected override void Init()
		{
			var instructionsLabel = new Label { Text = "Press the push page button. If everything works, you'll see the word success below.", FontSize = 16 };

			var resultsLabel = new Label { Text = string.Empty, FontSize = 16 };

			var pushButton = new Button
			{
				Text = "Push page",
				Command = new Command(async () =>
				{
					var shadowPage = new PageWithShadowButton();

					await Navigation.PushAsync(shadowPage);

					try
					{
						await shadowPage.Navigation.PopAsync();

						resultsLabel.Text = "Success";
					}
					catch (NullReferenceException)
					{
						resultsLabel.Text = "Error";
					}
				})
			};

			Content = new StackLayout
			{
				Children = {
					instructionsLabel,
		  			resultsLabel,
					pushButton
				}
			};


		}

#if UITEST
		[Test]
		public void Issue5951Test()
		{
			RunningApp.Tap(q => q.Marked("Push page"));
			RunningApp.WaitForElement(q => q.Marked("Push page"));

			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class PageWithShadowButton : ContentPage
	{
		public PageWithShadowButton()
		{
			var shadowButton = new Button { Text = "Never Visible", IsVisible = false };

			shadowButton.On<iOS>()
				.SetIsShadowEnabled(true)
				.SetShadowColor(Color.Black)
				.SetShadowOffset(new Size(10, 10))
				.SetShadowOpacity(0.2);

			Content = new StackLayout
			{
				Children = { shadowButton }
			};
		}
	}


}
