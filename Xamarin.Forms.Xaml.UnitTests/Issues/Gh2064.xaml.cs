using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh2064)));
				else
					Assert.Throws<XamlParseException>(()=> new Gh2064(useCompiledXaml));
			}
		}
	}
}
