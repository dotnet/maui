using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class FontConverterTests : BaseTestFixture
	{
		[TestCase("Bold", Controls.FontAttributes.Bold)]
		[TestCase("Italic", Controls.FontAttributes.Italic)]
		[TestCase("Bold, Italic", Controls.FontAttributes.Bold | Controls.FontAttributes.Italic)]
		public void FontAttributes(string attributeString, FontAttributes result)
		{
			var xaml = @"
			<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" FontAttributes=""" + result + @""" />";

			Device.PlatformServices = new MockPlatformServices();

			var label = new Label().LoadFromXaml(xaml);

			Assert.AreEqual(result, label.FontAttributes);
		}
	}
}