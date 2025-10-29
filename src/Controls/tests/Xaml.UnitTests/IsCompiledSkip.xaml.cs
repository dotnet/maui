using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class IsCompiledSkip : ContentPage
	{
		public IsCompiledSkip()
		{
			InitializeComponent();
		}

		public IsCompiledSkip(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}


		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void IsCompiled(bool useCompiledXaml)
		{
			var layout = new IsCompiledSkip(useCompiledXaml);
			Assert.False(typeof(IsCompiledSkip).IsCompiled());
		}
	}
}