using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlProcessing(XamlInflator.Default, true)]
	public partial class StringLiterals : ContentPage
	{
		public StringLiterals()
		{
			InitializeComponent();
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void EscapedStringsAreTreatedAsLiterals([Values]XamlInflator inflator)
			{
				var layout = new StringLiterals(inflator);
				Assert.AreEqual("Foo", layout.label0.Text);
				Assert.AreEqual("{Foo}", layout.label1.Text);
				Assert.AreEqual("Foo", layout.label2.Text);
				Assert.AreEqual("Foo", layout.label3.Text);
			}
		}
	}
}