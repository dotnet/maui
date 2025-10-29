using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh11061 : ContentPage
	{
		public DateTime MyDateTime { get; set; }

		public Gh11061() => InitializeComponent();
		public Gh11061(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh11061)));
				else
					Assert.Throws<XamlParseException>(() => new Gh11061(useCompiledXaml) { BindingContext = new { Date = DateTime.Today } });
			}
		}
	}
}
