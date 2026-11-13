using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11404 : _IssuesUITest
	{
		public Issue11404(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Line coordinates not computed correctly";

		[Test]
		[Category(UITestCategories.Shape)]
		public void LineWithReversedCoordinatesShouldRenderSymmetrical()
		{
			App.WaitForElement("DescriptionLabel");

			// Wait for the symmetry check to complete (SizeChanged fires after layout)
			App.WaitForElement("SymmetryResult");

			// The HostApp computes path bounds for both lines and checks symmetry.
			// "Pass" means bounds1.Left + bounds2.Right ≈ 200 and bounds1.Right + bounds2.Left ≈ 200,
			// proving the path coordinates are symmetric around the center axis (X=100).
			var result = App.FindElement("SymmetryResult").GetText();
			Assert.That(result, Is.EqualTo("Pass"),
				"Line paths should be symmetric: path coordinates for mirror-image lines should mirror around center axis X=100");
		}
	}
}
