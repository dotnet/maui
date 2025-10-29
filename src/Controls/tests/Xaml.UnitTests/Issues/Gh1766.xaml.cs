using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh1766 : ContentPage
	{
		public Gh1766()
		{
			InitializeComponent();
		}

		public Gh1766(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true), InlineData(false)]
			public void CSSPropertiesNotInerited(bool useCompiledXaml)
			{
				var layout = new Gh1766(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.stack.BackgroundColor);
				Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.entry.BackgroundColor);
			}
		}
	}
}
