using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class LayoutUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public LayoutUITests(TestDevice device)	
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[TearDown]
		public void LayoutUITestTearDown()
		{
			this.Back();
		}
	}
}
