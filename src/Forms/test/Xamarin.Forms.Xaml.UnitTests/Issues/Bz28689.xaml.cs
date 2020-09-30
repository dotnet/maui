using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz28689 : ContentPage
	{
		public Bz28689()
		{
			InitializeComponent();
		}

		public Bz28689(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void XArrayInResources(bool useCompiledXaml)
			{
				var layout = new Bz28689(useCompiledXaml);
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