using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class CollectionViewUITests : UITest
	{
		const string CollectionViewGallery = "CollectionView Gallery";

		public CollectionViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(CollectionViewGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[TearDown]
		public void CollectionViewUITestTearDown()
		{
			this.Back();
		}
	}
}
