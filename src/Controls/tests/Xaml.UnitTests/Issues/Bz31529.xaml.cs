using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz31529 : ContentPage
	{
		public Bz31529()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void AllowWhiteSpacesInMarkups([Values] XamlInflator inflator)
			{
				var layout = new Bz31529(inflator);
				Assert.AreEqual("Foo", layout.button.CommandParameter);
			}
		}
	}
}