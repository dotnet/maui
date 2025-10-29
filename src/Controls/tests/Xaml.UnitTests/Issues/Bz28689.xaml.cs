using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz28689 : ContentPage
	{
		public Bz28689()
		{
			InitializeComponent();
		}

		public Bz28689(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void XArrayInResources(bool useCompiledXaml)
			{
				var layout = new Bz28689(useCompiledXaml);
				var array = layout.Resources["stringArray"];
				Assert.IsType<string[]>(array);
				var stringarray = (string[])array;
				Assert.Equal(2, stringarray.Length);
				Assert.Equal("Test1", stringarray[0]);
				Assert.Equal("Test2", stringarray[1]);
			}
		}
	}
}