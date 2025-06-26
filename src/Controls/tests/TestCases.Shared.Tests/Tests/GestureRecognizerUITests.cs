﻿using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class GestureRecognizerUITests : CoreGalleryBasePageTest
	{
		const string GestureRecognizerGallery = "Gesture Recognizer Gallery";
		public GestureRecognizerUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GestureRecognizerGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void PointerGestureTest()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "PointerGestureRecognizerEvents");
			App.Tap("GoButton");

			App.WaitForElement("primaryLabel");
			// using Tap in place of moving mouse for now
			App.Tap("primaryLabel");
			App.Tap("secondaryLabel");
			App.WaitForElement("secondaryLabel");
			var secondaryLabelText = App.FindElement("secondaryLabel").GetText();
			Assert.That(secondaryLabelText, Is.Not.Null);
		}

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void DoubleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DoubleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("DoubleTapSurface");
			App.DoubleTap("DoubleTapSurface");

			var result = App.FindElement("DoubleTapResults").GetText();
			Assert.Equal("Success", result);
		}

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void SingleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "SingleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("SingleTapSurface");
			App.Tap("SingleTapSurface");

			var result = App.FindElement("SingleTapGestureResults").GetText();
			Assert.Equal("Success", result);
		}

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void DisabledSingleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "SingleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("DisabledTapSurface");
			App.Tap("DisabledTapSurface");

			var result = App.FindElement("DisabledTapGestureResults").GetText();
			Assert.NotEqual("Failed", result);
		}

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void DynamicallyAddedTapGesturesDontCauseMultipleTapEvents()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DynamicTapGestureGallery");
			App.Tap("GoButton");

			App.WaitForElement("DynamicTapSurface");
			App.Tap("DynamicTapSurface");
			App.Tap("DynamicTapSurface");
			App.Tap("DynamicTapSurface");

			var result = App.FindElement("DynamicTapGestureResults").GetText();

			if (int.TryParse(result, out var resultInt))
			{
				Assert.Equal(3, resultInt);
			}
			else
			{
				throw new InvalidOperationException("Failed to parse result as int");
			}
		}
	}
}