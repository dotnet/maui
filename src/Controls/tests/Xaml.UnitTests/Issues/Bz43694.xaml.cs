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
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void xStaticWithOnPlatformChildInRD(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(9, 6).Validate(() => MockCompiler.Compile(typeof(Bz43694)));
				else
					new XamlParseExceptionConstraint(9, 6).Validate(() => new Bz43694(useCompiledXaml));
			}
		}
	}
}