using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ContraintTypeConverterTests : BaseTestFixture
	{
		[Test]
		public void ConvertFrom()
		{
			var converter = new ConstraintTypeConverter();
			Assert.AreEqual(Constraint.Constant(1.0).Compute(null), ((Constraint)converter.ConvertFromInvariantString("1.0")).Compute(null));
			Assert.AreEqual(Constraint.Constant(1.3).Compute(null), ((Constraint)converter.ConvertFromInvariantString("1.3")).Compute(null));
		}
	}
}