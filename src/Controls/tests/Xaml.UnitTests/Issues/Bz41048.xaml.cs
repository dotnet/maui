using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz41048 : ContentPage
{
	public Bz41048()
	{
		InitializeComponent();
	}


	[TestFixture]
	class Tests
	{
		[TearDown]
		public void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		public void StyleDoesNotOverrideValues([Values] XamlInflator inflator)
		{
			var layout = new Bz41048(inflator);
			var label = layout.label0;
			Assert.That(label.TextColor, Is.EqualTo(Colors.Red));
			Assert.That(label.FontAttributes, Is.EqualTo(FontAttributes.Bold));
			Assert.That(label.LineBreakMode, Is.EqualTo(LineBreakMode.WordWrap));
		}
	}
}