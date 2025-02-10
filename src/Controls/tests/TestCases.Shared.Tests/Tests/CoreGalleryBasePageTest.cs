using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class CoreGalleryBasePageTest : UITest
	{
		public CoreGalleryBasePageTest(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{			
			NavigateToGallery();
		}

		protected abstract void NavigateToGallery();
	}
}
