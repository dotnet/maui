using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Unreported009 : ContentPage
	{
		public Unreported009()
		{
			InitializeComponent();
		}

		public Unreported009(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void AllowSetterValueAsElementProperties(bool useCompiledXaml)
			{
				var p = new Unreported009(useCompiledXaml);
				var s = p.Resources["Default"] as Style;
				Assert.Equal("Bananas!", (s.Setters[0].Value as Label).Text);
			}
		}
	}
}
