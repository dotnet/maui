// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2678 : ContentPage
{
	public Gh2678() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void StyleClassCanBeChanged(XamlInflator inflator)
		{
			var layout = new Gh2678(inflator);
			var label = layout.label0;
			Assert.Equal(Colors.Red, label.BackgroundColor);
			label.StyleClass = new List<string> { "two" };
			Assert.Equal(Colors.Green, label.BackgroundColor);
		}
	}
}
