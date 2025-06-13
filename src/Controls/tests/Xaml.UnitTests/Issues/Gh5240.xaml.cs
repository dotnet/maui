using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh5240 : ContentPage
	{
		public Gh5240() => InitializeComponent();
		public Gh5240(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			[Fact]
			public void FailOnUnresolvedDataType([Values(true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					() => MockCompiler.Compile(typeof(Gh5240))
			}
		}
	}
}
