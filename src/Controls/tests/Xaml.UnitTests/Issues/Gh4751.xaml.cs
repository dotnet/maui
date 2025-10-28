using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4751VM
	{
		public string Title { get; }
		public Gh4751VM(string title = null) => Title = title; //a .ctor with a default value IS NOT a default .ctor
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh4751 : ContentPage
	{
		public Gh4751() => InitializeComponent();
		public Gh4751(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh4751)));
				else
					Assert.Throws<XamlParseException>(() => new Gh4751(useCompiledXaml));
			}
		}
	}
}
