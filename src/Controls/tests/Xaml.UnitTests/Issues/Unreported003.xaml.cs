using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Unreported003 : ContentPage
	{
		public Unreported003()
		{
			InitializeComponent();
		}

		public Unreported003(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void AllowCtorArgsForValueTypes(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Unreported003));

				var page = new Unreported003(useCompiledXaml);
			}
		}
	}
}