﻿#if ANDROID // Crash only happened on Android
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26328 : _IssuesUITest
	{
		public override string Issue => "SwipeView causes Java.Lang.IllegalArgumentException: Cannot add a null child view to a ViewGroup";

		public Issue26328(TestDevice device)
		: base(device)
		{ }

		[Fact]
		[Trait("Category", UITestCategories.SwipeView)]
		public void NoCrashRemovingSwipeItems()
		{
			App.WaitForElement("TestCollectionView");

			for (int i = 0; i < 10; i++)
			{
				App.SwipeRightToLeft();
				App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture, swipePercentage: 0.2);
				App.ScrollUp("TestCollectionView", ScrollStrategy.Gesture, swipePercentage: 0.2);
			}
		}
	}
}
#endif