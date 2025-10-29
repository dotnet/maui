using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	using AbsoluteLayout = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

	public partial class Gh11551 : ContentPage
	{
		public Gh11551() => InitializeComponent();
		public Gh11551(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh11551(useCompiledXaml);
				var bounds = AbsoluteLayout.GetLayoutBounds(layout.label);
				Assert.Equal(new Rect(1, .5, -1, 22, bounds));
			}
		}
	}
}
