using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz40906 : ContentPage
{
	public Bz40906()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void ParsingCDATA([Values] XamlInflator inflator)
		{
			var page = new Bz40906(inflator);
			Assert.AreEqual("Foo", page.label0.Text);
			Assert.AreEqual("FooBar>><<", page.label1.Text);
		}
	}
}
