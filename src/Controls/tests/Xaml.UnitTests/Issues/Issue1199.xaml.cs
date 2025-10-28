using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue1199 : ContentPage
	{
		public Issue1199()
		{
			InitializeComponent();
		}

		public Issue1199(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void AllowCreationOfTypesFromString(bool useCompiledXaml)
			{
				var layout = new Issue1199(useCompiledXaml);
				var res = (Color)layout.Resources["AlmostSilver"];

				Assert.Equal(Color.FromArgb("#FFCCCCCC"), res);
			}
		}
	}
}