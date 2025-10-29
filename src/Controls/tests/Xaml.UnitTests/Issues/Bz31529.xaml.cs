using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz31529 : ContentPage
	{
		public Bz31529()
		{
			InitializeComponent();
		}
		public Bz31529(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void AllowWhiteSpacesInMarkups(bool useCompiledXaml)
			{
				var layout = new Bz31529(useCompiledXaml);
				Assert.Equal("Foo", layout.button.CommandParameter);
			}
		}
	}
}