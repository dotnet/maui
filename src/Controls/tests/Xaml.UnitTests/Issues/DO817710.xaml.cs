using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DO817710 : ContentPage
	{
		public DO817710() => InitializeComponent();
		public DO817710(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				Assert.DoesNotThrow(() => new DO817710(useCompiledXaml: useCompiledXaml));
			}
		}
	}
}
