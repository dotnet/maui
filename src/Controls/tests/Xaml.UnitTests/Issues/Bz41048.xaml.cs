using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz41048 : ContentPage
	{
		public Bz41048()
		{
			InitializeComponent();
		}

		public Bz41048(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			public void TearDown()
			{
				Application.Current = null;
			}

			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void StyleDoesNotOverrideValues(bool useCompiledXaml)
			{
				var layout = new Bz41048(useCompiledXaml);
				var label = layout.label0;
				Assert.Equal(Colors.Red, label.TextColor);
				Assert.Equal(FontAttributes.Bold, label.FontAttributes);
				Assert.Equal(LineBreakMode.WordWrap, label.LineBreakMode);
			}
		}
	}
}