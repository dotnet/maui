using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Maui;

namespace System.Maui.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh1978 : ContentPage
	{
		public Gh1978()
		{
			InitializeComponent();
		}

		public Gh1978(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			public void ReportError(bool useCompiledXaml)
			{
				if (!useCompiledXaml)
					return;
				Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh1978)));
			}
		}
	}
}
