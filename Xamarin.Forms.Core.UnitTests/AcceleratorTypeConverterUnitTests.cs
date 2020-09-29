using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class AcceleratorTypeConverterUnitTests : BaseTestFixture
	{
		[Test]
		public void TestAcceleratorTypeConverter()
		{
			var converter = new AcceleratorTypeConverter();
			string shourtCutKeyBinding = "ctrl+A";
			Assert.AreEqual(Accelerator.FromString(shourtCutKeyBinding), (Accelerator)converter.ConvertFromInvariantString(shourtCutKeyBinding));
		}
	}
}