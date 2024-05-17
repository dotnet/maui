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
	[Issue(IssueTracker.Github, 3509, "[iOS] NavigationPage.Popped called twice when Navigation.PopAsync is called",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Issue3509 : TestNavigationPage
	{
		const string _popPage = "Pop Page";
		protected override void Init()
		{
			int popCount = 0;
			Label label = new Label();

			PushAsync(new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label(){ Text = "If the number below is not a one test has failed"},
						label,
						new Button()
						{
							Text = "Push a Page",
							Command = new Command(() =>
							{
								PushAsync(new TestPage());
							})
						}
					}
				}
			});

			PushAsync(new TestPage());
			Popped += (s, e) =>
			{
				popCount++;
				label.Text = $"{popCount}";
			};
		}

		[Preserve(AllMembers = true)]
		public class TestPage : ContentPage
		{
			bool _popped = false;

			public TestPage()
			{
				Title = "Test page";
				var content = new StackLayout();
				content.Children.Add(new Button
				{
					Text = _popPage,
					Command = new Command(() =>
					{
						Navigation.PopAsync(false);
					}),
				});

				Content = content;
			}

			internal void Popped()
			{
				if (_popped)
					throw new Exception("Already popped");

				_popped = true;
			}
		}

#if UITEST
		[Test]
		public void PoppedOnlyFiresOnce()
		{
			RunningApp.WaitForElement(_popPage);
			RunningApp.Tap(_popPage);
			RunningApp.WaitForElement("1");
		}
#endif
	}
}
