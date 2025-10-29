using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh5254VM
	{
		public string Title { get; set; }
		public List<Gh5254VM> Answer { get; set; }
	}

	public partial class Gh5254 : ContentPage
	{
		public Gh5254() => InitializeComponent();
		public Gh5254(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh5254(useCompiledXaml)
				{
					BindingContext = new Gh5254VM
					{
						Answer = new List<Gh5254VM> {
							new Gh5254VM { Title = "Foo"},
							new Gh5254VM { Title = "Bar"},
						}
					}
				};
				Assert.Equal("Foo", layout.label.Text);
			}
		}
	}
}
