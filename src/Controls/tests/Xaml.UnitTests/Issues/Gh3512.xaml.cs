using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh3512 : ContentPage
	{
		public Gh3512()
		{
			InitializeComponent();
		}

		public Gh3512(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false), InlineData(true)]
			public void ThrowsOnDuplicateXKey(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh3512)));
				else
					Assert.Throws<ArgumentException>(() => new Gh3512(useCompiledXaml));
			}
		}
	}
}
