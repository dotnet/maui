using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.A
{
	public partial class Bz31234 : ContentPage
	{
		public Bz31234()
		{
			InitializeComponent();
		}

		public Bz31234(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(true), TestCase(false)]
			public void ShouldPass(bool useCompiledXaml)
			{
				new Bz31234(useCompiledXaml);
				Assert.Pass();
			}
		}
	}
}