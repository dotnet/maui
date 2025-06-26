using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11620 : ContentPage
	{
		public Gh11620() => InitializeComponent();
		public Gh11620(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Fact]
			public void BoxOnAdd([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11620(useCompiledXaml);
				var arr = layout.Resources["myArray"];
				Assert.That(arr, Is.TypeOf<object[]>());
				Assert.Equal(3, ((object[])arr).Length);
				Assert.Equal(32, ((object[])arr)[2]);
			}
		}
	}
}