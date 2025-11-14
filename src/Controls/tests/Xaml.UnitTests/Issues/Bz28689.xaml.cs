using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz28689 : ContentPage
	{
		public Bz28689()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void XArrayInResources([Values] XamlInflator inflator)
			{
				var layout = new Bz28689(inflator);
				var array = layout.Resources["stringArray"];
				Assert.That(array, Is.TypeOf<string[]>());
				var stringarray = (string[])array;
				Assert.AreEqual(2, stringarray.Length);
				Assert.AreEqual("Test1", stringarray[0]);
				Assert.AreEqual("Test2", stringarray[1]);
			}
		}
	}
}