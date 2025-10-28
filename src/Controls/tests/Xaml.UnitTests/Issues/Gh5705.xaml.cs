using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh5705 : Shell
	{
		public Gh5705() => InitializeComponent();
		public Gh5705(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var vm = new object();
				var shell = new Gh5705(useCompiledXaml) { BindingContext = vm };
				Assert.Equal(vm, shell.searchHandler.BindingContext);
			}
		}
	}
}
