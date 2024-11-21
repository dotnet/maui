using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class KeyboardTests : BaseTestFixture
	{
		[Fact]
		public void KeyboardTypesAreCorrect()
		{
			Assert.True(Keyboard.Chat is ChatKeyboard);
			Assert.True(Keyboard.Email is EmailKeyboard);
			Assert.True(Keyboard.Numeric is NumericKeyboard);
			Assert.True(Keyboard.Telephone is TelephoneKeyboard);
			Assert.True(Keyboard.Text is TextKeyboard);
			Assert.True(Keyboard.Url is UrlKeyboard);
			Assert.True(Keyboard.Date is DateKeyboard);
			Assert.True(Keyboard.Time is TimeKeyboard);
			Assert.True(Keyboard.Password is PasswordKeyboard);
		}
	}


	public class KeyboardTypeConverterTests : BaseTestFixture
	{
		[Fact]
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
				{"Keyboard.Date", Keyboard.Date},
				{"Keyboard.Time", Keyboard.Time},
				{"Keyboard.Password", Keyboard.Password},
			})
				Assert.Same(kvp.Value, converter.ConvertFromInvariantString(kvp.Key));
		}

		[Fact]
		public void ConversionFail()
		{
			var converter = new KeyboardTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("Foo"));
		}
	}
}