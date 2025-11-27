// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2678 : ContentPage
{
	public Gh2678() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void StyleClassCanBeChanged([Values] XamlInflator inflator)
		{
			var layout = new Gh2678(inflator);
			var label = layout.label0;
			Assert.That(label.BackgroundColor, Is.EqualTo(Colors.Red));
			label.StyleClass = new List<string> { "two" };
			Assert.That(label.BackgroundColor, Is.EqualTo(Colors.Green));
		}
	}
}
