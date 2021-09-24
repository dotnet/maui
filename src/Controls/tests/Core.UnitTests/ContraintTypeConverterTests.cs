using Microsoft.Maui.Controls.Compatibility;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;

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