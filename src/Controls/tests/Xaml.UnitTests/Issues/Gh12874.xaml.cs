using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh12874 : ContentPage
	{
		public Gh12874() => InitializeComponent();
		public Gh12874(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh12874(useCompiledXaml);
				Assert.Equal(LayoutOptions.Start, layout.label0.HorizontalOptions);
				Assert.Equal(LayoutOptions.Start, layout.label1.HorizontalOptions);
				layout.label0.ClearValue(Label.HorizontalOptionsProperty);
				layout.label1.ClearValue(Label.HorizontalOptionsProperty);
				Assert.Equal(LayoutOptions.Center, layout.label0.HorizontalOptions);
				Assert.Equal(LayoutOptions.Center, layout.label1.HorizontalOptions);
			}
		}
	}
}
