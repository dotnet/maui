using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz43694 : ContentPage
	{
		public Bz43694()
		{
			InitializeComponent();
		}

		public Bz43694(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void xStaticWithOnPlatformChildInRD(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(9, 6), () => MockCompiler.Compile(typeof(Bz43694)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(9, 6), () => new Bz43694(useCompiledXaml));
			}
		}
	}
}