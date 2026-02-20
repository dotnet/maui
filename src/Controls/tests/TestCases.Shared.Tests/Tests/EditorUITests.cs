using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class EditorUITests : _ViewUITests
	{
		public const string EditorGallery = "Editor Gallery";

		public override string GalleryPageName => EditorGallery;

		public EditorUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(EditorGallery);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
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
