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


		public class Tests
		{
			[Theory]
			[Values]
			public void XArrayInResources(XamlInflator inflator)
			{
				var layout = new Bz28689(inflator);
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