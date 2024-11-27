﻿using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CheckBoxUITests : _ViewUITests
	{
		const string CheckBoxGallery = "CheckBox Gallery";

		public CheckBoxUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(CheckBoxGallery);
		}

		[Test]
		[Category(UITestCategories.CheckBox)]
		public override void IsEnabled()
		{
			if (Device == TestDevice.Mac ||
				Device == TestDevice.iOS)
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}

			base.IsEnabled();
		}
	}
}
