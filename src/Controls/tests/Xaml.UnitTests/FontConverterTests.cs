using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public class FontConverterTests : BaseTestFixture
	{
		[Xunit.Theory]
		[InlineData("Bold", Controls.FontAttributes.Bold)]
		[InlineData("Italic", Controls.FontAttributes.Italic)]
		[InlineData("Bold, Italic", Controls.FontAttributes.Bold | Controls.FontAttributes.Italic)]
		public void FontAttributes(FontAttributes result)
		{
			var xaml = @"
			<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" FontAttributes=""" + result + @""" />";

			var label = new Label().LoadFromXaml(xaml);

			Assert.Equal(result, label.FontAttributes);
		}
	}
}