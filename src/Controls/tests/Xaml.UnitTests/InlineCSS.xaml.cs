using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class InlineCSS : ContentPage
	{
		public InlineCSS()
		{
			InitializeComponent();
		}

		public InlineCSS(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false), TestCase(true)]
			public void InlineCSSParsed(bool useCompiledXaml)
			{
				var layout = new InlineCSS(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
			}

			[InlineData(false), TestCase(true)]
			public void InitialValue(bool useCompiledXaml)
			{
				var layout = new InlineCSS(useCompiledXaml);
				Assert.Equal(Colors.Green, layout.BackgroundColor);
				Assert.Equal(Colors.Green, layout.stack.BackgroundColor);
				Assert.Equal(Colors.Green, layout.button.BackgroundColor);
				Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
				Assert.Equal(TextTransform.Uppercase, layout.label.TextTransform);
			}
		}
	}
}
