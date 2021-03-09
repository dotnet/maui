using System;
using System.Collections.Generic;

using NUnit.Framework;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;

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
		}

		[TestFixture]
		class Tests
		{
			[TestCase(false), TestCase(true)]
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
