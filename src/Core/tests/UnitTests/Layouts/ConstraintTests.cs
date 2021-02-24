using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class ConstraintTests
	{
		[Theory("When resolving constraints, external constraints take precedence")]
		[InlineData(100, 200, 100)]
		[InlineData(100, 100, 100)]
		[InlineData(100, -1, 100)]
		public void ExternalWinsOverDesired(double externalConstraint, double desiredLength, double expected)
		{
			var resolution = LayoutManager.ResolveConstraints(externalConstraint, desiredLength);
			Assert.Equal(expected, resolution);
		}

		[Theory("When resolving constraints, external constraints take precedence")]
		[InlineData(100, 200, 130, 100)]
		[InlineData(100, -1, 130, 100)]
		public void ExternalWinsOverDesiredAndMeasured(double externalConstraint, double desiredLength, double measured, double expected)
		{
			var resolution = LayoutManager.ResolveConstraints(externalConstraint, desiredLength, measured);
			Assert.Equal(expected, resolution);
		}

		[Fact("If external and request constraints don't apply, constrain to measured value")]
		public void MeasuredWinsIfNothingElseApplies()
		{
			var resolution = LayoutManager.ResolveConstraints(double.PositiveInfinity, -1, 245);
			Assert.Equal(245, resolution);
		}

		[Fact("If external constraints don't apply, constrain to requested value")]
		public void RequestedTakesPrecedenceOverMeasured()
		{
			var resolution = LayoutManager.ResolveConstraints(double.PositiveInfinity, 90, 245);
			Assert.Equal(90, resolution);
		}
	}
}
