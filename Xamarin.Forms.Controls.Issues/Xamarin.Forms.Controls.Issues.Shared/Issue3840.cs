using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3840, "[iOS] Translation change causes ScrollView to reset to initial position (0, 0)",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ScrollView)]
#endif
	public class Issue3840 : TestContentPage
	{
		const string _failedText = "Test Failed if Visible";
		const string _button1 = "FirstClick";
		const string _button2 = "SecondClick";

		protected override void Init()
		{
			ScrollView scroll = null;
			scroll = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						new Label()
						{
							Text = _failedText
						},
						new Button()
						{
							Text = "Click Me First",
							AutomationId = _button1,
							Command = new Command(async () =>
							{
								await scroll.ScrollToAsync(0, 100, true);
							}),
							HorizontalOptions = LayoutOptions.Start
						},
						new BoxView { Color = Color.Red, HeightRequest = 500 },
						new Button()
						{
							Text = "Click Me Second",
							AutomationId = _button2,
							Command = new Command(async () =>
							{
								scroll.TranslationX = 100;
								await Task.Delay(100);
								// using one because of a bug on UWP that doesn't react to being set back to zero
								scroll.TranslationX = 1;

							}),
							HorizontalOptions = LayoutOptions.Start
						},
						new BoxView { Color = Color.Gray, HeightRequest = 500 },
						new BoxView { Color = Color.Yellow, HeightRequest = 500 }
					}
				}
			};

			var mainLayout = new AbsoluteLayout();
			mainLayout.Children.Add(scroll, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
			Content = mainLayout;
		}


#if UITEST
		[Test]
		public void TranslatingViewKeepsScrollViewPosition()
		{
			RunningApp.WaitForElement(_failedText);
			RunningApp.Tap(_button1);
			RunningApp.Tap(_button2);
			RunningApp.WaitForNoElement(_failedText);
		}
#endif
	}
}
