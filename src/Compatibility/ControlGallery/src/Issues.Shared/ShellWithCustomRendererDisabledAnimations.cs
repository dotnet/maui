using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "[Shell] Overriding animation with custom renderer to remove animation breaks next navigation",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellWithCustomRendererDisabledAnimations : TestShell
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage();
			base.AddFlyoutItem(contentPage, "Root");

			contentPage.Content = new Button()
			{
				Text = "Click Me",
				AutomationId = "PageLoaded",
				Command = new Command(async () =>
				{
					await Navigation.PushAsync(CreateSecondPage());
				})
			};
		}

		ContentPage CreateSecondPage()
		{
			ContentPage page = new ContentPage();

			page.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If clicking `Go Back` goes back to previous page then test has passed"
					},
					new Button()
					{
						Text = "Go Back",
						Command = new Command(async () =>
						{
							await GoToAsync("..");
						}),
						AutomationId = "GoBack"
					}
				}
			};

			return page;
		}

#if UITEST && __ANDROID__
		[Test]
		public void NavigationWithACustomRendererThatDoesntSetAnAnimationStillWorks()
		{
			RunningApp.Tap("PageLoaded");
			RunningApp.Tap("GoBack");
			RunningApp.WaitForElement("PageLoaded");
		}
#endif
	}
}
