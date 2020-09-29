using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BindingTypeConverterTests : BaseTestFixture
	{
		[Test]
		public void CanConvertFrom()
		{
			var c = new BindingTypeConverter();
			Assert.That(c.CanConvertFrom(typeof(string)), Is.True);
			Assert.That(c.CanConvertFrom(typeof(int)), Is.False);
		}

		[Test]
		public void Convert()
		{
			var c = new BindingTypeConverter();
			var binding = c.ConvertFromInvariantString("Path");

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.That(((Binding)binding).Path, Is.EqualTo("Path"));
			Assert.That(((Binding)binding).Mode, Is.EqualTo(BindingMode.Default));
		}
	}
}