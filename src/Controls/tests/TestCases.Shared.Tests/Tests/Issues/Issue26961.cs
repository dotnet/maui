using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26961 : _IssuesUITest
	{
		public Issue26961(TestDevice device) : base(device) { }

		public override string Issue => "Lines not drawing correctly with StrokeThickness and position-dependent starting points";

		[Test]
		[Category(UITestCategories.Shape)]
		public void LineWithThickStrokeShouldRenderSymmetrical()
		{
			App.WaitForElement("Issue26961Description");
			App.WaitForElement("Issue26961SymmetryResult");

			// HostApp computes PathForBounds for two mirror-image lines with StrokeThickness=20.
			// "Pass" means the thick-stroke inset is correctly combined with the right-edge
			// translation check, keeping both lines symmetric around the center axis X=100.
			var result = App.FindElement("Issue26961SymmetryResult").GetText();
			Assert.That(result, Is.EqualTo("Pass"),
				"Lines with StrokeThickness=20 and position-dependent starting points should render symmetrically");
		}
	}
}
