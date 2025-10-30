using System;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes;

public class PathGeometryConverterTests : BaseTestFixture
{
	private readonly PathGeometryConverter _converter = new();

	[Fact]
	public void ConvertNullTest()
	{
		var result = _converter.ConvertFromInvariantString(null);
		var pathGeometry = Assert.IsType<PathGeometry>(result);
		Assert.Empty(pathGeometry.Figures);
	}
}