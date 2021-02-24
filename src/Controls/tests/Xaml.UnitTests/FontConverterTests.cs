using NUnit.Framework;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class FontConverterTests : BaseTestFixture
	{
		[TestCase("Bold", Forms.FontAttributes.Bold)]
		[TestCase("Italic", Forms.FontAttributes.Italic)]
		[TestCase("Bold, Italic", Forms.FontAttributes.Bold | Forms.FontAttributes.Italic)]
		public void FontAttributes(string attributeString, FontAttributes result)
		{
			var xaml = @"
			<Label 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" FontAttributes=""" + result + @""" />";

			Device.PlatformServices = new MockPlatformServices();

			var label = new Label().LoadFromXaml(xaml);

			Assert.AreEqual(result, label.FontAttributes);
		}
	}
}