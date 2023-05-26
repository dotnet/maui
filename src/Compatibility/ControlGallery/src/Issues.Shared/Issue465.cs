using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 465, "Change in Navigation.PushModal", PlatformAffected.All)]
	public class Issue465 : TestTabbedPage
	{
		protected override async void Init()
		{
			Children.Add(
				new ContentPage
				{
					Content = new Label
					{
						Text = "I was popppppped"
					}
				}
			);

			await Navigation.PushModalAsync(new ModalPage());
		}

		[Preserve(AllMembers = true)]
		public class ModalPage : ContentPage
		{
			public ModalPage()
			{
				var popButton = new Button
				{
					Text = "Pop this page"
				};
				popButton.Clicked += (s, e) => Navigation.PopModalAsync();

				Content = popButton;
			}
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void Issue465TestsPushPopModal ()
		{
			RunningApp.WaitForElement (q => q.Button ("Pop this page"));
			RunningApp.Screenshot ("All elements exist");

			RunningApp.Tap (q => q.Button ("Pop this page"));
			RunningApp.WaitForElement (q => q.Marked ("I was popppppped"));
			RunningApp.Screenshot ("Popped modal successful");
		}
#endif
	}
}
