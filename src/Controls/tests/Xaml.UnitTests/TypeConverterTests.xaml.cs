using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class TypeConverterTests : ContentPage
	{
		public TypeConverterTests()
		{
			InitializeComponent();
		}

		public TypeConverterTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void UriAreConverted(bool useCompiledXaml)
			{
				var layout = new TypeConverterTests(useCompiledXaml);
				Assert.IsType<Uri>(layout.imageSource.Uri);
				Assert.Equal("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
			}
		}
	}
}