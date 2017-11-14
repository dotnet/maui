using NUnit.Framework;
using System.Linq;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EffectiveFlowDirectionExtensions : BaseTestFixture
	{

		[Test]
		public void LeftToRightImplicit()
		{
			var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection();

			Assert.IsTrue(target.IsLeftToRight());
			Assert.IsTrue(target.IsImplicit());

			Assert.IsFalse(target.IsRightToLeft());
			Assert.IsFalse(target.IsExplicit());
		}

		[Test]
		public void LeftToRightExplicit()
		{
			var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection(isExplicit: true);

			Assert.IsTrue(target.IsLeftToRight());
			Assert.IsTrue(target.IsExplicit());

			Assert.IsFalse(target.IsRightToLeft());
			Assert.IsFalse(target.IsImplicit());
		}

		[Test]
		public void RightToLeftImplicit()
		{
			var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection();

			Assert.IsTrue(target.IsRightToLeft());
			Assert.IsTrue(target.IsImplicit());

			Assert.IsFalse(target.IsLeftToRight());
			Assert.IsFalse(target.IsExplicit());
		}

		[Test]
		public void RightToLeftExplicit()
		{
			var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection(isExplicit: true);

			Assert.IsTrue(target.IsRightToLeft());
			Assert.IsTrue(target.IsExplicit());

			Assert.IsFalse(target.IsLeftToRight());
			Assert.IsFalse(target.IsImplicit());
		}
	}
}