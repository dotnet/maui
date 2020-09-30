using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5461, "[Android] ScrollView crashes when setting ScrollbarFadingEnabled to false in Custom Renderer",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ScrollView)]
#endif
	public class Issue5461 : TestContentPage
	{
		const string Success = "If you can see this, the test has passed";
		protected override void Init()
		{
			ScrollView scrollView = new ScrollbarFadingEnabledFalseScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = Success
						}
					},
					HeightRequest = 2000
				}
			};

			Content = scrollView;
		}

		public class ScrollbarFadingEnabledFalseScrollView : ScrollView { }


#if UITEST && __ANDROID__
		[Test]
		public void ScrollViewWithScrollbarFadingEnabledFalseDoesntCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
