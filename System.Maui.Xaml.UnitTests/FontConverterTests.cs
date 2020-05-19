using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	[TestFixture]
	public class FontConverterTests : BaseTestFixture
	{
		[TestCase ("Bold", System.Maui.FontAttributes.Bold)]
		[TestCase ("Italic", System.Maui.FontAttributes.Italic)]
		[TestCase ("Bold, Italic", System.Maui.FontAttributes.Bold | System.Maui.FontAttributes.Italic)]
		public void FontAttributes (string attributeString, FontAttributes result)
		{
			var xaml = @"
			<Label 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" FontAttributes=""" + result + @""" />";

			Device.PlatformServices = new MockPlatformServices ();

			var label = new Label ().LoadFromXaml (xaml);

			Assert.AreEqual (result, label.FontAttributes);
#pragma warning disable 618
			Assert.AreEqual (result, label.Font.FontAttributes);
#pragma warning restore 618
		}
	}
}