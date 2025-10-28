using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh4099 : ContentPage
	{
		public Gh4099() => InitializeComponent();

		public Gh4099(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[Fact(Skip = "Ignore for now, Compiled Converters are disabled")]
			public void BetterExceptionReport(bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					try
					{
						MockCompiler.Compile(typeof(Gh4099));
					}
					catch (BuildException xpe)
					{
						Assert.Equal(5, xpe.XmlInfo.LineNumber);
						Assert.Pass();
					}
					throw new Xunit.Sdk.XunitException();
				}
			}
		}
	}
}
