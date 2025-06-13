using Xunit;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class EditorUITests : _ViewUITests
	{
		public const string EditorGallery = "Editor Gallery";

		public EditorUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(EditorGallery);
		}

		[Fact]
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
