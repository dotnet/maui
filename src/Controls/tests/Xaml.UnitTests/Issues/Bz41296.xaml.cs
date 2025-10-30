using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz41296 : ContentPage
{
	public Bz41296()
	{
		InitializeComponent();
	}


	[TestFixture]
	class Tests
	{
		[Test]
		public void MarkupExtensionInDefaultNamespace([Values] XamlInflator inflator)
		{
			var layout = new Bz41296(inflator);
			Assert.AreEqual("FooBar", layout.TestLabel.Text.ToString());
		}
	}
}
