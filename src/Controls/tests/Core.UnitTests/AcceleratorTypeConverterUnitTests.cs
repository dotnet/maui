using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AcceleratorTypeConverterUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestAcceleratorTypeConverter()
		{
			var converter = new AcceleratorTypeConverter();
			string shourtCutKeyBinding = "ctrl+A";
			Assert.Equal(Accelerator.FromString(shourtCutKeyBinding), (Accelerator)converter.ConvertFromInvariantString(shourtCutKeyBinding));
		}
	}
}
