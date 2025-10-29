using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class ColumnDefinitionCollectionTypeConverterUnitTests : BaseTestFixture
{
	private readonly ColumnDefinitionCollectionTypeConverter _converter = new();

	[Fact]
	public void ConvertNullTest()
	{
		Assert.Throws<InvalidOperationException>(() => _converter.ConvertFromInvariantString(null));
	}
}