using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2064 : ContentPage
	{
		public Gh2064()
		{
			InitializeComponent();
		}

		public Gh2064(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(false), InlineData(true)]
			public void ReportMissingTargetTypeOnStyle(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2064)));
				else
					Assert.Throws<XamlParseException>(() => new Gh2064(useCompiledXaml));
			}
		}
	}
}
