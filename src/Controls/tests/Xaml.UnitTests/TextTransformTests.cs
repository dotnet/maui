using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class TextTransformTests : BaseTestFixture
	{
		[InlineData(TextTransform.None)]
		[InlineData(TextTransform.Lowercase)]
		[Theory]
		[InlineData(TextTransform.Uppercase)]
		public void LabelTextTransform(TextTransform result)
		{
			var xaml = @"
			<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TextTransform=""" + result + @""" />";

			var label = new Label().LoadFromXaml(xaml);

			Assert.Equal(result, label.TextTransform);
		}
	}
}