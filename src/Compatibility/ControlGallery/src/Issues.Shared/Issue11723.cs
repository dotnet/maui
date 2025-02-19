using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 11723, "[Bug] ContentPage in NavigationStack misplaced initially",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11723 : TestShell
	{
		int labelIndex = 0;
		ContentPage CreateContentPage()
		{
			var page = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "As you navigate this text should show up in the correct spot. If it's hidden and then shows up this test has failed.",
							AutomationId = $"InitialText{labelIndex}"
						},
						new Button()
						{
							Text = "Push Page",
							AutomationId = "PushPage",
							Command = new Command(async () =>
							{
								labelIndex++;
								await Navigation.PushAsync(CreateContentPage());
							})
						},
						new Button()
						{
							Text = "Pop Page",
							AutomationId = "PopPage",
							Command = new Command(async () =>
							{
								labelIndex--;
								await Navigation.PopAsync();
							})
						}
					}
				}
			};

			SetNavBarIsVisible(page, false);
			PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(page, true);

			return page;
		}

		protected override void Init()
		{
			AddContentPage(CreateContentPage());
		}


#if UITEST
		[Test]
		public void PaddingIsSetOnPageBeforeItsVisible()
		{
			var initialTextPosition = RunningApp.WaitForFirstElement($"InitialText0").Rect;
			RunningApp.Tap("PushPage");
			CompareTextLocation(initialTextPosition, 1);
			RunningApp.Tap("PushPage");
			CompareTextLocation(initialTextPosition, 2);
			RunningApp.Tap("PushPage");
			CompareTextLocation(initialTextPosition, 3);
			RunningApp.Tap("PopPage");
			CompareTextLocation(initialTextPosition, 2);
			RunningApp.Tap("PopPage");
			CompareTextLocation(initialTextPosition, 1);

		}

		void CompareTextLocation(Xamarin.UITest.Queries.AppRect initialRect, int i)
		{
			var newRect = RunningApp.WaitForFirstElement($"InitialText{i}").Rect;

			Assert.AreEqual(newRect.X, initialRect.X, $"Error With Test :{i}");
			Assert.AreEqual(newRect.Y, initialRect.Y, $"Error With Test :{i}");
			Assert.AreEqual(newRect.CenterX, initialRect.CenterX, $"Error With Test :{i}");
			Assert.AreEqual(newRect.CenterY, initialRect.CenterY, $"Error With Test :{i}");
		}
#endif
	}
}
