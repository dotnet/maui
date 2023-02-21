using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EffectiveFlowDirectionExtensions : BaseTestFixture
	{
		[Fact]
		public void LeftToRightImplicit()
		{
			var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection();

			Assert.True(target.IsLeftToRight());
			Assert.True(target.IsImplicit());

			Assert.False(target.IsRightToLeft());
			Assert.False(target.IsExplicit());
		}

		[Fact]
		public void LeftToRightExplicit()
		{
			var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection(isExplicit: true);

			Assert.True(target.IsLeftToRight());
			Assert.True(target.IsExplicit());

			Assert.False(target.IsRightToLeft());
			Assert.False(target.IsImplicit());
		}

		[Fact]
		public void RightToLeftImplicit()
		{
			var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection();

			Assert.True(target.IsRightToLeft());
			Assert.True(target.IsImplicit());

			Assert.False(target.IsLeftToRight());
			Assert.False(target.IsExplicit());
		}

		[Fact]
		public void RightToLeftExplicit()
		{
			var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection(isExplicit: true);

			Assert.True(target.IsRightToLeft());
			Assert.True(target.IsExplicit());

			Assert.False(target.IsLeftToRight());
			Assert.False(target.IsImplicit());
		}
	}
}