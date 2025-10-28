using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class StringLiterals : ContentPage
	{
		public StringLiterals()
		{
			InitializeComponent();
		}

		public StringLiterals(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void EscapedStringsAreTreatedAsLiterals(bool useCompiledXaml)
			{
				var layout = new StringLiterals(useCompiledXaml);
				Assert.Equal("Foo", layout.label0.Text);
				Assert.Equal("{Foo}", layout.label1.Text);
				Assert.Equal("Foo", layout.label2.Text);
				Assert.Equal("Foo", layout.label3.Text);
			}
		}
	}
}