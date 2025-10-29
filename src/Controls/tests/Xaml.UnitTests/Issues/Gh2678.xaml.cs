// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh2678 : ContentPage
	{
		public Gh2678() => InitializeComponent();
		public Gh2678(bool useCompiledXaml)
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
				var layout = new Gh2678(useCompiledXaml);
				var label = layout.label0;
				Assert.Equal(Colors.Red, label.BackgroundColor);
				label.StyleClass = new List<string> { "two" };
				Assert.Equal(Colors.Green, label.BackgroundColor);
			}
		}
	}
}
