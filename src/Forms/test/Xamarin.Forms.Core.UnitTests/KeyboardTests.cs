using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	internal class KeyboardTests : BaseTestFixture
	{
		[Test]
		public void KeyboardTypesAreCorrect()
		{
			Assert.True(Keyboard.Chat is ChatKeyboard);
			Assert.True(Keyboard.Email is EmailKeyboard);
			Assert.True(Keyboard.Numeric is NumericKeyboard);
			Assert.True(Keyboard.Telephone is TelephoneKeyboard);
			Assert.True(Keyboard.Text is TextKeyboard);
			Assert.True(Keyboard.Url is UrlKeyboard);
		}
	}

	[TestFixture]
	internal class KeyboardTypeConverterTests : BaseTestFixture
	{
		[Test]
		public void ConversionConvert()
		{

			var converter = new KeyboardTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			foreach (var kvp in new Dictionary<string, Keyboard> {
				{"Keyboard.Default", Keyboard.Default},
				{"Keyboard.Email", Keyboard.Email},
				{"Keyboard.Text", Keyboard.Text},
				{"Keyboard.Url", Keyboard.Url},
				{"Keyboard.Telephone", Keyboard.Telephone},
				{"Keyboard.Chat", Keyboard.Chat},
			})
				Assert.AreSame(kvp.Value, converter.ConvertFromInvariantString(kvp.Key));
		}

		[Test]
		public void ConversionFail()
		{
			var converter = new KeyboardTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("Foo"));
		}
	}
}