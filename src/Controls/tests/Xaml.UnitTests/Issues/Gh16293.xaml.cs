using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh16293 : ContentPage
	{
		public Gh16293() => InitializeComponent();
		public Gh16293(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void ShouldResolveNested(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh16293)));

				var layout = new Gh16293(useCompiledXaml);
				Assert.Equal("LibraryConstant", layout.Label1.Text);
				Assert.Equal("NestedLibraryConstant", layout.Label2.Text);
			}

			[InlineData(true), InlineData(false)]
			public void ShouldResolveInternalNested(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh16293)));

				var layout = new Gh16293(useCompiledXaml);
				Assert.Equal("InternalLibraryConstant", layout.Label3.Text);
				Assert.Equal("InternalNestedLibraryConstant", layout.Label4.Text);
			}
		}
	}
}
