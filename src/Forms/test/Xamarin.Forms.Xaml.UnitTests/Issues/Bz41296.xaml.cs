using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz41296 : ContentPage
	{
		public Bz41296()
		{
			InitializeComponent();
		}

		public Bz41296(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void MarkupExtensionInDefaultNamespace(bool useCompiledXaml)
			{
				var layout = new Bz41296(useCompiledXaml);
				Assert.AreEqual("FooBar", layout.TestLabel.Text.ToString());
			}
		}
	}
}