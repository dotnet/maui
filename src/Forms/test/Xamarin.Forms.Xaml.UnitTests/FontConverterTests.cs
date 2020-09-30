using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
#pragma warning disable 618
			Assert.AreEqual(result, label.Font.FontAttributes);
#pragma warning restore 618
		}
	}
}