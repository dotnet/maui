using System;
using System.Collections.Generic;

using System.Maui;
using NUnit.Framework;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Bz41296 : ContentPage
	{
		public Bz41296()
		{
			InitializeComponent ();
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
				var layout = new Bz41296 (useCompiledXaml);
				Assert.AreEqual("FooBar", layout.TestLabel.Text.ToString());
			}
		}
	}
}