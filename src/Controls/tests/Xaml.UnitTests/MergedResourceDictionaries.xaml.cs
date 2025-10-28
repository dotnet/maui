using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class MergedResourceDictionaries : ContentPage
	{
		public MergedResourceDictionaries()
		{
			InitializeComponent();
		}

		public MergedResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void MergedResourcesAreFound(bool useCompiledXaml)
			{
				MockCompiler.Compile(typeof(MergedResourceDictionaries));
				var layout = new MergedResourceDictionaries(useCompiledXaml);
				Assert.Equal("Foo", layout.label0.Text);
				Assert.Equal(Colors.Pink, layout.label0.TextColor);
				Assert.Equal(Color.FromArgb("#111", layout.label0.BackgroundColor));
			}
		}
	}
}