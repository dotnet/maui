using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh6648 : ContentPage
	{
		public Gh6648() => InitializeComponent();
		public Gh6648(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public public class Tests
		{
			//[Theory]
			//[InlineData(true)]
			//[InlineData(false)]
			//public void Method(bool useCompiledXaml)
			//{
			//	if (useCompiledXaml)
			//		Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh6648)));
			//}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh6648(useCompiledXaml);
				layout.stack.BindingContext = new { foo = "Foo" };
				Assert.Equal("Foo", layout.label.Text);
			}
		}
	}
}
