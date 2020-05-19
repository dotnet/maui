using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2034 : ContentPage
	{
		public Gh2034()
		{
			InitializeComponent();
		}

		public Gh2034(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			public void Compiles(bool useCompiledXaml)
			{
				if (!useCompiledXaml)
					return;
				MockCompiler.Compile(typeof(Gh2034));
				Assert.Pass();
			}
		}
	}
}
