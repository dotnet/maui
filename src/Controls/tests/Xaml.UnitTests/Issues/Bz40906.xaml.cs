using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz40906 : ContentPage
	{
		public Bz40906()
		{
			InitializeComponent();
		}

		public Bz40906(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void ParsingCDATA(bool useCompiledXaml)
			{
				var page = new Bz40906(useCompiledXaml);
				Assert.Equal("Foo", page.label0.Text);
				Assert.Equal("FooBar>><<", page.label1.Text);
			}
		}
	}
}
