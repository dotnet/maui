using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DynamicResource : ContentPage
	{
		public DynamicResource()
		{
			InitializeComponent();
		}

		public DynamicResource(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void TestDynamicResources(bool useCompiledXaml)
			{
				var layout = new DynamicResource(useCompiledXaml);
				var label = layout.label0;

				Assert.Null(label.Text);

				layout.Resources = new ResourceDictionary {
					{"FooBar", "FOOBAR"},
				};
				Assert.Equal("FOOBAR", label.Text);
			}
		}
	}
}