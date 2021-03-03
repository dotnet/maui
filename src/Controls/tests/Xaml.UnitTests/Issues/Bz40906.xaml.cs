using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz40906 : ContentPage
	{
		public Bz40906()
		{
			InitializeComponent();
		}

		public Bz40906(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
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

			[TestCase(true)]
			[TestCase(false)]
			public void ParsingCDATA(bool useCompiledXaml)
			{
				var page = new Bz40906(useCompiledXaml);
				Assert.AreEqual("Foo", page.label0.Text);
				Assert.AreEqual("FooBar>><<", page.label1.Text);
			}
		}
	}
}
