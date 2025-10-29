using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2578 : ContentPage
	{
		public Issue2578()
		{
			InitializeComponent();
		}

		public Issue2578(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MultipleTriggers(bool useCompiledXaml)
			{
				Issue2578 layout = new Issue2578(useCompiledXaml);

				Assert.Null(layout.label.Text);
				Assert.Null(layout.label.BackgroundColor);
				Assert.Equal(Colors.Olive, layout.label.TextColor);
				layout.label.Text = "Foo";
				Assert.Equal(Colors.Red, layout.label.BackgroundColor);
			}
		}
	}
}