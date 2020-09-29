using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class StringLiterals : ContentPage
	{
		public StringLiterals()
		{
			InitializeComponent();
		}

		public StringLiterals(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void EscapedStringsAreTreatedAsLiterals(bool useCompiledXaml)
			{
				var layout = new StringLiterals(useCompiledXaml);
				Assert.AreEqual("Foo", layout.label0.Text);
				Assert.AreEqual("{Foo}", layout.label1.Text);
				Assert.AreEqual("Foo", layout.label2.Text);
				Assert.AreEqual("Foo", layout.label3.Text);
			}
		}
	}
}