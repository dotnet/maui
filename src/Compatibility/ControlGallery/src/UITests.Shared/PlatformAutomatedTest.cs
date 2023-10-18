using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests.Shared
{
	internal class PlatformAutomatedTest : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			//App.NavigateToGallery(GalleryQueries.PlatformAutomatedTestsGallery);
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
		public void AutomatedTests()
		{
			App.WaitForElement("SUCCESS", timeout: TimeSpan.FromMinutes(2));
		}
	}
}
