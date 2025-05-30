using System;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes;

public class MatrixTypeConverterUnitTests : BaseTestFixture
{
	private readonly MatrixTypeConverter _converter = new();

	[Fact]
	public void ConvertNullTest()
	{
		Assert.Throws<ArgumentException>(() => _converter.ConvertFromInvariantString(null));
	}
}