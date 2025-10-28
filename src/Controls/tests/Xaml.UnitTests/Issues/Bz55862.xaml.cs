using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[System.ComponentModel.TypeConverter(typeof(ThicknessTypeConverter))]
	public struct Bz55862Bar
	{
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz55862 : ContentPage
	{
		public Bz55862Bar Foo { get; set; }
		public Bz55862()
		{
			InitializeComponent();
		}

		public Bz55862(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false/*, true*/)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz55862)));
				else
					Assert.Throws<XamlParseException>(() => new Bz55862(useCompiledXaml));
			}
		}
	}
}
